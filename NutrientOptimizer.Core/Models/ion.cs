namespace NutrientOptimizer.Core;

public enum Ion
{
    Nitrate,      // NO3-
    Ammonium,     // NH4+
    Potassium,    // K+
    Calcium,      // Ca2+
    Magnesium,    // Mg2+
    Phosphate,    // H2PO4- / PO4 3- (we'll treat as P for now, or split later)
    Sulfate,      // SO4 2-
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