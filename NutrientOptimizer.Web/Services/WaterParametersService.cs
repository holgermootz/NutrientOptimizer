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
    public WaterParameters GetParameters()
    {
        return _parameters;
    }

    /// <summary>
    /// Update water parameters
    /// </summary>
    public void SetParameters(WaterParameters parameters)
    {
        if (parameters != null)
        {
            _parameters = new WaterParameters
            {
                Nitrate = parameters.Nitrate,
                Calcium = parameters.Calcium,
                Magnesium = parameters.Magnesium,
                Potassium = parameters.Potassium,
                Sulfur = parameters.Sulfur
            };
            NotifyChanged();
        }
    }

    /// <summary>
    /// Notify subscribers of changes
    /// </summary>
    private void NotifyChanged()
    {
        OnParametersChanged?.Invoke();
    }
}
