using Microsoft.EntityFrameworkCore;

namespace NutrientOptimizer.Core.Data;

public class NutrientDbContext : DbContext
{
    public DbSet<SaltEntity> Salts { get; set; } = null!;

    public NutrientDbContext(DbContextOptions<NutrientDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure SaltEntity -> Ion enum as string key
        modelBuilder.Entity<SaltEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SaltContribution>()
            .HasKey(sc => new { sc.SaltId, sc.Ion });

        modelBuilder.Entity<SaltContribution>()
            .Property(sc => sc.Ion)
            .HasConversion<string>();
    }
}

public class SaltEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public double MolecularWeight { get; set; }

    public List<SaltContribution> Contributions { get; set; } = new();
}

public class SaltContribution
{
    public int SaltId { get; set; }
    public Ion Ion { get; set; }
    public double GramsPerMole { get; set; }  // elemental mass per mole of salt
}