using NutrientOptimizer.Core.Models;

namespace NutrientOptimizer.Core;

public static class PlantProfileLibrary
{
    public static IReadOnlyList<PlantProfile> Profiles => _profiles;

    private static readonly List<PlantProfile> _profiles = new()
    {
        // General Hydroponic Lettuce (vegetative)
        new PlantProfile
        {
            Name = "Lettuce - Vegetative",
            Description = "Common target ranges for hydroponic lettuce (e.g., Butterhead, Romaine)",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     150, 250, 200),
                new(Ion.Ammonium,      0,  30,  10),  // Low ammonium preferred
                new(Ion.Potassium,   150, 300, 200),
                new(Ion.Calcium, 100, 220, 160),
                new(Ion.Magnesium,    30,  80,  50),
                new(Ion.Phosphate,    30,  70,  50),  // as elemental P
                new(Ion.Sulfate,      50, 200, 100),
                // Micros (ppm)
                new(Ion.Iron,         1.5, 4.0, 2.5),
                new(Ion.Manganese,    0.5, 2.0, 1.0),
                new(Ion.Zinc,        0.05, 0.5, 0.2),
                new(Ion.Copper,     0.02, 0.2, 0.05),
                new(Ion.Boron,       0.2, 1.0, 0.5),
                new(Ion.Molybdenum, 0.01, 0.1, 0.05)
            },
            MinEC = 1.2,
            MaxEC = 2.0
        },

        // Tomato - Fruiting Stage (more demanding)
        new PlantProfile
        {
            Name = "Tomato - Fruiting",
            Description = "Typical hydroponic tomato during fruit development",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     180, 300, 250),
                new(Ion.Ammonium,      0,  20,  10),
                new(Ion.Potassium,   250, 400, 350),
                new(Ion.Calcium,     120, 250, 200),
                new(Ion.Magnesium,    40, 100,  70),
                new(Ion.Phosphate,    40,  80,  60),
                new(Ion.Sulfate,     100, 300, 200),
                new(Ion.Iron,         2.0, 5.0, 3.0),
                new(Ion.Manganese,    0.5, 2.5, 1.5),
                new(Ion.Zinc,        0.1, 0.8, 0.3),
                new(Ion.Copper,     0.03, 0.3, 0.1),
                new(Ion.Boron,       0.3, 1.2, 0.7),
                new(Ion.Molybdenum, 0.02, 0.15, 0.05)
            },
            MinEC = 2.0,
            MaxEC = 3.5
        },
         new PlantProfile
        {
            Name = "Test - Calcium Only",
            Description = "Very simple profile: only Calcium and Nitrate from one salt",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Calcium, 100, 200, 150),   // We want 100–200 ppm Ca, ideally 150
                new(Ion.Nitrate, 300, 600, 450),  // Nitrate will come bundled (300–600 ppm, target 450)
            }
        },
        new PlantProfile
        {
            Name = "Test - C,N,P",
            Description = "Simple profile: only Calcium and Nitrate and Potassium from two salts",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Calcium, 100, 200, 150),   // We want 100–200 ppm Ca, ideally 150
                new(Ion.Nitrate, 300, 600, 450),
                new(Ion.Potassium, 100, 600, 450)          // Nitrate will come bundled (300–600 ppm, target 450)
            }
        }
    };
}