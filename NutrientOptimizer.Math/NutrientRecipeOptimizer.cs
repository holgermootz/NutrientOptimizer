using Google.OrTools.LinearSolver;
using NutrientOptimizer.Core;
using NutrientOptimizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Math;

public class NutrientRecipeOptimizer
{
    private readonly IReadOnlyList<Salt> _availableSalts;
    private readonly PlantProfile _plantProfile;

    public NutrientRecipeOptimizer(IReadOnlyList<Salt> availableSalts, PlantProfile plantProfile)
    {
        _availableSalts = availableSalts ?? throw new ArgumentNullException(nameof(availableSalts));
        _plantProfile = plantProfile ?? throw new ArgumentNullException(nameof(plantProfile));
    }

    public OptimizationResult Solve()
    {
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null)
        {
            return new OptimizationResult
            {
                Success = false,
                ReasonForTermination = "SCIP solver not available."
            };
        }

        // Variables: grams per liter of each salt (>= 0)
        var variables = new Dictionary<Salt, Variable>();
        foreach (var salt in _availableSalts)
        {
            variables[salt] = solver.MakeNumVar(0.0, double.PositiveInfinity, salt.Name);
        }

        bool hasOptimizableTargets = false;

        // Constraints: min <= ion concentration <= max for every targeted ion
        foreach (var target in _plantProfile.IonTargets)
        {
            var constraint = solver.MakeConstraint(target.MinPpm, target.MaxPpm, target.Ion.ToString());

            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(target.Ion, out double ionMassPerMole))
                {
                    double ppmPerGramPerLiter = (ionMassPerMole * 1000.0) / salt.MolecularWeight;
                    constraint.SetCoefficient(variables[salt], ppmPerGramPerLiter);
                }
            }
        }

        // Objective: minimize sum of absolute deviations from targets
        Objective objective = solver.Objective();
        objective.SetMinimization();

        foreach (var target in _plantProfile.IonTargets)
        {
            if (!target.TargetPpm.HasValue)
                continue;

            bool canSupply = _availableSalts.Any(s => s.IonContributions.ContainsKey(target.Ion));
            if (!canSupply)
                continue;

            hasOptimizableTargets = true;

            LinearExpr actual = new LinearExpr();
            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(target.Ion, out double ionMassPerMole))
                {
                    double coeff = (ionMassPerMole * 1000.0) / salt.MolecularWeight;
                    actual += variables[salt] * coeff;
                }
            }

            var slackPos = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_pos_{target.Ion}");
            var slackNeg = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_neg_{target.Ion}");

            solver.Add(actual + slackPos - slackNeg == target.TargetPpm.Value);

            objective.SetCoefficient(slackPos, 1.0);
            objective.SetCoefficient(slackNeg, 1.0);
        }

        if (!hasOptimizableTargets)
        {
            // Minimize total salt usage if no real targets
            foreach (var v in variables.Values)
                objective.SetCoefficient(v, 0.0001);
        }

        // Solve
        var resultStatus = solver.Solve();

        bool success = resultStatus == Solver.ResultStatus.OPTIMAL || resultStatus == Solver.ResultStatus.FEASIBLE;

        var amounts = new Dictionary<Salt, double>();
        if (success)
        {
            foreach (var kv in variables)
            {
                double value = kv.Value.SolutionValue();
                if (value > 1e-6)
                    amounts[kv.Key] = value;
            }
        }

        var result = new OptimizationResult
        {
            Success = success,
            SaltAmounts = amounts,
            ReasonForTermination = resultStatus.ToString(),
            FinalError = success ? solver.Objective().Value() : double.NaN
        };

        // Populate unused salts
        result.UnusedSalts = _availableSalts
            .Where(s => !amounts.ContainsKey(s) || amounts[s] < 1e-6)
            .ToList();

        // === Rich infeasibility diagnostics ===
        if (resultStatus == Solver.ResultStatus.INFEASIBLE)
        {
            foreach (var target in _plantProfile.IonTargets)
            {
                bool hasSource = _availableSalts.Any(s => s.IonContributions.ContainsKey(target.Ion));

                if (!hasSource && target.MinPpm > 0)
                {
                    result.InfeasibilityReasons.Add(
                        $"{target.Ion}: Minimum {target.MinPpm} ppm required, but NO salt provides this ion.");
                }
                else if (hasSource && target.MinPpm > 0)
                {
                    // Optional: could add more advanced checks (e.g. unavoidable over-supply), but rare
                }
            }

            if (!result.InfeasibilityReasons.Any())
            {
                result.InfeasibilityReasons.Add(
                    "All required ions have sources, but constraints conflict (e.g., using one salt forces another ion outside its allowed range).");
            }
        }

        return result;
    }
}