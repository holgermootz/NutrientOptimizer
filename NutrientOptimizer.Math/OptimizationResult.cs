using NutrientOptimizer.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NutrientOptimizer.Math;

public class IonComparison
{
    public Ion Ion { get; set; }
    public double TargetPpm { get; set; }
    public double ActualPpm { get; set; }
    public double DeltaPpm { get; set; }
    public bool InRange { get; set; }
}

public class OptimizationResult
{
    public bool Success { get; set; }
    public Dictionary<Salt, double> SaltAmounts { get; set; } = new();
    public List<Salt> UnusedSalts { get; set; } = new();
    public string ReasonForTermination { get; set; } = string.Empty;
    public double FinalError { get; set; }
    public List<string> InfeasibilityReasons { get; set; } = new();
    public List<IonComparison> IonComparisons { get; set; } = new();

    public Recipe ToRecipe()
    {
        var recipe = new Recipe();
        foreach (var kv in SaltAmounts)
        {
            if (kv.Value > 1e-6) // filter numerical noise
                recipe.AddSalt(kv.Key, System.Math.Round(kv.Value, 5));
        }
        return recipe;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (!Success)
        {
            sb.AppendLine($"=== OPTIMIZATION FAILED: {ReasonForTermination} ===\n");

            if (InfeasibilityReasons.Any())
            {
                foreach (var reason in InfeasibilityReasons)
                    sb.AppendLine(reason);
            }

            if (SaltAmounts.Any() || UnusedSalts.Any())
            {
                sb.AppendLine("\nAvailable salts:");
                foreach (var salt in SaltAmounts.Keys.Concat(UnusedSalts).OrderBy(s => s.Name))
                {
                    string status = SaltAmounts.ContainsKey(salt) && SaltAmounts[salt] > 1e-6 ? "✓" : "✗";
                    sb.AppendLine($"  [{status}] {salt.Name}");
                }
            }

            return sb.ToString();
        }

        sb.AppendLine("=== OPTIMIZATION SUCCESS ===\n");

        sb.AppendLine("Recipe (g/L):");
        if (SaltAmounts.Any())
        {
            foreach (var kv in SaltAmounts.OrderBy(k => k.Key.Name))
                sb.AppendLine($"  {kv.Key.Name}: {kv.Value:F4} g/L");
        }
        else
        {
            sb.AppendLine("  (No salts required)");
        }

        sb.AppendLine("\nIon Concentrations:");
        foreach (var comp in IonComparisons.OrderBy(c => c.Ion.ToString()))
        {
            string status = comp.InRange ? "✓" : "✗";
            sb.AppendLine($"  [{status}] {comp.Ion}: {comp.ActualPpm:F1} ppm (target: {comp.TargetPpm:F1}, delta: {comp.DeltaPpm:+0.0;-0.0})");
        }

        return sb.ToString();
    }
}