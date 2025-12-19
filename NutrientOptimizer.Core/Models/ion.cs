namespace NutrientOptimizer.Core;

public enum Ion
{
    Nitrate,      // NO3−
    Ammonium,     // NH4+
    Potassium,    // K+
    Calcium,      // Ca2+
    Magnesium,    // Mg2+
    Phosphate,    // PO43−
    Sulfate,      // SO42−
    Iron,         // Fe
    Manganese,    // Mn
    Zinc,         // Zn
    Copper,       // Cu
    Boron,        // B
    Molybdenum,   // Mo
    Sodium,       // Na
    Silicon,      // Si
    Chlorine      // Cl
}

public static class IonExtensions
{
    /// <summary>
    /// Get the chemical symbol/abbreviation for an ion
    /// </summary>
    public static string GetAbbreviation(this Ion ion) => ion switch
    {
        Ion.Nitrate => "NO₃⁻",
        Ion.Ammonium => "NH₄⁺",
        Ion.Potassium => "K⁺",
        Ion.Calcium => "Ca²⁺",
        Ion.Magnesium => "Mg²⁺",
        Ion.Phosphate => "PO₄³⁻",
        Ion.Sulfate => "SO₄²⁻",
        Ion.Iron => "Fe",
        Ion.Manganese => "Mn",
        Ion.Zinc => "Zn",
        Ion.Copper => "Cu",
        Ion.Boron => "B",
        Ion.Molybdenum => "Mo",
        Ion.Sodium => "Na",
        Ion.Silicon => "Si",
        Ion.Chlorine => "Cl",
        _ => ion.ToString()
    };

    /// <summary>
    /// Get display name with abbreviation (e.g., "Calcium (Ca²⁺)")
    /// </summary>
    public static string GetDisplayName(this Ion ion) => $"{ion} ({ion.GetAbbreviation()})";
}    