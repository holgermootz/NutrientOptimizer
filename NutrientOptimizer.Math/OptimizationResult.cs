using NutrientOptimizer.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NutrientOptimizer.Math;

public class OptimizationResult
{
    public bool Success { get; set; }
    public Dictionary<Salt, double> SaltAmounts { get; set; } = new();
    public List<Salt> UnusedSalts { get; set; } = new();
    public string ReasonForTermination { get; set; } = string.Empty;
    public double FinalError { get; set; }
    public List<string> InfeasibilityReasons { get; set; } = new();

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
            sb.AppendLine($"=== OPTIMIZATION FAILED: {ReasonForTermination} ===");

            if (InfeasibilityReasons.Any())
            {
                sb.AppendLine("\nWhy the optimization failed:");
                foreach (var reason in InfeasibilityReasons)
                    sb.AppendLine($"  • {reason}");
            }
            else
            {
                sb.AppendLine("\nNo specific ion conflicts detected (possibly numerical issues or unbounded model).");
            }

            sb.AppendLine("\nAvailable salts were:");
            foreach (var salt in SaltAmounts.Keys.Concat(UnusedSalts).OrderBy(s => s.Name))
            {
                string status = SaltAmounts.ContainsKey(salt) && SaltAmounts[salt] > 1e-6 ? " (used)" : " (not used)";
                sb.AppendLine($"  - {salt.Name} ({salt.Formula}){status}");
            }

            return sb.ToString();
        }

        // Success case
        sb.AppendLine("=== OPTIMAL RECIPE FOUND ===");
        sb.AppendLine("Recipe (g/L):");
        if (SaltAmounts.Any())
        {
            foreach (var kv in SaltAmounts.OrderBy(k => k.Key.Name))
                sb.AppendLine($"  {kv.Key.Name}: {kv.Value:F5} g/L");
        }
        else
        {
            sb.AppendLine("  No salts required (all targets met with zero input).");
        }

        if (UnusedSalts.Any())
        {
            sb.AppendLine("\nSalts not used:");
            foreach (var salt in UnusedSalts.OrderBy(s => s.Name))
                sb.AppendLine($"  - {salt.Name} ({salt.Formula})");
        }

        var solution = NutrientCalculator.CalculateSolution(ToRecipe());
        sb.AppendLine("\n=== RESULTING ION CONCENTRATIONS (ppm) ===");
        sb.Append(solution);

        sb.AppendLine($"\nTotal deviation from ideal targets: {FinalError:F4}");

        return sb.ToString();
    }
}