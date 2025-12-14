using System.Collections.Generic;

namespace NutrientOptimizer.Core;

public static class SaltLibrary
{
    public static IReadOnlyList<Salt> CommonSalts => _salts;

    private static readonly List<Salt> _salts = new()
    {
        // Calcium Nitrate Tetrahydrate - Ca(NO3)2 · 4H2O
        new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
            Formula = "Ca(NO3)2·4H2O",
            MolecularWeight = 236.15,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078,
                [Ion.Nitrate] = 124.01   // 2 × 62.005
            }
        },

        // Potassium Nitrate - KNO3
        new Salt
        {
            Name = "Potassium Nitrate",
            Formula = "KNO3",
            MolecularWeight = 101.1032,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        },

        // Magnesium Sulfate Heptahydrate - MgSO4 · 7H2O
        new Salt
        {
            Name = "Magnesium Sulfate Heptahydrate",
            Formula = "MgSO4·7H2O",
            MolecularWeight = 246.4746,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Magnesium] = 24.305,
                [Ion.Sulfate] = 96.0626
            }
        },

        // Monopotassium Phosphate - KH2PO4
        new Salt
        {
            Name = "Monopotassium Phosphate",
            Formula = "KH2PO4",
            MolecularWeight = 136.0855,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Phosphate] = 30.9738   // elemental P
            }
        },

        // Calcium Chloride Dihydrate - CaCl2 · 2H2O
        new Salt
        {
            Name = "Calcium Chloride Dihydrate",
            Formula = "CaCl2·2H2O",
            MolecularWeight = 147.014,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078
            }
        }
    };
}