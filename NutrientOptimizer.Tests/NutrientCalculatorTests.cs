using NutrientOptimizer.Core;
using NutrientOptimizer.Math;

namespace NutrientOptimizer.Tests;

public class NutrientCalculatorTests
{
    [Fact]
    public void CalculateSolution_BasicRecipe_ReturnsExpectedIons()
    {
        var recipe = new Recipe();
        var caNitrate = SaltLibrary.CommonSalts[0];
        var kNitrate = SaltLibrary.CommonSalts[1];
        var mgSulfate = SaltLibrary.CommonSalts[2];

        recipe.AddSalt(caNitrate, 0.9);  // 0.9 g/L Ca(NO3)2·4H2O
        recipe.AddSalt(kNitrate, 0.5);   // 0.5 g/L KNO3
        recipe.AddSalt(mgSulfate, 0.4);  // 0.4 g/L MgSO4·7H2O

        var solution = NutrientCalculator.CalculateSolution(recipe);

        // Approximate expected values (you can refine these)
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
        // Use some realistic amounts...
        recipe.AddSalt(SaltLibrary.CommonSalts[0], 0.95);  // Ca(NO3)2·4H2O
        recipe.AddSalt(SaltLibrary.CommonSalts[1], 0.60);  // KNO3
        recipe.AddSalt(SaltLibrary.CommonSalts[2], 0.50);  // MgSO4·7H2O
                                                           // Add phosphate source later

        var solution = NutrientCalculator.CalculateSolution(recipe);
        var lettuce = PlantProfileLibrary.Profiles[0];

        var violations = SolutionValidator.GetViolations(solution, lettuce);
        // This will likely show missing Phosphate and low K, etc. — that's expected at this stage
        Assert.NotEmpty(violations); // for now, until we complete the recipe
    }
    [Fact]
    public void Optimizer_SingleSalt_SimpleProfile_ReturnsValidRecipe()
    {
        // Use only ONE salt
        var singleSalt = SaltLibrary.CommonSalts
            .First(s => s.Name == "Calcium Nitrate Tetrahydrate");

        var availableSalts = new List<Salt> { singleSalt };

        // Use our simple test profile
        var testProfile = PlantProfileLibrary.Profiles
            .First(p => p.Name == "Test - Calcium Only");

        var optimizer = new NutrientRecipeOptimizer(availableSalts, testProfile);
        var result = optimizer.Solve();

        Console.WriteLine(result);

        // Should succeed
        Assert.True(result.Success, $"Optimization failed: {result.ReasonForTermination}");

        // Should use some amount of the salt
        Assert.Single(result.SaltAmounts);
        Assert.Contains(result.SaltAmounts, kv => kv.Key.Name == "Calcium Nitrate Tetrahydrate");

        // Check resulting concentrations are within bounds
        var solution = NutrientCalculator.CalculateSolution(result.ToRecipe());
        var ca = solution.IonConcentrationsPpm[Ion.Calcium];
        var no3 = solution.IonConcentrationsPpm[Ion.Nitrate];

        Assert.InRange(ca, 100, 200);
        Assert.InRange(no3, 300, 600);

        // Should be very close to target Calcium (150 ppm) since that's optimizable
        // Ca(NO3)2·4H2O gives ~169.7 ppm Ca and ~525 ppm NO3 per 1 g/L
        // To hit 150 ppm Ca → ~0.883 g/L
        var expectedGrams = 150.0 / (40.078 * 1000 / 236.15); // ~0.883 g/L
        var actualGrams = result.SaltAmounts[singleSalt];
        Assert.Equal(expectedGrams, actualGrams, 2); // within 0.01 precision
    }

    [Fact]
    public void Optimizer_TwoSalts_SimpleProfile_ReturnsValidRecipe()
    {
        // Use only two salts
        var firstSalt = SaltLibrary.CommonSalts
            .First(s => s.Name == "Calcium Nitrate Tetrahydrate");
        var secondSalt = SaltLibrary.CommonSalts
            .First(s => s.Name == "Potassium Nitrate");
        //"Potassium Nitrate"
        var availableSalts = new List<Salt> { firstSalt, secondSalt };

        // Use our simple test profile
        var testProfile = PlantProfileLibrary.Profiles
            .First(p => p.Name == "Test - C,N,P");

        var optimizer = new NutrientRecipeOptimizer(availableSalts, testProfile);
        var result = optimizer.Solve();

        Console.WriteLine(result);

        // Should succeed
        Assert.True(result.Success, $"Optimization failed: {result.ReasonForTermination}");

        // Should use some amount of the salt
        Assert.Single(result.SaltAmounts);
        Assert.Contains(result.SaltAmounts, kv => kv.Key.Name == "Calcium Nitrate Tetrahydrate");

        // Check resulting concentrations are within bounds
        var solution = NutrientCalculator.CalculateSolution(result.ToRecipe());
        var ca = solution.IonConcentrationsPpm[Ion.Calcium];
        var no3 = solution.IonConcentrationsPpm[Ion.Nitrate];

        Assert.InRange(ca, 100, 200);
        Assert.InRange(no3, 300, 600);

        // Should be very close to target Calcium (150 ppm) since that's optimizable
        // Ca(NO3)2·4H2O gives ~169.7 ppm Ca and ~525 ppm NO3 per 1 g/L
        // To hit 150 ppm Ca → ~0.883 g/L
        var expectedGrams = 150.0 / (40.078 * 1000 / 236.15); // ~0.883 g/L
        var actualGrams = result.SaltAmounts[firstSalt];
        Assert.Equal(expectedGrams, actualGrams, 2); // within 0.01 precision
    }
    // Helper to make it easy
    private static Salt GetSalt(string name) =>
        SaltLibrary.CommonSalts.First(s => s.Name == name);
}