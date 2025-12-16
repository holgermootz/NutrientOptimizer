using Microsoft.EntityFrameworkCore;

namespace NutrientOptimizer.Core.Data;

public class NutrientDbContext : DbContext
{
    public DbSet<SaltEntity> Salts { get; set; } = null!;

    public NutrientDbContext(DbContextOptions<NutrientDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure SaltEntity
        modelBuilder.Entity<SaltEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SaltEntity>()
            .Property(s => s.Category)
            .HasConversion<string>();

        modelBuilder.Entity<SaltEntity>()
            .Property(s => s.Group)
            .HasConversion<string>();

        modelBuilder.Entity<SaltEntity>()
            .Property(s => s.Type)
            .HasConversion<string>();

        modelBuilder.Entity<SaltEntity>()
            .HasMany(s => s.Contributions)
            .WithOne(c => c.Salt)
            .HasForeignKey(sc => sc.SaltId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SaltContribution with surrogate PK and unique constraint
        modelBuilder.Entity<SaltContribution>()
            .HasKey(sc => sc.Id);

        modelBuilder.Entity<SaltContribution>()
            .HasIndex(sc => new { sc.SaltId, sc.Ion })
            .IsUnique();

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
    public string Category { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Type { get; set; } = "Raw";

    public List<SaltContribution> Contributions { get; set; } = new();
}

public class SaltContribution
{
    public int Id { get; set; }
    public int SaltId { get; set; }
    public string Ion { get; set; } = string.Empty;
    public double GramsPerMole { get; set; }
    
    public SaltEntity Salt { get; set; } = null!;
}