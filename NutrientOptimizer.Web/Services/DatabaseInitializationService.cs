using Microsoft.EntityFrameworkCore;
using NutrientOptimizer.Core;
using NutrientOptimizer.Core.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NutrientOptimizer.Web.Services;

/// <summary>
/// Service to initialize and seed the nutrient database from CSV files
/// </summary>
public class DatabaseInitializationService
{
    private readonly NutrientDbContext _context;

    public DatabaseInitializationService(NutrientDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Initialize the database with substances from CSV files
    /// Deletes and recreates the database on every start
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Delete existing database
            Console.WriteLine("???  Deleting existing database...");
            await _context.Database.EnsureDeletedAsync();
            Console.WriteLine("? Database deleted");

            // Create fresh database
            Console.WriteLine("???  Creating new database...");
            await _context.Database.EnsureCreatedAsync();
            Console.WriteLine("? Database created");

            Console.WriteLine("?? Loading substances from CSV files...");

            // Find CSV files in data/init folder
            var csvFiles = FindCsvFiles();
            if (csvFiles.Count == 0)
            {
                Console.WriteLine("??  No CSV files found in data/init folder");
                return;
            }

            Console.WriteLine($"Found {csvFiles.Count} CSV file(s)");

            // Load all substances from CSV files
            var substances = new List<Salt>();
            foreach (var csvFile in csvFiles)
            {
                Console.WriteLine($"  Loading: {Path.GetFileName(csvFile)}");
                var loaded = LoadSubstancesFromCsv(csvFile);
                substances.AddRange(loaded);
                Console.WriteLine($"  ? Loaded {loaded.Count} substances");
            }

            if (substances.Count == 0)
            {
                Console.WriteLine("??  No substances loaded from CSV files");
                return;
            }

            Console.WriteLine($"? Total: {substances.Count} substances loaded");

            // Convert Salt objects to SaltEntity objects
            var saltEntities = substances.Select(salt => new SaltEntity
            {
                Name = salt.Name,
                Formula = salt.Formula,
                MolecularWeight = salt.MolecularWeight,
                Category = salt.Category.ToString(),
                Group = salt.Group.ToString(),
                Type = salt.Type.ToString(),
                Contributions = salt.IonContributions
                    .Select(ic => new SaltContribution
                    {
                        Ion = ic.Key.ToString(),
                        GramsPerMole = ic.Value
                    })
                    .ToList()
            }).ToList();

            // Save to database
            _context.Salts.AddRange(saltEntities);
            var saved = await _context.SaveChangesAsync();
            Console.WriteLine($"? Saved {saved} records to database");

            // Verify
            var count = await _context.Salts.CountAsync();
            Console.WriteLine($"? Database now contains {count} salts");
            
            // Print summary statistics
            var stats = await PrintDatabaseStatistics();
            Console.WriteLine($"\n{stats}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR initializing database: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Print database statistics
    /// </summary>
    private async Task<string> PrintDatabaseStatistics()
    {
        var sb = new StringBuilder();
        sb.AppendLine("?? Database Statistics:");

        var entities = await _context.Salts.ToListAsync();
        
        // Group by type
        var byType = entities.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count());
        sb.AppendLine($"  By Type:");
        foreach (var (type, count) in byType.OrderByDescending(x => x.Value))
        {
            sb.AppendLine($"    • {type}: {count}");
        }

        // Group by group (primary nutrient)
        var byGroup = entities.GroupBy(e => e.Group).ToDictionary(g => g.Key, g => g.Count());
        sb.AppendLine($"  By Primary Nutrient:");
        foreach (var (group, count) in byGroup.OrderByDescending(x => x.Value).Take(8))
        {
            sb.AppendLine($"    • {group}: {count}");
        }

        // Group by category
        var byCategory = entities.GroupBy(e => e.Category).ToDictionary(g => g.Key, g => g.Count());
        sb.AppendLine($"  By Category:");
        foreach (var (category, count) in byCategory.OrderByDescending(x => x.Value))
        {
            sb.AppendLine($"    • {category}: {count}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Find CSV files in the data/init folder
    /// </summary>
    private List<string> FindCsvFiles()
    {
        var baseDir = AppContext.BaseDirectory;
        
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "data", "init"),
            Path.Combine(baseDir, "..", "..", "..", "data", "init"),
            Path.Combine(baseDir, "..", "data", "init"),
            @"C:\Source\NutrientOptimizer\data\init"
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                var csvFiles = Directory.GetFiles(path, "*.csv").ToList();
                if (csvFiles.Count > 0)
                {
                    Console.WriteLine($"Found data folder at: {path}");
                    return csvFiles;
                }
            }
        }

        return new List<string>();
    }

    /// <summary>
    /// Load substances from a CSV file
    /// </summary>
    private List<Salt> LoadSubstancesFromCsv(string csvFilePath)
    {
        var substances = new List<Salt>();
        var lines = File.ReadAllLines(csvFilePath, Encoding.UTF8);

        if (lines.Length < 2)
            return substances;

        // Parse header row
        var headers = ParseCsvLine(lines[0]);
        var nameIdx = Array.IndexOf(headers, "NAME");
        var formulaIdx = Array.IndexOf(headers, "FORMULA");
        var typeIdx = Array.IndexOf(headers, "TYPE");
        var nNo3Idx = Array.IndexOf(headers, "N (NO3-)");
        var nNh4Idx = Array.IndexOf(headers, "N (NH4+)");
        var pIdx = Array.IndexOf(headers, "P");
        var kIdx = Array.IndexOf(headers, "K");
        var mgIdx = Array.IndexOf(headers, "MG");
        var caIdx = Array.IndexOf(headers, "CA");
        var sIdx = Array.IndexOf(headers, "S");
        var bIdx = Array.IndexOf(headers, "B");
        var feIdx = Array.IndexOf(headers, "FE");
        var znIdx = Array.IndexOf(headers, "ZN");
        var mnIdx = Array.IndexOf(headers, "MN");
        var cuIdx = Array.IndexOf(headers, "CU");
        var moIdx = Array.IndexOf(headers, "MO");
        var naIdx = Array.IndexOf(headers, "NA");
        var siIdx = Array.IndexOf(headers, "SI");
        var clIdx = Array.IndexOf(headers, "CL");

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            var cells = ParseCsvLine(lines[i]);
            if (cells.Length == 0 || string.IsNullOrWhiteSpace(cells[0]))
                continue;

            try
            {
                var name = SafeGetCell(cells, nameIdx)?.Trim() ?? "";
                var formula = SafeGetCell(cells, formulaIdx)?.Trim() ?? "";
                var typeStr = SafeGetCell(cells, typeIdx)?.Trim() ?? "Raw";

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var ionContributions = new Dictionary<Ion, double>();

                // Add ion contributions (percentage values from CSV)
                if (TryParseDouble(cells, nNo3Idx, out var v) && v > 0) ionContributions[Ion.Nitrate] = v;
                if (TryParseDouble(cells, nNh4Idx, out v) && v > 0) ionContributions[Ion.Ammonium] = v;
                if (TryParseDouble(cells, pIdx, out v) && v > 0) ionContributions[Ion.Phosphate] = v;
                if (TryParseDouble(cells, kIdx, out v) && v > 0) ionContributions[Ion.Potassium] = v;
                if (TryParseDouble(cells, mgIdx, out v) && v > 0) ionContributions[Ion.Magnesium] = v;
                if (TryParseDouble(cells, caIdx, out v) && v > 0) ionContributions[Ion.Calcium] = v;
                if (TryParseDouble(cells, sIdx, out v) && v > 0) ionContributions[Ion.Sulfate] = v;
                if (TryParseDouble(cells, bIdx, out v) && v > 0) ionContributions[Ion.Boron] = v;
                if (TryParseDouble(cells, feIdx, out v) && v > 0) ionContributions[Ion.Iron] = v;
                if (TryParseDouble(cells, znIdx, out v) && v > 0) ionContributions[Ion.Zinc] = v;
                if (TryParseDouble(cells, mnIdx, out v) && v > 0) ionContributions[Ion.Manganese] = v;
                if (TryParseDouble(cells, cuIdx, out v) && v > 0) ionContributions[Ion.Copper] = v;
                if (TryParseDouble(cells, moIdx, out v) && v > 0) ionContributions[Ion.Molybdenum] = v;
                if (TryParseDouble(cells, naIdx, out v) && v > 0) ionContributions[Ion.Sodium] = v;
                if (TryParseDouble(cells, siIdx, out v) && v > 0) ionContributions[Ion.Silicon] = v;
                if (TryParseDouble(cells, clIdx, out v) && v > 0) ionContributions[Ion.Chlorine] = v;

                // Skip if no ions
                if (ionContributions.Count == 0)
                    continue;

                // Determine category and group based on PRIMARY ion (highest concentration)
                var (category, group) = ClassifySubstance(ionContributions);

                // Parse Type (Raw or Commercial)
                var type = SubstanceType.Raw;
                if (typeStr.Equals("Commercial", StringComparison.OrdinalIgnoreCase))
                    type = SubstanceType.Commercial;

                var substance = new Salt
                {
                    Name = name,
                    Formula = formula,
                    MolecularWeight = 100.0,
                    Category = category,
                    Group = group,
                    Type = type,
                    IonContributions = ionContributions
                };

                substances.Add(substance);
            }
            catch 
            { 
            }
        }

        return substances;
    }

