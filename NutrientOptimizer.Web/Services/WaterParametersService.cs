using NutrientOptimizer.Core.Models;

namespace NutrientOptimizer.Web.Services;

/// <summary>
/// Service to manage local water parameters across the application
/// </summary>
public class WaterParametersService
{
    private WaterParameters _parameters = WaterParameters.CreateDefault();

    /// <summary>
    /// Event fired when water parameters change
    /// </summary>
    public event Action? OnParametersChanged;

    /// <summary>
    /// Get current water parameters
    /// </summary>
    public WaterParameters GetParameters() => new()
    {
        Nitrate = _parameters.Nitrate,
        Calcium = _parameters.Calcium,
        Magnesium = _parameters.Magnesium,
        Potassium = _parameters.Potassium,
        Sulfur = _parameters.Sulfur
    };

    /// <summary>
    /// Update water parameters
    /// </summary>
    public void SetParameters(WaterParameters parameters)
    {
        _parameters = parameters;
        NotifyChanged();
    }

    /// <summary>
    /// Notify subscribers of changes
    /// </summary>
    private void NotifyChanged()
    {
        OnParametersChanged?.Invoke();
    }
}
