namespace NutrientOptimizer.Core;

public class IonTarget
{
    public Ion Ion { get; set; }

    /// <summary>
    /// Desired target concentration in ppm (parts per million, mg/L).
    /// Can be null if no specific target, only min/max matter.
    /// </summary>
    public double? TargetPpm { get; set; }

    /// <summary>
    /// Minimum acceptable concentration in ppm (inclusive).
    /// </summary>
    public double MinPpm { get; set; }

    /// <summary>
    /// Maximum acceptable concentration in ppm (inclusive).
    /// </summary>
    public double MaxPpm { get; set; }

    public IonTarget(Ion ion, double minPpm, double maxPpm, double? targetPpm = null)
    {
        Ion = ion;
        MinPpm = minPpm;
        MaxPpm = maxPpm;
        TargetPpm = targetPpm;
    }

    public override string ToString()
    {
        return TargetPpm.HasValue
            ? $"{Ion}: {MinPpm} – {MaxPpm} ppm (target {TargetPpm} ppm)"
            : $"{Ion}: {MinPpm} – {MaxPpm} ppm";
    }
}