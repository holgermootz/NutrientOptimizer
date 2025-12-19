using Google.OrTools.LinearSolver;
using NutrientOptimizer.Core;
using NutrientOptimizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Math;

/// <summary>
/// Optimizes nutrient recipes by solving a linear program for required ions.
/// Always returns the closest possible solution (soft bounds on min/max, hard priority on targets).
/// </summary>
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
        var ionDemands = ExtractIonDemands(_plantProfile);
        if (ionDemands.Count == 0)
            return FailResult("No ion targets in plant profile");

        var ionTagging = TagAvailableIons(ionDemands);

        var solverResult = SolveWithSoftConstraints(ionTagging);

        var ionResults = CalculateCompleteIonResults(solverResult.SaltAmounts);

        return BuildOptimizationResult(solverResult, ionResults, ionDemands, ionTagging);
    }

    private List<IonDemand> ExtractIonDemands(PlantProfile profile)
    {
        return profile.IonTargets
            .Select(t => new IonDemand
            {
                Ion = t.Ion,
                TargetPpm = t.TargetPpm ?? ((t.MinPpm + t.MaxPpm) / 2.0),
                MinPpm = t.MinPpm,
                MaxPpm = t.MaxPpm
            })
            .ToList();
    }

    private IonTagging TagAvailableIons(List<IonDemand> demands)
    {
        var tagging = new IonTagging();
        var suppliedIons = _availableSalts
            .SelectMany(s => s.IonContributions.Keys)
            .ToHashSet();

        foreach (var demand in demands)
        {
            if (suppliedIons.Contains(demand.Ion))
                tagging.NeededIons.Add(demand);
            else
                tagging.UnsupplyableIons.Add(demand);
        }
        return tagging;
    }

    /// <summary>
    /// Single solver using soft bounds and weighted target deviations.
    /// Always feasible (unless no salts at all) and returns the closest possible recipe.
    /// </summary>
    private OptimizationResult SolveWithSoftConstraints(IonTagging ionTagging)
    {
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
            return FailResult("SCIP solver not available");

        // Variables: grams per liter of each salt
        var variables = _availableSalts.ToDictionary(
            salt => salt,
            salt => solver.MakeNumVar(0.0, double.PositiveInfinity, salt.Name));

        // Build expression for every supplyable ion
        var ionExpressions = new Dictionary<Ion, LinearExpr>();
        foreach (var demand in ionTagging.NeededIons)
        {
            var expr = new LinearExpr();
            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(demand.Ion, out double massPerMole))
                {
                    double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                    expr += variables[salt] * coeff;
                }
            }
            ionExpressions[demand.Ion] = expr;
        }

        // Objective weights
        const double targetWeight = 1.0;   // Primary goal: hit targets
        const double boundWeight = 0.1;   // Secondary: respect min/max as much as possible

        Objective objective = solver.Objective();
        objective.SetMinimization();

        foreach (var demand in ionTagging.NeededIons)
        {
            var expr = ionExpressions[demand.Ion];

            // 1. Target deviation (high priority)
            var devPos = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_pos_{demand.Ion}");
            var devNeg = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_neg_{demand.Ion}");
            solver.Add(expr + devPos - devNeg == demand.TargetPpm);
            objective.SetCoefficient(devPos, targetWeight);
            objective.SetCoefficient(devNeg, targetWeight);

            // 2. Bound violations (low priority)
            var under = solver.MakeNumVar(0.0, double.PositiveInfinity, $"under_{demand.Ion}");
            var over = solver.MakeNumVar(0.0, double.PositiveInfinity, $"over_{demand.Ion}");
            solver.Add(expr >= demand.MinPpm - under);
            solver.Add(expr <= demand.MaxPpm + over);
            objective.SetCoefficient(under, boundWeight);
            objective.SetCoefficient(over, boundWeight);
        }

        var status = solver.Solve();

        // Extract solution
        var amounts = new Dictionary<Salt, double>();
        foreach (var kv in variables)
        {
            double value = kv.Value.SolutionValue();
            if (value > 1e-6)
                amounts[kv.Key] = value;
        }

        var result = new OptimizationResult
        {
            Success = true, // We always have a mathematical solution
            SaltAmounts = amounts,
            ReasonForTermination = status.ToString(),
            IsApproximateSolution = ionTagging.UnsupplyableIons.Any() || status != Solver.ResultStatus.OPTIMAL
        };

        if (ionTagging.UnsupplyableIons.Any())
        {
            result.InfeasibilityReasons.Add("=== UNSUPPLYABLE IONS (fixed at 0 ppm) ===");
            foreach (var ion in ionTagging.UnsupplyableIons)
            {
                result.InfeasibilityReasons.Add($" ✗ {ion.Ion}: target {ion.TargetPpm:F1} ppm → 0 ppm (no source)");
            }
        }

        result.InfeasibilityReasons.Add("Closest possible solution (bounds treated as soft constraints)");

        return result;
    }

    private IonResults CalculateCompleteIonResults(Dictionary<Salt, double> saltAmounts)
    {
        var results = new IonResults();
        foreach (var saltAmount in saltAmounts)
        {
            double gramsPerLiter = saltAmount.Value;
            foreach (var (ion, massPerMole) in saltAmount.Key.IonContributions)
            {
                double coeff = massPerMole * 1000.0 / saltAmount.Key.MolecularWeight;
                double ppm = gramsPerLiter * coeff;
                if (!results.IonConcentrations.TryGetValue(ion, out _))
                    results.IonConcentrations[ion] = 0;
                results.IonConcentrations[ion] += ppm;
            }
        }
        return results;
    }

    private OptimizationResult BuildOptimizationResult(
        OptimizationResult solverResult,
        IonResults ionResults,
        List<IonDemand> demands,
        IonTagging ionTagging)
    {
        // Unused salts
        solverResult.UnusedSalts = _availableSalts
            .Where(s => !solverResult.SaltAmounts.ContainsKey(s) || solverResult.SaltAmounts[s] < 1e-6)
            .ToList();

        // Ion comparisons (includes unsupplyable ions at 0 ppm)
        foreach (var demand in demands)
        {
            double actual = ionResults.IonConcentrations.GetValueOrDefault(demand.Ion, 0.0);
            solverResult.IonComparisons.Add(new IonComparison
            {
                Ion = demand.Ion,
                TargetPpm = demand.TargetPpm,
                ActualPpm = actual,
                DeltaPpm = actual - demand.TargetPpm,
                InRange = actual >= demand.MinPpm - 1e-3 && actual <= demand.MaxPpm + 1e-3
            });
        }

        return solverResult;
    }

    private OptimizationResult FailResult(string reason)
    {
        return new OptimizationResult
        {
            Success = false,
            ReasonForTermination = reason,
            InfeasibilityReasons = { reason }
        };
    }
}

// Helper classes (unchanged – kept here for completeness if file is standalone)
public class IonDemand
{
    public Ion Ion { get; set; }
    public double TargetPpm { get; set; }
    public double MinPpm { get; set; }
    public double MaxPpm { get; set; }
}

public class IonTagging
{
    public List<IonDemand> NeededIons { get; } = new();
    public List<IonDemand> UnsupplyableIons { get; } = new();
}

public class IonResults
{
    public Dictionary<Ion, double> IonConcentrations { get; } = new();
}