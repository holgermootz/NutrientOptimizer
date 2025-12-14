namespace NutrientOptimizer.Core;

public class Recipe
{
    public List<RecipeItem> Items { get; set; } = new();

    public void AddSalt(Salt salt, double gramsPerLiter)
    {
        Items.Add(new RecipeItem { Salt = salt, GramsPerLiter = gramsPerLiter });
    }
}

public class RecipeItem
{
    public Salt Salt { get; set; } = null!;
    public double GramsPerLiter { get; set; }
}