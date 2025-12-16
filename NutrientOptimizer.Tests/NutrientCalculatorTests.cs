using NutrientOptimizer.Core;
using NutrientOptimizer.Math;

namespace NutrientOptimizer.Tests;

public class NutrientCalculatorTests
{
    [Fact]
    public void CalculateSolution_BasicRecipe_ReturnsExpectedIons()
    {
        var recipe = new Recipe();
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
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
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var mgSulfate = new Salt
        {
            Name = "Magnesium Sulfate Heptahydrate",
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

        recipe.AddSalt(caNitrate, 0.9);
        recipe.AddSalt(kNitrate, 0.5);
        recipe.AddSalt(mgSulfate, 0.4);

        var solution = NutrientCalculator.CalculateSolution(recipe);

        Assert.True(System.Math.Abs(solution.IonConcentrationsPpm[Ion.Calcium] - 152) < 10);
        Assert.True(System.Math.Abs(solution.IonConcentrationsPpm[Ion.Nitrate] - 470) < 20);
        Assert.True(System.Math.Abs(solution.IonConcentrationsPpm[Ion.Potassium] - 193) < 10);
        Assert.True(System.Math.Abs(solution.IonConcentrationsPpm[Ion.Magnesium] - 39) < 5);
        Assert.True(System.Math.Abs(solution.IonConcentrationsPpm[Ion.Sulfate] - 156) < 10);
    }

    [Fact]
    public void SolutionValidator_LettuceProfile_RespectsRanges()
    {
        var recipe = new Recipe();
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
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
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var mgSulfate = new Salt
        {
            Name = "Magnesium Sulfate Heptahydrate",
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

        recipe.AddSalt(caNitrate, 0.95);
        recipe.AddSalt(kNitrate, 0.60);
        recipe.AddSalt(mgSulfate, 0.50);

        var solution = NutrientCalculator.CalculateSolution(recipe);
        var lettuce = PlantProfileLibrary.Profiles[0];

        var violations = SolutionValidator.GetViolations(solution, lettuce);
        Assert.NotEmpty(violations);
    }

    [Fact]
    public void Optimizer_SingleSalt_SimpleProfile_ReturnsValidRecipe()
    {
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
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
        var testProfile = PlantProfileLibrary.Profiles
            .First(p => p.Name == "Test - Calcium Only");

        var optimizer = new NutrientRecipeOptimizer(availableSalts, testProfile);
        var result = optimizer.Solve();

        Console.WriteLine(result);

        Assert.True(result.Success, $"Optimization failed: {result.ReasonForTermination}");
        Assert.Single(result.SaltAmounts);
        Assert.Contains(result.SaltAmounts, kv => kv.Key.Name == "Calcium Nitrate Tetrahydrate");

        var solution = NutrientCalculator.CalculateSolution(result.ToRecipe());
        var ca = solution.IonConcentrationsPpm[Ion.Calcium];
        var no3 = solution.IonConcentrationsPpm[Ion.Nitrate];

        Assert.InRange(ca, 100, 200);
        Assert.InRange(no3, 300, 600);
    }

    [Fact]
    public void Optimizer_TwoSalts_SimpleProfile_ReturnsValidRecipe()
    {
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
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
            Group = SaltGroup.NitrogenSource,
            IonContributions = new Dictionary<Ion, double>
            {
                [Ion.Potassium] = 39.0983,
                [Ion.Nitrate] = 62.005
            }
        };

        var availableSalts = new List<Salt> { caNitrate, kNitrate };
        var testProfile = PlantProfileLibrary.Profiles
            .First(p => p.Name == "Test - C,N,P");

        var optimizer = new NutrientRecipeOptimizer(availableSalts, testProfile);
        var result = optimizer.Solve();

        Console.WriteLine(result);

        Assert.True(result.Success, $"Optimization failed: {result.ReasonForTermination}");
        Assert.Single(result.SaltAmounts);

        var solution = NutrientCalculator.CalculateSolution(result.ToRecipe());
        var ca = solution.IonConcentrationsPpm[Ion.Calcium];
        var no3 = solution.IonConcentrationsPpm[Ion.Nitrate];

        Assert.InRange(ca, 100, 200);
        Assert.InRange(no3, 300, 600);
    }

    [Fact]
    public void Optimizer_AllSalts_SimpleProfile_ReturnsValidRecipe()
    {
        var caNitrate = new Salt
        {
            Name = "Calcium Nitrate Tetrahydrate",
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

        var allSalts = new List<Salt> { caNitrate };
        var testProfile = PlantProfileLibrary.Profiles
            .First(p => p.Name == "Test - C,N,P");

        var optimizer = new NutrientRecipeOptimizer(allSalts, testProfile);
        var result = optimizer.Solve();

        Console.WriteLine(result);

        Assert.True(result.Success, $"Optimization failed: {result.ReasonForTermination}");
    }
}