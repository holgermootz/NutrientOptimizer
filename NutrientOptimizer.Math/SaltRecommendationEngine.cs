using NutrientOptimizer.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Math;

/// <summary>
/// Recommends specific salts to add to make an infeasible optimization feasible
/// </summary>
public class SaltRecommendationEngine
{
    private readonly IReadOnlyList<Salt> _availableSalts;
    private readonly IReadOnlyList<Salt> _selectedSalts;
    private readonly List<IonDemand> _ionDemands;

    public SaltRecommendationEngine(
        IReadOnlyList<Salt> availableSalts,
        IReadOnlyList<Salt> selectedSalts,
        List<IonDemand> ionDemands)
    {
        _availableSalts = availableSalts;
        _selectedSalts = selectedSalts;
        _ionDemands = ionDemands;
    }

    /// <summary>
    /// Generate concrete salt recommendations
    /// </summary>
    public List<SaltRecommendation> GetRecommendations()
    {
        var recommendations = new List<SaltRecommendation>();

        // 1. Find missing ion sources
        var suppliedIons = _selectedSalts
            .SelectMany(s => s.IonContributions.Keys)
            .ToHashSet();

        var missingIons = _ionDemands
            .Where(d => !suppliedIons.Contains(d.Ion))
            .ToList();

        // For each missing ion, recommend the best salt
        foreach (var missing in missingIons)
        {
            var bestSalts = FindBestSaltsFor(missing.Ion);
            if (bestSalts.Any())
            {
                recommendations.Add(new SaltRecommendation
                {
                    Reason = $"Provides {missing.Ion} (required {missing.TargetPpm:F0} ppm)",
                    RecommendedSalts = bestSalts,
                    Priority = 1  // Critical - missing source
                });
            }
        }

        // 2. If all ions are supplied, look for ratio/flexibility issues
        if (!recommendations.Any() && _selectedSalts.Count < 4)
        {
            var diversifySalts = FindDiversifyingSalts();
            if (diversifySalts.Any())
            {
                recommendations.Add(new SaltRecommendation
                {
                    Reason = "Adds flexibility for ratio balancing",
                    RecommendedSalts = diversifySalts,
                    Priority = 2  // Helpful - improves flexibility
                });
            }
        }

        return recommendations.OrderBy(r => r.Priority).ToList();
    }

    /// <summary>
    /// Find the best salts that provide a specific ion
    /// </summary>
    private List<SaltOption> FindBestSaltsFor(Ion targetIon)
    {
        var candidates = _availableSalts
            .Where(s => !_selectedSalts.Contains(s) && s.IonContributions.ContainsKey(targetIon))
            .ToList();

        // Score each candidate
        var scored = candidates
            .Select(s => new SaltOption
            {
                Salt = s,
                IonContent = s.IonContributions[targetIon],
                SecondaryBenefits = s.IonContributions.Count - 1,
                Score = CalculateScore(s, targetIon)
            })
            .OrderByDescending(x => x.Score)
            .Take(3)  // Top 3 recommendations
            .ToList();

        return scored;
    }

    /// <summary>
    /// Find salts that provide complementary ions for better ratio balance
    /// </summary>
    private List<SaltOption> FindDiversifyingSalts()
    {
        var suppliedIons = _selectedSalts
            .SelectMany(s => s.IonContributions.Keys)
            .ToHashSet();

        // Find salts that provide ions already needed but from different sources
        var candidates = _availableSalts
            .Where(s => !_selectedSalts.Contains(s) && 
                        s.IonContributions.Keys.Any(ion => suppliedIons.Contains(ion)))
            .ToList();

        var scored = candidates
            .Select(s => new SaltOption
            {
                Salt = s,
                IonContent = s.IonContributions.Values.Max(),
                SecondaryBenefits = s.IonContributions.Count,
                Score = CalculateScore(s, null)
            })
            .OrderByDescending(x => x.Score)
            .Take(2)
            .ToList();

        return scored;
    }

    /// <summary>
    /// Score a salt based on relevance and multi-ion contribution
    /// </summary>
    private double CalculateScore(Salt salt, Ion? targetIon)
    {
        double score = 0;

        if (targetIon.HasValue && salt.IonContributions.TryGetValue(targetIon.Value, out var primary))
        {
            score += primary * 10;  // Primary contribution weight
        }

        // Bonus for multi-ion salts (better for filling multiple needs)
        score += salt.IonContributions.Count * 2;

        // Bonus for macronutrients
        var macros = new[] { Ion.Nitrate, Ion.Ammonium, Ion.Potassium, Ion.Calcium, Ion.Magnesium, Ion.Phosphate, Ion.Sulfate };
        var macroCount = salt.IonContributions.Keys.Count(i => macros.Contains(i));
        score += macroCount * 3;

        return score;
    }
}

/// <summary>
/// Represents a recommendation to add specific salts
/// </summary>
public class SaltRecommendation
{
    public string Reason { get; set; } = "";
    public List<SaltOption> RecommendedSalts { get; set; } = new();
    public int Priority { get; set; }  // 1 = Critical, 2 = Helpful

    public override string ToString()
    {
        if (!RecommendedSalts.Any())
            return Reason;

        var saltsStr = string.Join(" or ", 
            RecommendedSalts.Select(s => $"{s.Salt.Name} ({s.Salt.Formula})"));
        
        return $"{Reason}\n  ? {saltsStr}";
    }
}

/// <summary>
/// A specific salt option with its score
/// </summary>
public class SaltOption
{
    public Salt Salt { get; set; } = null!;
    public double IonContent { get; set; }
    public int SecondaryBenefits { get; set; }
    public double Score { get; set; }

    public string GetDescription()
    {
        var ions = string.Join(", ", 
            Salt.IonContributions
                .OrderByDescending(x => x.Value)
                .Take(3)
                .Select(x => $"{x.Key}: {x.Value:F1}%"));
        
        return $"{Salt.Name} ({ions})";
    }
}
