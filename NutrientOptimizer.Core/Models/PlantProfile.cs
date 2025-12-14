namespace NutrientOptimizer.Core.Models;

public class PlantProfile
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of ion targets (one per relevant ion)
    /// </summary>
    public List<IonTarget> IonTargets { get; set; } = new();

    /// <summary>
    /// Optional: EC range in mS/cm (if you want to constrain total salts later)
    /// </summary>
    public double? MinEC { get; set; }
    public double? MaxEC { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{Name} - {Description}");
        foreach (var target in IonTargets)
            sb.AppendLine("  " + target.ToString());

        if (MinEC.HasValue || MaxEC.HasValue)
            sb.AppendLine($"  EC: {MinEC:F2} – {MaxEC:F2} mS/cm");

        return sb.ToString();
    }

    /// <summary>
    /// Helper to quickly get target for a specific ion (returns null if not defined)
    /// </summary>
    public IonTarget? GetTarget(Ion ion) => IonTargets.FirstOrDefault(t => t.Ion == ion);
}