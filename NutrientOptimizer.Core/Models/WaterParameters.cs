namespace NutrientOptimizer.Core.Models;

/// <summary>
/// Represents water parameters (baseline ion concentrations in local water supply)
/// </summary>
public class WaterParameters
{
    /// <summary>
    /// Nitrate concentration in ppm
    /// </summary>
    public double Nitrate { get; set; }

    /// <summary>
    /// Calcium concentration in ppm
    /// </summary>
    public double Calcium { get; set; }

    /// <summary>
    /// Magnesium concentration in ppm
    /// </summary>
    public double Magnesium { get; set; }

    /// <summary>
    /// Potassium concentration in ppm
    /// </summary>
    public double Potassium { get; set; }

    /// <summary>
    /// Sulfur (Sulfate) concentration in ppm
    /// </summary>
    public double Sulfur { get; set; }

    /// <summary>
    /// Get total dissolved solids (approximation)
    /// </summary>
    public double GetTotalDissolved() => Nitrate + Calcium + Magnesium + Potassium + Sulfur;

    /// <summary>
    /// Create a default instance with zero values
    /// </summary>
    public static WaterParameters CreateDefault() => new()
    {
        Nitrate = 0,
        Calcium = 0,
        Magnesium = 0,
        Potassium = 0,
        Sulfur = 0
    };

    public override string ToString()
    {
        return $"NO??: {Nitrate:F1} | Ca²?: {Calcium:F1} | Mg²?: {Magnesium:F1} | K?: {Potassium:F1} | SO?²?: {Sulfur:F1}";
    }
}
