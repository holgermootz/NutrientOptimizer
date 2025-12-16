using System;
using System.Collections.Generic;
using System.Linq;
using NutrientOptimizer.Core.DbfReader;

namespace NutrientOptimizer.Core.Data;

/// <summary>
/// Imports nutrient/substance data from the substances_win.dbf file
/// and converts it to Salt model instances
/// </summary>
public class SubstanceImporter
{
    // Map DBF field names to nutrient/element names
    private static readonly Dictionary<string, Ion> FieldToIonMapping = new()
    {
        { "N (NO3-)", Ion.Nitrate },
        { "N (NH4+)", Ion.Ammonium },
        { "P", Ion.Phosphate },
        { "K", Ion.Potassium },
        { "MG", Ion.Magnesium },
        { "CA", Ion.Calcium },
        { "S", Ion.Sulfate },
        { "B", Ion.Boron },
        { "FE", Ion.Iron },
        { "ZN", Ion.Zinc },
        { "MN", Ion.Manganese },
        { "CU", Ion.Copper },
        { "MO", Ion.Molybdenum },
        { "NA", Ion.Sodium },
        { "SI", Ion.Silicon },
        { "CL", Ion.Chlorine },
    };

    /// <summary>
    /// Import substances from DBF file and convert to Salt objects
    /// </summary>
    public static List<Salt> ImportFromDbf(string dbfFilePath)
    {
        var (fileInfo, records) = DbfFileReader.ReadDbfFile(dbfFilePath);
        var salts = new List<Salt>();

        foreach (var record in records)
        {
            var salt = ConvertRecordToSalt(record);
            if (salt != null)
            {
                salts.Add(salt);
            }
        }

        return salts;
    }

    /// <summary>
    /// Convert a DBF record dictionary to a Salt object
    /// </summary>
    private static Salt? ConvertRecordToSalt(Dictionary<string, string> record)
    {
        // Get basic info
        if (!record.TryGetValue("NAME", out var name) || string.IsNullOrWhiteSpace(name))
            return null;

        // Skip placeholder/empty entries
        if (name.StartsWith("*") && name.Trim() == "*")
            return null;

        // Remove asterisk prefix if present
        name = name.TrimStart('*').Trim();

        if (!record.TryGetValue("FORMULA", out var formula))
            formula = string.Empty;

        formula = formula.Trim();

        // Determine category and group based on content
        var (category, group) = DetermineCategoryAndGroup(record);

        var salt = new Salt
        {
            Name = name,
            Formula = formula,
            Category = category,
            Group = group,
            Description = $"{name} ({formula})"
        };

        // Extract ion contributions
        foreach (var (fieldName, ion) in FieldToIonMapping)
        {
            if (record.TryGetValue(fieldName, out var valueStr) &&
                double.TryParse(valueStr.Replace(",", "."), System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                if (value > 0)
                {
                    salt.IonContributions[ion] = value;
                }
            }
        }

        return salt;
    }

    /// <summary>
    /// Determine the category and group based on ion contributions
    /// </summary>
    private static (SaltCategory category, SaltGroup group) DetermineCategoryAndGroup(
        Dictionary<string, string> record)
    {
        // Helper function to check if a field has significant value
        bool HasElement(string fieldName)
        {
            return record.TryGetValue(fieldName, out var val) &&
                   double.TryParse(val.Replace(",", "."), System.Globalization.NumberStyles.Any,
                       System.Globalization.CultureInfo.InvariantCulture, out var value) &&
                   value > 0;
        }

        var category = SaltCategory.Macronutrient;
        SaltGroup? group = null;

        // Determine primary element
        if (HasElement("N (NO3-)") || HasElement("N (NH4+)"))
        {
            group = SaltGroup.NitrogenSource;
        }
        else if (HasElement("P"))
        {
            group = SaltGroup.PhosphorusSource;
        }
        else if (HasElement("K"))
        {
            group = SaltGroup.PotassiumSource;
        }
        else if (HasElement("CA"))
        {
            group = SaltGroup.CalciumSource;
        }
        else if (HasElement("MG"))
        {
            group = SaltGroup.MagnesiumSource;
        }
        else if (HasElement("S"))
        {
            group = SaltGroup.SulfurSource;
        }
        else if (HasElement("FE"))
        {
            group = SaltGroup.IronSource;
            category = SaltCategory.Micronutrient;
        }
        else if (HasElement("MN"))
        {
            group = SaltGroup.ManganeseSource;
            category = SaltCategory.Micronutrient;
        }
        else if (HasElement("ZN"))
        {
            group = SaltGroup.ZincSource;
            category = SaltCategory.Micronutrient;
        }
        else if (HasElement("CU"))
        {
            group = SaltGroup.CopperSource;
            category = SaltCategory.Micronutrient;
        }
        else if (HasElement("B"))
        {
            group = SaltGroup.BoronSource;
            category = SaltCategory.Micronutrient;
        }
        else if (HasElement("MO"))
        {
            group = SaltGroup.MolybdenumSource;
            category = SaltCategory.Micronutrient;
        }

        // If no primary element found, default to nitrogen
        if (group == null)
        {
            group = SaltGroup.NitrogenSource;
        }

        return (category, group.Value);
    }
}
