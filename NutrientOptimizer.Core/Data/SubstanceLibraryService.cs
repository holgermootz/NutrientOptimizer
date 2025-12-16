using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Core.Data;

/// <summary>
/// Singleton service for managing the substance library.
/// Provides caching and easy access to imported substances throughout the application.
/// </summary>
public class SubstanceLibraryService
{
    private static SubstanceLibraryService? _instance;
    private List<Salt>? _cachedSubstances;
    private readonly string _dbfPath;
    private DateTime _lastLoadTime = DateTime.MinValue;
    private const int CacheValidityMinutes = 60;
    private bool _loadFailed = false;

    private SubstanceLibraryService(string dbfPath)
    {
        _dbfPath = dbfPath;
    }

    /// <summary>
    /// Initialize the substance library service with the path to the DBF file
    /// </summary>
    public static void Initialize(string dbfPath)
    {
        _instance = new SubstanceLibraryService(dbfPath);
    }

    /// <summary>
    /// Get the singleton instance (must call Initialize first)
    /// </summary>
    public static SubstanceLibraryService Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    "SubstanceLibraryService not initialized. Call Initialize() first.");
            return _instance;
        }
    }

    /// <summary>
    /// Get all substances, using cache if valid
    /// </summary>
    public List<Salt> GetAllSubstances()
    {
        // If load previously failed, return empty list
        if (_loadFailed)
        {
            return new List<Salt>();
        }

        // Return cached data if still valid
        if (_cachedSubstances != null && 
            DateTime.UtcNow.Subtract(_lastLoadTime).TotalMinutes < CacheValidityMinutes)
        {
            return _cachedSubstances;
        }

        try
        {
            // Load fresh data
            _cachedSubstances = SubstanceImporter.ImportFromDbf(_dbfPath);
            _lastLoadTime = DateTime.UtcNow;

            if (_cachedSubstances == null || _cachedSubstances.Count == 0)
            {
                Console.WriteLine("WARNING: DBF import returned no substances");
                _cachedSubstances = new List<Salt>();
                _loadFailed = true;
            }

            return _cachedSubstances;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in GetAllSubstances: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            _loadFailed = true;
            _cachedSubstances = new List<Salt>();
            return new List<Salt>();
        }
    }

    /// <summary>
    /// Clear the cache and force reload next time
    /// </summary>
    public void ClearCache()
    {
        _cachedSubstances = null;
        _lastLoadTime = DateTime.MinValue;
        _loadFailed = false;
    }

    /// <summary>
    /// Get substances filtered by category
    /// </summary>
    public List<Salt> GetByCategory(SaltCategory category)
    {
        return GetAllSubstances()
            .Where(s => s.Category == category)
            .ToList();
    }

    /// <summary>
    /// Get substances filtered by group
    /// </summary>
    public List<Salt> GetByGroup(SaltGroup group)
    {
        return GetAllSubstances()
            .Where(s => s.Group == group)
            .ToList();
    }

    /// <summary>
    /// Get substances that contain a specific ion
    /// </summary>
    public List<Salt> GetByIon(Ion ion)
    {
        return GetAllSubstances()
            .Where(s => s.IonContributions.ContainsKey(ion))
            .ToList();
    }

    /// <summary>
    /// Search substances by name or formula
    /// </summary>
    public List<Salt> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllSubstances();

        var lowerQuery = query.ToLowerInvariant();
        return GetAllSubstances()
            .Where(s => s.Name.ToLowerInvariant().Contains(lowerQuery) ||
                        s.Formula.ToLowerInvariant().Contains(lowerQuery))
            .ToList();
    }

    /// <summary>
    /// Find a specific substance by name
    /// </summary>
    public Salt? FindByName(string name)
    {
        return GetAllSubstances()
            .FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get statistics about the substance library
    /// </summary>
    public LibraryStatistics GetStatistics()
    {
        var substances = GetAllSubstances();
        var macros = substances.Where(s => s.Category == SaltCategory.Macronutrient).ToList();
        var micros = substances.Where(s => s.Category == SaltCategory.Micronutrient).ToList();

        return new LibraryStatistics
        {
            TotalSubstances = substances.Count,
            MacronutrientCount = macros.Count,
            MicronutrientCount = micros.Count,
            GroupBreakdown = substances
                .GroupBy(s => s.Group)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastLoadTime = _lastLoadTime
        };
    }
}

/// <summary>
/// Statistics about the substance library
/// </summary>
public class LibraryStatistics
{
    public int TotalSubstances { get; set; }
    public int MacronutrientCount { get; set; }
    public int MicronutrientCount { get; set; }
    public Dictionary<SaltGroup, int> GroupBreakdown { get; set; } = new();
    public DateTime LastLoadTime { get; set; }

    public override string ToString()
    {
        var lines = new List<string>
        {
            $"Substance Library Statistics:",
            $"  Total: {TotalSubstances}",
            $"  Macronutrients: {MacronutrientCount}",
            $"  Micronutrients: {MicronutrientCount}",
            $"  Loaded at: {LastLoadTime:yyyy-MM-dd HH:mm:ss}",
            $"",
            $"By Group:"
        };

        foreach (var (group, count) in GroupBreakdown.OrderBy(x => x.Key.ToString()))
        {
            lines.Add($"  {group}: {count}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
