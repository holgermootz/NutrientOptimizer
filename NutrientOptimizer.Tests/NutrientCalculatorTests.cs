using NutrientOptimizer.Core;
using NutrientOptimizer.Math;

namespace NutrientOptimizer.Tests;

public class NutrientCalculatorTests
{
    [Fact]
    public void Optimizer_BasicMacrosSuccess()
    {
        // Arrange
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate",
            Formula = "Ca(NO3)2·4H2O",
            MolecularWeight = 236.15,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078,
                [Ion.Nitrate] = 124.01
            }
        };

        var kNitrate = new Salt
        {
            Name = "Potassium Nitrate",
            Formula = "KNO3",
            MolecularWeight = 101.1032,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.PotassiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var mgSulfate = new Salt
        {
            Name = "Magnesium Sulfate",
            Formula = "MgSO4·7H2O",
            MolecularWeight = 246.4746,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.MagnesiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Magnesium] = 24.305,
                [Ion.Sulfate] = 96.0626
            }
        };

        var availableSalts = new List<Salt> { caNitrate, kNitrate, mgSulfate };
        var profile = PlantProfileLibrary.Profiles.First(p => p.Name == "Test - Three Macros");

        // Act
        var optimizer = new NutrientRecipeOptimizer(availableSalts, profile);
        var result = optimizer.Solve();

        // Assert
        Assert.True(result.Success, result.ReasonForTermination);
        Assert.NotEmpty(result.SaltAmounts);
        Assert.NotEmpty(result.IonComparisons);

        Console.WriteLine(result);
    }

    [Fact]
    public void Optimizer_InsufficientSaltsFailure()
    {
        // Arrange: Only nitrogen source, missing P, K, Mg, S
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate",
            Formula = "Ca(NO3)2·4H2O",
            MolecularWeight = 236.15,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078,
                [Ion.Nitrate] = 124.01
            }
        };

        var availableSalts = new List<Salt> { caNitrate };
        var profile = PlantProfileLibrary.Profiles.First(p => p.Name == "Lettuce - Vegetative");

        // Act
        var optimizer = new NutrientRecipeOptimizer(availableSalts, profile);
        var result = optimizer.Solve();

        // Assert
        Assert.False(result.Success);
        Assert.Contains("no source", string.Join("\n", result.InfeasibilityReasons));

        Console.WriteLine(result);
    }

    [Fact]
    public void Optimizer_PartialSuppliableIons()
    {
        // Arrange: Use Lettuce profile which requires Phosphate - salts don't provide it
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate",
            Formula = "Ca(NO3)2·4H2O",
            MolecularWeight = 236.15,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078,
                [Ion.Nitrate] = 124.01
            }
        };

        var kNitrate = new Salt
        {
            Name = "Potassium Nitrate",
            Formula = "KNO3",
            MolecularWeight = 101.1032,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.PotassiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var mgSulfate = new Salt
        {
            Name = "Magnesium Sulfate",
            Formula = "MgSO4·7H2O",
            MolecularWeight = 246.4746,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.MagnesiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Magnesium] = 24.305,
                [Ion.Sulfate] = 96.0626
            }
        };

        var availableSalts = new List<Salt> { caNitrate, kNitrate, mgSulfate };
        // Lettuce profile requires Phosphate which we don't provide
        var profile = PlantProfileLibrary.Profiles.First(p => p.Name == "Lettuce - Vegetative");

        // Act
        var optimizer = new NutrientRecipeOptimizer(availableSalts, profile);
        var result = optimizer.Solve();

        // Assert - missing Phosphate
        Assert.False(result.Success);
        Assert.Contains("Phosphate", string.Join("\n", result.InfeasibilityReasons));

        Console.WriteLine(result);
    }

    [Fact]
    public void Optimizer_DisplaysIonDeltas()
    {
        // Arrange
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate",
            Formula = "Ca(NO3)2·4H2O",
            MolecularWeight = 236.15,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Calcium] = 40.078,
                [Ion.Nitrate] = 124.01
            }
        };

        var kNitrate = new Salt
        {
            Name = "Potassium Nitrate",
            Formula = "KNO3",
            MolecularWeight = 101.1032,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.PotassiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var mgSulfate = new Salt
        {
            Name = "Magnesium Sulfate",
            Formula = "MgSO4·7H2O",
            MolecularWeight = 246.4746,
            Category = SaltCategory.Macronutrient,
            Group = SaltGroup.MagnesiumSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Magnesium] = 24.305,
                [Ion.Sulfate] = 96.0626
            }
        };

        var availableSalts = new List<Salt> { caNitrate, kNitrate, mgSulfate };
        var profile = PlantProfileLibrary.Profiles.First(p => p.Name == "Lettuce - Vegetative");

        // Act
        var optimizer = new NutrientRecipeOptimizer(availableSalts, profile);
        var result = optimizer.Solve();

        // Assert - verify comparison data structure
        if (result.Success)
        {
            Assert.NotEmpty(result.IonComparisons);

            var nitrateComp = result.IonComparisons.FirstOrDefault(c => c.Ion == Ion.Nitrate);
            Assert.NotNull(nitrateComp);
            Assert.True(nitrateComp.ActualPpm > 0);
            Assert.True(nitrateComp.TargetPpm > 0);

            Console.WriteLine($"Nitrate: {nitrateComp.ActualPpm:F1} ppm (target: {nitrateComp.TargetPpm:F1}, delta: {nitrateComp.DeltaPpm:+0.0;-0.0})");
        }

        Console.WriteLine(result);
    }
}