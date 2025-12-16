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

        // Basil - Fast growing herb
        new PlantProfile
        {
            Name = "Basil - Vegetative",
            Description = "Sweet basil and culinary varieties - fast growing herb stage",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     140, 200, 170),
                new(Ion.Ammonium,      0,  25,   8),
                new(Ion.Potassium,   120, 200, 160),
                new(Ion.Calcium,      80, 150, 120),
                new(Ion.Magnesium,    25,  60,  40),
                new(Ion.Phosphate,    25,  50,  35),
                new(Ion.Sulfate,      50, 150,  80),
                new(Ion.Iron,         1.5, 3.5, 2.5),
                new(Ion.Manganese,    0.5, 1.5, 0.8),
                new(Ion.Zinc,        0.05, 0.3, 0.15),
                new(Ion.Copper,     0.02, 0.15, 0.05),
                new(Ion.Boron,       0.3, 0.8, 0.5),
                new(Ion.Molybdenum, 0.01, 0.08, 0.03)
            },
            MinEC = 1.0,
            MaxEC = 1.8
        },

        // Cucumber - Flowering/Fruiting
        new PlantProfile
        {
            Name = "Cucumber - Fruiting",
            Description = "Greenhouse cucumber during flowering and fruit set",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     160, 280, 220),
                new(Ion.Ammonium,      0,  20,   8),
                new(Ion.Potassium,   180, 300, 240),
                new(Ion.Calcium,     100, 200, 150),
                new(Ion.Magnesium,    35,  80,  55),
                new(Ion.Phosphate,    30,  60,  45),
                new(Ion.Sulfate,      60, 180, 100),
                new(Ion.Iron,         1.5, 4.0, 2.5),
                new(Ion.Manganese,    0.5, 2.0, 1.0),
                new(Ion.Zinc,        0.1, 0.5, 0.25),
                new(Ion.Copper,     0.03, 0.2, 0.08),
                new(Ion.Boron,       0.4, 1.0, 0.6),
                new(Ion.Molybdenum, 0.01, 0.1, 0.05)
            },
            MinEC = 1.4,
            MaxEC = 2.4
        },

        // Peppers - Fruiting Stage
        new PlantProfile
        {
            Name = "Peppers - Fruiting",
            Description = "Sweet bell peppers and chili peppers during fruit development",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     170, 280, 220),
                new(Ion.Ammonium,      0,  20,  10),
                new(Ion.Potassium,   220, 350, 280),
                new(Ion.Calcium,     130, 220, 170),
                new(Ion.Magnesium,    40,  90,  60),
                new(Ion.Phosphate,    35,  70,  50),
                new(Ion.Sulfate,      80, 200, 120),
                new(Ion.Iron,         2.0, 4.5, 3.0),
                new(Ion.Manganese,    0.8, 2.5, 1.5),
                new(Ion.Zinc,        0.15, 0.6, 0.3),
                new(Ion.Copper,     0.05, 0.25, 0.12),
                new(Ion.Boron,       0.5, 1.2, 0.8),
                new(Ion.Molybdenum, 0.02, 0.12, 0.06)
            },
            MinEC = 1.8,
            MaxEC = 3.0
        },

        // Strawberry - Flowering/Fruiting
        new PlantProfile
        {
            Name = "Strawberry - Fruiting",
            Description = "Strawberries during flowering and fruit development",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     130, 200, 160),
                new(Ion.Ammonium,      0,  15,   5),
                new(Ion.Potassium,   180, 280, 220),
                new(Ion.Calcium,     100, 180, 140),
                new(Ion.Magnesium,    30,  70,  45),
                new(Ion.Phosphate,    30,  60,  45),
                new(Ion.Sulfate,      50, 150,  80),
                new(Ion.Iron,         1.5, 3.5, 2.5),
                new(Ion.Manganese,    0.8, 2.0, 1.2),
                new(Ion.Zinc,        0.1, 0.4, 0.2),
                new(Ion.Copper,     0.05, 0.2, 0.1),
                new(Ion.Boron,       0.6, 1.2, 0.9),
                new(Ion.Molybdenum, 0.01, 0.08, 0.04)
            },
            MinEC = 1.3,
            MaxEC = 2.1
        },

        // Spinach - Vegetative/Harvest
        new PlantProfile
        {
            Name = "Spinach - Vegetative",
            Description = "Spinach during vegetative growth and harvest stage",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     160, 240, 200),
                new(Ion.Ammonium,      0,  30,  10),
                new(Ion.Potassium,   120, 200, 160),
                new(Ion.Calcium,      80, 150, 110),
                new(Ion.Magnesium,    30,  70,  45),
                new(Ion.Phosphate,    25,  50,  35),
                new(Ion.Sulfate,      40, 120,  70),
                new(Ion.Iron,         2.0, 4.0, 3.0),
                new(Ion.Manganese,    1.0, 2.5, 1.5),
                new(Ion.Zinc,        0.1, 0.5, 0.25),
                new(Ion.Copper,     0.05, 0.2, 0.1),
                new(Ion.Boron,       0.3, 0.8, 0.5),
                new(Ion.Molybdenum, 0.02, 0.1, 0.05)
            },
            MinEC = 1.2,
            MaxEC = 1.9
        },

        // Kale - Leafy Green
        new PlantProfile
        {
            Name = "Kale - Vegetative",
            Description = "Kale and other brassica leafy greens during growth",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     180, 260, 220),
                new(Ion.Ammonium,      0,  25,  10),
                new(Ion.Potassium,   140, 220, 180),
                new(Ion.Calcium,     100, 170, 135),
                new(Ion.Magnesium,    35,  75,  50),
                new(Ion.Phosphate,    30,  60,  45),
                new(Ion.Sulfate,      50, 150,  80),
                new(Ion.Iron,         2.5, 5.0, 3.5),
                new(Ion.Manganese,    1.0, 2.5, 1.5),
                new(Ion.Zinc,        0.15, 0.6, 0.3),
                new(Ion.Copper,     0.05, 0.25, 0.12),
                new(Ion.Boron,       0.4, 1.0, 0.6),
                new(Ion.Molybdenum, 0.02, 0.1, 0.05)
            },
            MinEC = 1.3,
            MaxEC = 2.1
        },

        // Microgreens - High Density
        new PlantProfile
        {
            Name = "Microgreens - Growth",
            Description = "Microgreens and sprouts - intensive, short-cycle production",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     100, 180, 140),
                new(Ion.Ammonium,      0,  20,   5),
                new(Ion.Potassium,   100, 180, 140),
                new(Ion.Calcium,      60, 120,  90),
                new(Ion.Magnesium,    20,  50,  35),
                new(Ion.Phosphate,    20,  40,  30),
                new(Ion.Sulfate,      30, 100,  60),
                new(Ion.Iron,         1.5, 3.0, 2.0),
                new(Ion.Manganese,    0.5, 1.5, 1.0),
                new(Ion.Zinc,        0.05, 0.25, 0.15),
                new(Ion.Copper,     0.02, 0.1, 0.05),
                new(Ion.Boron,       0.2, 0.6, 0.4),
                new(Ion.Molybdenum, 0.01, 0.05, 0.03)
            },
            MinEC = 0.8,
            MaxEC = 1.4
        },

        // Watercress - Aquatic Vegetable
        new PlantProfile
        {
            Name = "Watercress - Vegetative",
            Description = "Watercress and aquatic greens - prefer lower nutrient levels",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     120, 180, 150),
                new(Ion.Ammonium,      0,  20,   5),
                new(Ion.Potassium,    80, 150, 110),
                new(Ion.Calcium,      60, 120,  90),
                new(Ion.Magnesium,    20,  50,  35),
                new(Ion.Phosphate,    20,  40,  30),
                new(Ion.Sulfate,      30, 100,  60),
                new(Ion.Iron,         1.5, 3.0, 2.0),
                new(Ion.Manganese,    0.8, 2.0, 1.2),
                new(Ion.Zinc,        0.1, 0.4, 0.2),
                new(Ion.Copper,     0.03, 0.15, 0.08),
                new(Ion.Boron,       0.3, 0.8, 0.5),
                new(Ion.Molybdenum, 0.01, 0.08, 0.04)
            },
            MinEC = 0.9,
            MaxEC = 1.6
        },

        // Eggplant - Fruiting
        new PlantProfile
        {
            Name = "Eggplant - Fruiting",
            Description = "Eggplant/Aubergine during flowering and fruit set",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Nitrate,     160, 260, 210),
                new(Ion.Ammonium,      0,  15,   5),
                new(Ion.Potassium,   200, 320, 260),
                new(Ion.Calcium,     120, 210, 165),
                new(Ion.Magnesium,    40,  85,  60),
                new(Ion.Phosphate,    35,  65,  50),
                new(Ion.Sulfate,      80, 200, 120),
                new(Ion.Iron,         2.0, 4.5, 3.0),
                new(Ion.Manganese,    0.8, 2.5, 1.5),
                new(Ion.Zinc,        0.15, 0.5, 0.3),
                new(Ion.Copper,     0.05, 0.2, 0.1),
                new(Ion.Boron,       0.5, 1.2, 0.8),
                new(Ion.Molybdenum, 0.02, 0.12, 0.06)
            },
            MinEC = 1.6,
            MaxEC = 2.8
        },

        // Test Profiles
        new PlantProfile
        {
            Name = "Test - Calcium Only",
            Description = "Very simple profile: only Calcium and Nitrate from one salt",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Calcium, 100, 200, 150),
                new(Ion.Nitrate, 300, 600, 450),
            }
        },
        new PlantProfile
        {
            Name = "Test - C,N,P",
            Description = "Simple profile: only Calcium, Nitrate, and Potassium",
            IonTargets = new List<IonTarget>
            {
                new(Ion.Calcium, 100, 200, 150),
                new(Ion.Nitrate, 300, 600, 450),
                new(Ion.Boron, 10, 20, 15),
                new(Ion.Potassium, 100, 600, 450)
            }
        }
    };
}