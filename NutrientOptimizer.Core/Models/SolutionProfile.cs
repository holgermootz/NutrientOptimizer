namespace NutrientOptimizer.Core;

public class SolutionProfile
{
    public Dictionary<Ion, double> IonConcentrationsPpm { get; set; } = new();

    public double TotalDissolvedSolidsPpm => IonConcentrationsPpm.Values.Sum();

    public override string ToString()
    {
        return string.Join("\n", IonConcentrationsPpm
            .OrderBy(kv => kv.Key.ToString())
            .Select(kv => $"{kv.Key}: {kv.Value:F2} ppm"));
    }
}