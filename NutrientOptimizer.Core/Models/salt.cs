namespace NutrientOptimizer.Core;

public class Salt
{
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public double MolecularWeight { get; set; }

    // Dictionary: Ion -> amount of that ion (in grams per mole of salt)
    public Dictionary<Ion, double> IonContributions { get; set; } = new();

    // Optional: water of crystallization factor if needed later
}