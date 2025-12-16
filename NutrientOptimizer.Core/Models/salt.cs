namespace NutrientOptimizer.Core;

public enum SaltCategory
{
    Macronutrient,
    Micronutrient
}

public enum SaltGroup
{
    // Macronutrients
    NitrogenSource,
    PhosphorusSource,
    PotassiumSource,
    CalciumSource,
    MagnesiumSource,
    SulfurSource,
    
    // Micronutrients
    IronSource,
    ManganeseSource,
    ZincSource,
    CopperSource,
    BoronSource,
    MolybdenumSource
}

public enum SubstanceType
{
    Raw,           // Pure chemical salts
    Commercial     // Commercial fertilizer formulations
}

public class Salt
{
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public double MolecularWeight { get; set; }

    // Categorization
    public SaltCategory Category { get; set; }
    public SaltGroup Group { get; set; }
    public SubstanceType Type { get; set; } = SubstanceType.Raw;
    
    // Description for user display
    public string Description { get; set; } = string.Empty;

    // Dictionary: Ion -> amount of that ion (in grams per mole of salt)
    public Dictionary<Ion, double> IonContributions { get; set; } = new();

    /// <summary>
    /// Get a user-friendly display of ions contributed by this salt.
    /// Example: "Nitrogen, Potassium" or "Iron (chelated)"
    /// </summary>
    public string GetIonSummary()
    {
        var ions = IonContributions.Keys
            .OrderBy(ion => ion.ToString())
            .Select(ion => ion.ToString())
            .ToList();
        
        return string.Join(", ", ions);
    }

    /// <summary>
    /// Get detailed contribution info for tooltips
    /// </summary>
    public string GetDetailedInfo()
    {
        var lines = new List<string>
        {
            $"Formula: {Formula}",
            $"Molecular Weight: {MolecularWeight:F2} g/mol",
            $"Category: {Category}",
            $"Type: {Type}",
            $"",
            "Ion Contributions (g/mol):"
        };

        foreach (var (ion, grams) in IonContributions.OrderBy(x => x.Key.ToString()))
        {
            lines.Add($"  • {ion}: {grams:F4} g/mol");
        }

        return string.Join("\n", lines);
    }

    public override string ToString()
    {
        return $"{Name} - {GetIonSummary()}";
    }
}