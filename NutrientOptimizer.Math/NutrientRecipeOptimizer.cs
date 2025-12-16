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
        var supplyAnalysis = AnalyzeIonSupply();

        if (supplyAnalysis.SupplyableTargets.Count == 0)
        {
            return new OptimizationResult
            {
                Success = false,
                ReasonForTermination = "No controllable ions",
                InfeasibilityReasons = { "No available salts provide any of the required ions." }
            };
        }

        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null)
        {
            return new OptimizationResult
            {
                Success = false,
                ReasonForTermination = "SCIP solver not available."
            };
        }

        // Variables: grams per liter of each salt
        var variables = _availableSalts.ToDictionary(
            salt => salt,
            salt => solver.MakeNumVar(0.0, double.PositiveInfinity, salt.Name));

        // Constraints: only for ions we can actually supply
        foreach (var target in supplyAnalysis.SupplyableTargets)
        {
            var constraint = solver.MakeConstraint(target.MinPpm, target.MaxPpm, $"Bound_{target.Ion}");

            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(target.Ion, out double massPerMole))
                {
                    double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                    constraint.SetCoefficient(variables[salt], coeff);
                }
            }
        }

        // Objective: minimize total absolute deviation from targets
        Objective objective = solver.Objective();
        objective.SetMinimization(); // ← correct: no argument

        bool hasOptimizableTargets = false;

        foreach (var target in supplyAnalysis.SupplyableTargets)
        {
            if (!target.TargetPpm.HasValue)
                continue;

            hasOptimizableTargets = true;

            // Build linear expression for actual ppm of this ion
            LinearExpr actualExpr = new LinearExpr(); // starts at 0

            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(target.Ion, out double massPerMole))
                {
                    double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                    actualExpr += variables[salt] * coeff;
                }
            }

            // Slack variables for absolute deviation
            var slackPos = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_pos_{target.Ion}");
            var slackNeg = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_neg_{target.Ion}");

            // actual + pos - neg = target
            solver.Add(actualExpr + slackPos - slackNeg == target.TargetPpm.Value);

            // Minimize sum of slacks
            objective.SetCoefficient(slackPos, 1.0);
            objective.SetCoefficient(slackNeg, 1.0);
        }

        // Fallback: minimize total salt usage if no targets
        if (!hasOptimizableTargets)
        {
            foreach (var variable in variables.Values)
            {
                objective.SetCoefficient(variable, 0.0001);
            }
        }

        // Solve
        var resultStatus = solver.Solve();
        bool success = resultStatus == Solver.ResultStatus.OPTIMAL || resultStatus == Solver.ResultStatus.FEASIBLE;

        // Extract solution
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
            UnusedSalts = _availableSalts.Where(s => !amounts.ContainsKey(s) || amounts[s] < 1e-6).ToList(),
            ReasonForTermination = resultStatus.ToString(),
            FinalError = success ? objective.Value() : double.NaN
        };

        // Report ignored (unsupplyable) ions
        if (supplyAnalysis.UnsupplyableTargets.Any())
        {
            result.InfeasibilityReasons.Add("=== PARTIAL OPTIMIZATION ===");
            result.InfeasibilityReasons.Add("These required ions cannot be supplied and were ignored:");
            foreach (var t in supplyAnalysis.UnsupplyableTargets.Where(t => t.MinPpm > 0))
            {
                result.InfeasibilityReasons.Add($"  • {t.Ion}: ≥ {t.MinPpm} ppm (no source available)");
            }
            result.InfeasibilityReasons.Add("");
        }

        // Only detect ratio conflicts if optimization failed or was not optimal
        // If we have a successful solution, these "conflicts" are not real
        if (!success || resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            var conflictWarnings = DetectRatioConflicts(supplyAnalysis.SupplyableTargets, _availableSalts.ToList());
            if (conflictWarnings.Any())
            {
                result.InfeasibilityReasons.Add("=== RATIO CONFLICTS DETECTED ===");
                result.InfeasibilityReasons.AddRange(conflictWarnings);
            }
        }

        return result;
    }

    private (List<IonTarget> SupplyableTargets, List<IonTarget> UnsupplyableTargets) AnalyzeIonSupply()
    {
        var suppliedIons = _availableSalts
            .SelectMany(s => s.IonContributions.Keys)
            .ToHashSet();

        var supplyable = new List<IonTarget>();
        var unsupplyable = new List<IonTarget>();

        foreach (var target in _plantProfile.IonTargets)
        {
            if (suppliedIons.Contains(target.Ion))
                supplyable.Add(target);
            else
                unsupplyable.Add(target);
        }

        return (supplyable, unsupplyable);
    }

    private List<string> DetectRatioConflicts(List<IonTarget> targets, List<Salt> salts)
    {
        var warnings = new List<string>();

        foreach (var victimTarget in targets)
        {
            var victimIon = victimTarget.Ion;

            // Unavoidable minimum (when trying to maximize other ions / satisfy their mins)
            double unavoidableMin = ComputeExtreme(victimIon, minimize: false, targets, salts);
            if (unavoidableMin > victimTarget.MaxPpm + 1e-3)
            {
                warnings.Add($"{victimIon} will always exceed {victimTarget.MaxPpm:F1} ppm " +
                             $"(unavoidable ≥ {unavoidableMin:F1} ppm) when meeting other requirements.");
            }

            // Unavoidable maximum
            double unavoidableMax = ComputeExtreme(victimIon, minimize: true, targets, salts);
            if (unavoidableMax < victimTarget.MinPpm - 1e-3)
            {
                warnings.Add($"{victimIon} will always be below {victimTarget.MinPpm:F1} ppm " +
                             $"(unavoidable ≤ {unavoidableMax:F1} ppm) when meeting other requirements.");
            }
        }

        // Remove warnings if they don't actually prevent a solution
        // (i.e., if the optimal solution respects the bounds)
        return warnings;
    }

    private double ComputeExtreme(Ion targetIon, bool minimize, List<IonTarget> targets, List<Salt> salts)
    {
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null) return minimize ? double.PositiveInfinity : 0;

        var variables = salts.ToDictionary(s => s, s => solver.MakeNumVar(0.0, double.PositiveInfinity, s.Name));

        // Enforce bounds for all ions EXCEPT the target one
        foreach (var t in targets.Where(t => t.Ion != targetIon))
        {
            var c = solver.MakeConstraint(t.MinPpm, t.MaxPpm);
            foreach (var salt in salts)
            {
                if (salt.IonContributions.TryGetValue(t.Ion, out double m))
                {
                    double coeff = m * 1000.0 / salt.MolecularWeight;
                    c.SetCoefficient(variables[salt], coeff);
                }
            }
        }

        // Build objective: minimize or maximize the target ion concentration
        Objective obj = solver.Objective();
        if (minimize)
            obj.SetMinimization();
        else
            obj.SetMaximization();

        // Add each contributing variable to the objective with proper coefficient
        foreach (var salt in salts)
        {
            if (salt.IonContributions.TryGetValue(targetIon, out double m))
            {
                double coeff = m * 1000.0 / salt.MolecularWeight;
                obj.SetCoefficient(variables[salt], coeff);
            }
        }

        var status = solver.Solve();

        if (status != Solver.ResultStatus.OPTIMAL && status != Solver.ResultStatus.FEASIBLE)
            return minimize ? double.PositiveInfinity : 0;

        // Compute the actual value of the target ion from the solution
        double value = 0.0;
        foreach (var salt in salts)
        {
            if (salt.IonContributions.TryGetValue(targetIon, out double m))
            {
                double coeff = m * 1000.0 / salt.MolecularWeight;
                value += variables[salt].SolutionValue() * coeff;
            }
        }

        return value;
    }
}