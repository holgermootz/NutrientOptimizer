using Microsoft.EntityFrameworkCore;
using NutrientOptimizer.Core;
using NutrientOptimizer.Core.Data;

namespace NutrientOptimizer.Web.Services;

/// <summary>
/// Service to retrieve salts from the database
/// </summary>
public class SaltDatabaseService
{
    private readonly NutrientDbContext _context;

    public SaltDatabaseService(NutrientDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all salts from the database as Salt objects
    /// </summary>
    public List<Salt> GetAllSalts()
    {
        try
        {
            var saltEntities = _context.Salts.Include(s => s.Contributions).ToList();

            var salts = saltEntities.Select(entity => new Salt
            {
                Name = entity.Name,
                Formula = entity.Formula,
                MolecularWeight = entity.MolecularWeight,
                Category = Enum.Parse<SaltCategory>(entity.Category, ignoreCase: true),
                Group = Enum.Parse<SaltGroup>(entity.Group, ignoreCase: true),
                Type = Enum.Parse<SubstanceType>(entity.Type, ignoreCase: true),
                IonContributions = entity.Contributions
                    .ToDictionary(c => Enum.Parse<Ion>(c.Ion, ignoreCase: true), c => c.GramsPerMole)
            }).ToList();

            Console.WriteLine($"Loaded {salts.Count} salts from database");
            return salts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR getting salts from database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<Salt>();
        }
    }
}
