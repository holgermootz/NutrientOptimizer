namespace NutrientOptimizer.Core;

public static class NutrientCalculator
{
    /// <summary>
    /// Calculates the resulting ion concentrations from a recipe (in ppm)
    /// </summary>
    public static SolutionProfile CalculateSolution(Recipe recipe)
    {
        var profile = new SolutionProfile();

        foreach (var item in recipe.Items)
        {
            var salt = item.Salt;
            double molesPerLiter = item.GramsPerLiter / salt.MolecularWeight;

            foreach (var ionContrib in salt.IonContributions)
            {
                double ppm = molesPerLiter * ionContrib.Value * 1000; // g/L → ppm (mg/L)

                if (profile.IonConcentrationsPpm.ContainsKey(ionContrib.Key))
                    profile.IonConcentrationsPpm[ionContrib.Key] += ppm;
                else
                    profile.IonConcentrationsPpm[ionContrib.Key] = ppm;
            }
        }

        return profile;
    }
}