using NutrientOptimizer.Core.Models;

namespace NutrientOptimizer.Core;

public static class SolutionValidator
{
    public static bool IsWithinTargets(SolutionProfile solution, PlantProfile plantProfile)
    {
        foreach (var target in plantProfile.IonTargets)
        {
            if (!solution.IonConcentrationsPpm.TryGetValue(target.Ion, out double actualPpm))
                actualPpm = 0;

            if (actualPpm < target.MinPpm || actualPpm > target.MaxPpm)
                return false;
        }

        // Optional EC check later when we calculate it
        return true;
    }

    public static List<string> GetViolations(SolutionProfile solution, PlantProfile plantProfile)
    {
        var violations = new List<string>();

        foreach (var target in plantProfile.IonTargets)
        {
            solution.IonConcentrationsPpm.TryGetValue(target.Ion, out double actualPpm);
            if (actualPpm < target.MinPpm)
                violations.Add($"{target.Ion}: {actualPpm:F1} ppm (too low, min {target.MinPpm})");
            else if (actualPpm > target.MaxPpm)
                violations.Add($"{target.Ion}: {actualPpm:F1} ppm (too high, max {target.MaxPpm})");
        }

        return violations;
    }
}