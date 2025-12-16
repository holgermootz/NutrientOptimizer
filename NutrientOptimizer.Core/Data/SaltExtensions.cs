using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Core.Data;

/// <summary>
/// Extension methods for working with Salt objects and substance data
/// </summary>
public static class SaltExtensions
{
    /// <summary>
    /// Get the total macronutrient content (as percentage sum)
    /// </summary>
    public static double GetMacronutrientTotal(this Salt salt)
    {
        var macroNutrients = new[] 
        { 
            Ion.Nitrate, Ion.Ammonium, Ion.Phosphate, Ion.Potassium, 
            Ion.Calcium, Ion.Magnesium, Ion.Sulfate 
        };

        return salt.IonContributions
            .Where(kvp => macroNutrients.Contains(kvp.Key))
            .Sum(kvp => kvp.Value);
    }

    /// <summary>
    /// Get the total micronutrient content (as ppm sum)
    /// </summary>
    public static double GetMicronutrientTotal(this Salt salt)
    {
        var microNutrients = new[] 
        { 
            Ion.Iron, Ion.Manganese, Ion.Zinc, Ion.Copper, Ion.Boron, Ion.Molybdenum 
        };

        return salt.IonContributions
            .Where(kvp => microNutrients.Contains(kvp.Key))
            .Sum(kvp => kvp.Value);
    }

    /// <summary>
    /// Check if this salt is a primary nitrogen source (>5%)
    /// </summary>
    public static bool IsPrimaryNitrogenSource(this Salt salt)
    {
        return salt.IonContributions.TryGetValue(Ion.Nitrate, out var n1) && n1 > 5 ||
               salt.IonContributions.TryGetValue(Ion.Ammonium, out var n2) && n2 > 5;
    }

    /// <summary>
    /// Check if this salt is a primary potassium source (>15%)
    /// </summary>
    public static bool IsPrimaryPotassiumSource(this Salt salt)
    {
        return salt.IonContributions.TryGetValue(Ion.Potassium, out var k) && k > 15;
    }

    /// <summary>
    /// Check if this salt is a primary phosphorus source (>8%)
    /// </summary>
    public static bool IsPrimaryPhosphorusSource(this Salt salt)
    {
        return salt.IonContributions.TryGetValue(Ion.Phosphate, out var p) && p > 8;
    }

    /// <summary>
    /// Get the primary nutrient (the one with highest contribution)
    /// </summary>
    public static Ion? GetPrimaryNutrient(this Salt salt)
    {
        return salt.IonContributions.Count == 0 
            ? null 
            : salt.IonContributions.MaxBy(kvp => kvp.Value).Key;
    }

    /// <summary>
    /// Compare two salts by their ion content similarity
    /// Returns a score from 0 (completely different) to 1 (identical)
    /// </summary>
    public static double SimilarityScore(this Salt salt1, Salt salt2)
    {
        if (salt1.IonContributions.Count == 0 && salt2.IonContributions.Count == 0)
            return 1.0;

        if (salt1.IonContributions.Count == 0 || salt2.IonContributions.Count == 0)
            return 0.0;

        var ions1 = new HashSet<Ion>(salt1.IonContributions.Keys);
        var ions2 = new HashSet<Ion>(salt2.IonContributions.Keys);

        var intersection = ions1.Intersect(ions2);
        var union = ions1.Union(ions2);

        // Jaccard similarity
        return (double)intersection.Count() / union.Count();
    }

    /// <summary>
    /// Format ion contribution as a readable string
    /// </summary>
    public static string FormatComposition(this Salt salt)
    {
        if (salt.IonContributions.Count == 0)
            return "No ion contributions";

        var parts = salt.IonContributions
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => $"{kvp.Key}: {kvp.Value:F2}")
            .ToList();

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Calculate the ionic ratio (K:Ca:Mg ratio commonly used in hydroponics)
    /// </summary>
    public static (double K, double Ca, double Mg) GetMacroRatio(this Salt salt)
    {
        var k = salt.IonContributions.GetValueOrDefault(Ion.Potassium, 0);
        var ca = salt.IonContributions.GetValueOrDefault(Ion.Calcium, 0);
        var mg = salt.IonContributions.GetValueOrDefault(Ion.Magnesium, 0);

        // Normalize by calcium (if Ca exists)
        if (ca > 0)
        {
            return (k / ca, 1.0, mg / ca);
        }

        return (k, ca, mg);
    }
}