    /// <summary>
    /// Classify substance by its PRIMARY ion (highest concentration)
    /// </summary>
    private (SaltCategory category, SaltGroup group) ClassifySubstance(Dictionary<Ion, double> ionContributions)
    {
        if (ionContributions.Count == 0)
            return (SaltCategory.Macronutrient, SaltGroup.NitrogenSource);

        // Find the ion with highest concentration
        var primaryIon = ionContributions.OrderByDescending(x => x.Value).First().Key;

        // Classify based on primary ion
        return primaryIon switch
        {
            Ion.Nitrate or Ion.Ammonium => (SaltCategory.Macronutrient, SaltGroup.NitrogenSource),
            Ion.Phosphate => (SaltCategory.Macronutrient, SaltGroup.PhosphorusSource),
            Ion.Potassium => (SaltCategory.Macronutrient, SaltGroup.PotassiumSource),
            Ion.Calcium => (SaltCategory.Macronutrient, SaltGroup.CalciumSource),
            Ion.Magnesium => (SaltCategory.Macronutrient, SaltGroup.MagnesiumSource),
            Ion.Sulfate => (SaltCategory.Macronutrient, SaltGroup.SulfurSource),
            Ion.Iron => (SaltCategory.Micronutrient, SaltGroup.IronSource),
            Ion.Manganese => (SaltCategory.Micronutrient, SaltGroup.ManganeseSource),
            Ion.Zinc => (SaltCategory.Micronutrient, SaltGroup.ZincSource),
            Ion.Copper => (SaltCategory.Micronutrient, SaltGroup.CopperSource),
            Ion.Boron => (SaltCategory.Micronutrient, SaltGroup.BoronSource),
            Ion.Molybdenum => (SaltCategory.Micronutrient, SaltGroup.MolybdenumSource),
            _ => (SaltCategory.Macronutrient, SaltGroup.NitrogenSource)
        };
    }

    /// <summary>
    /// Parse a CSV line handling quoted fields
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(c);
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    /// <summary>
    /// Safely get a cell value from a CSV row
    /// </summary>
    private string? SafeGetCell(string[] cells, int index)
    {
        if (index < 0 || index >= cells.Length)
            return null;
        return cells[index];
    }

    /// <summary>
    /// Try to parse a double value from CSV (handling European decimal format)
    /// </summary>
    private bool TryParseDouble(string[] cells, int index, out double value)
    {
        value = 0;
        if (index < 0 || index >= cells.Length)
            return false;

        var cell = cells[index]?.Trim().Replace(',', '.') ?? "";
        if (string.IsNullOrWhiteSpace(cell))
            return false;

        return double.TryParse(cell, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }
}
