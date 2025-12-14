using Microsoft.EntityFrameworkCore;

namespace NutrientOptimizer.Core.Data;

public class SaltRepository
{
    private readonly NutrientDbContext _db;

    public SaltRepository(NutrientDbContext db) => _db = db;

    public async Task<IReadOnlyList<Salt>> GetAllSaltsAsync()
    {
        var entities = await _db.Salts
            .Include(s => s.Contributions)
            .ToListAsync();

        return entities.Select(e => new Salt
        {
            Name = e.Name,
            Formula = e.Formula,
            MolecularWeight = e.MolecularWeight,
            IonContributions = e.Contributions.ToDictionary(c => c.Ion, c => c.GramsPerMole)
        }).ToList();
    }
}