/// <summary>
/// Extension methods for collections of salts
/// </summary>
public static class SaltCollectionExtensions
{
    /// <summary>
    /// Group salts by their primary nutrient
    /// </summary>
    public static Dictionary<Ion?, List<Salt>> GroupByPrimaryNutrient(this IEnumerable<Salt> salts)
    {
        return salts
            .GroupBy(s => s.GetPrimaryNutrient())
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Filter salts that contain all specified ions
    /// </summary>
    public static List<Salt> ContainingAllIons(this IEnumerable<Salt> salts, params Ion[] ions)
    {
        var ionSet = new HashSet<Ion>(ions);
        return salts
            .Where(s => ionSet.IsSubsetOf(s.IonContributions.Keys))
            .ToList();
    }

    /// <summary>
    /// Filter salts that contain any of the specified ions
    /// </summary>
    public static List<Salt> ContainingAnyIon(this IEnumerable<Salt> salts, params Ion[] ions)
    {
        var ionSet = new HashSet<Ion>(ions);
        return salts
            .Where(s => s.IonContributions.Keys.Intersect(ionSet).Any())
            .ToList();
    }

    /// <summary>
    /// Get total ion contribution across all salts
    /// </summary>
    public static Dictionary<Ion, double> GetTotalComposition(this IEnumerable<Salt> salts)
    {
        var result = new Dictionary<Ion, double>();

        foreach (var salt in salts)
        {
            foreach (var (ion, contribution) in salt.IonContributions)
            {
                if (result.ContainsKey(ion))
                    result[ion] += contribution;
                else
                    result[ion] = contribution;
            }
        }

        return result;
    }

    /// <summary>
    /// Find salts most similar to a reference salt
    /// </summary>
    public static List<Salt> FindSimilar(
        this IEnumerable<Salt> salts, 
        Salt referenceSalt, 
        int maxResults = 5,
        double minSimilarity = 0.5)
    {
        return salts
            .Select(s => new { Salt = s, Score = s.SimilarityScore(referenceSalt) })
            .Where(x => x.Score >= minSimilarity && x.Salt.Name != referenceSalt.Name)
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .Select(x => x.Salt)
            .ToList();
    }

    /// <summary>
    /// Get statistics about a collection of salts
    /// </summary>
    public static SaltCollectionStats GetStatistics(this IEnumerable<Salt> salts)
    {
        var saltList = salts.ToList();
        var allIons = saltList
            .SelectMany(s => s.IonContributions.Keys)
            .Distinct()
            .ToList();

        var avgMacroContent = saltList
            .Average(s => s.GetMacronutrientTotal());

        var avgMicroContent = saltList
            .Average(s => s.GetMicronutrientTotal());

        return new SaltCollectionStats
        {
            Count = saltList.Count,
            DistinctIons = allIons.Count,
            AverageMacronutrientContent = avgMacroContent,
            AverageMicronutrientContent = avgMicroContent,
            MostCommonIons = allIons
                .Select(ion => new { Ion = ion, Count = saltList.Count(s => s.IonContributions.ContainsKey(ion)) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Select(x => x.Ion)
                .ToList()
        };
    }
}

/// <summary>
/// Statistics for a collection of salts
/// </summary>
public class SaltCollectionStats
{
    public int Count { get; set; }
    public int DistinctIons { get; set; }
    public double AverageMacronutrientContent { get; set; }
    public double AverageMicronutrientContent { get; set; }
    public List<Ion> MostCommonIons { get; set; } = new();

    public override string ToString()
    {
        var lines = new List<string>
        {
            $"Collection Statistics:",
            $"  Count: {Count}",
            $"  Distinct Ions: {DistinctIons}",
            $"  Avg Macronutrient Content: {AverageMacronutrientContent:F2}%",
            $"  Avg Micronutrient Content: {AverageMicronutrientContent:F4}%",
            $"  Most Common Ions: {string.Join(", ", MostCommonIons)}"
        };

        return string.Join(Environment.NewLine, lines);
    }
}
