using Google.OrTools.LinearSolver;
using NutrientOptimizer.Core;
using NutrientOptimizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Math;

/// <summary>
/// Optimizes nutrient recipes by solving a linear program for required ions.
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
        // Step 1: Get ion demands (non-null targets)
        var ionDemands = ExtractIonDemands(_plantProfile);
        if (ionDemands.Count == 0)
        {
            return FailResult("No ion targets with non-null demands in plant profile");
        }

        // Step 2: Tag ions and analyze availability
        var ionTagging = TagAvailableIons(ionDemands);
        if (ionTagging.NeededIons.Count == 0)
        {
            return FailResult("No required ions can be supplied by available salts");
        }

        // Step 3: Solve for needed ions
        var solverResult = SolveForIons(ionTagging);
        if (!solverResult.Success)
        {
            return solverResult;
        }

        // Step 4: Calculate complete ion results
        var ionResults = CalculateCompleteIonResults(solverResult.SaltAmounts);

        // Step 5: Build comprehensive result
        return BuildOptimizationResult(solverResult, ionResults, ionDemands, ionTagging);
    }

    /// <summary>
    /// Step 1: Extract ALL ion targets (not just those with TargetPpm)
    /// </summary>
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

    /// <summary>
    /// Step 2: Tag ions as needed/unnecessary and check availability
    /// </summary>
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
    /// Step 3: Solve linear program for needed ions
    /// </summary>
    private OptimizationResult SolveForIons(IonTagging ionTagging)
    {
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null)
            return FailResult("SCIP solver not available");

        // Variables: grams per liter of each salt
        var variables = _availableSalts.ToDictionary(
            salt => salt,
            salt => solver.MakeNumVar(0.0, double.PositiveInfinity, salt.Name));

        // Constraints: bounds for needed ions
        foreach (var demand in ionTagging.NeededIons)
        {
            var constraint = solver.MakeConstraint(demand.MinPpm, demand.MaxPpm, $"Bound_{demand.Ion}");

            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(demand.Ion, out double massPerMole))
                {
                    double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                    constraint.SetCoefficient(variables[salt], coeff);
                }
            }
        }

        // Objective: minimize deviation from targets
        Objective objective = solver.Objective();
        objective.SetMinimization();

        foreach (var demand in ionTagging.NeededIons)
        {
            LinearExpr actualExpr = new LinearExpr();

            foreach (var salt in _availableSalts)
            {
                if (salt.IonContributions.TryGetValue(demand.Ion, out double massPerMole))
                {
                    double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                    actualExpr += variables[salt] * coeff;
                }
            }

            var slackPos = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_pos_{demand.Ion}");
            var slackNeg = solver.MakeNumVar(0.0, double.PositiveInfinity, $"dev_neg_{demand.Ion}");

            solver.Add(actualExpr + slackPos - slackNeg == demand.TargetPpm);

            objective.SetCoefficient(slackPos, 1.0);
            objective.SetCoefficient(slackNeg, 1.0);
        }

        var status = solver.Solve();
        bool success = status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE;

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
            ReasonForTermination = status.ToString()
        };

        if (!success)
        {
            result.InfeasibilityReasons.Add($"Solver status: {status}");
            foreach (var ion in ionTagging.UnsupplyableIons)
            {
                result.InfeasibilityReasons.Add($"  ✗ {ion.Ion}: no source available");
            }
        }

        return result;
    }

    /// <summary>
    /// Step 4: Calculate complete ion concentrations from salt amounts
    /// </summary>
    private IonResults CalculateCompleteIonResults(Dictionary<Salt, double> saltAmounts)
    {
        var results = new IonResults();

        foreach (var salt in saltAmounts.Keys)
        {
            double gramsPerLiter = saltAmounts[salt];

            foreach (var (ion, massPerMole) in salt.IonContributions)
            {
                double coeff = massPerMole * 1000.0 / salt.MolecularWeight;
                double ppm = gramsPerLiter * coeff;

                if (!results.IonConcentrations.ContainsKey(ion))
                    results.IonConcentrations[ion] = 0;

                results.IonConcentrations[ion] += ppm;
            }
        }

        return results;
    }

    /// <summary>
    /// Step 5: Build comprehensive result with deltas and comparisons
    /// </summary>
    private OptimizationResult BuildOptimizationResult(
        OptimizationResult solverResult,
        IonResults ionResults,
        List<IonDemand> demands,
        IonTagging ionTagging)
    {
        solverResult.UnusedSalts = _availableSalts
            .Where(s => !solverResult.SaltAmounts.ContainsKey(s) || solverResult.SaltAmounts[s] < 1e-6)
            .ToList();

        // Add comparison data
        foreach (var demand in demands)
        {
            var actual = ionResults.IonConcentrations.GetValueOrDefault(demand.Ion, 0.0);
            var delta = actual - demand.TargetPpm;
            var inRange = actual >= demand.MinPpm - 1e-3 && actual <= demand.MaxPpm + 1e-3;

            solverResult.IonComparisons.Add(new IonComparison
            {
                Ion = demand.Ion,
                TargetPpm = demand.TargetPpm,
                ActualPpm = actual,
                DeltaPpm = delta,
                InRange = inRange
            });
        }

        // Add unsupplyable ions to result
        foreach (var ion in ionTagging.UnsupplyableIons)
        {
            solverResult.InfeasibilityReasons.Add($"  ✗ {ion.Ion}: target {ion.TargetPpm} ppm (no source)");
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

/// <summary>
/// Represents an ion demand from the plant profile
/// </summary>
public class IonDemand
{
    public Ion Ion { get; set; }
    public double TargetPpm { get; set; }
    public double MinPpm { get; set; }
    public double MaxPpm { get; set; }
}

/// <summary>
/// Tags ions as needed or unsupplyable
/// </summary>
public class IonTagging
{
    public List<IonDemand> NeededIons { get; } = new();
    public List<IonDemand> UnsupplyableIons { get; } = new();
}

/// <summary>
/// Calculated ion concentrations
/// </summary>
public class IonResults
{
    public Dictionary<Ion, double> IonConcentrations { get; } = new();
}