using BurglarBuster.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Infrastructure;

/// <summary>
/// EF Core context over the synthetic person dataset. There is deliberately no
/// generic write-capable repository built on top of this — person data access is
/// read-only by design after seeding (see CLAUDE.md).
/// </summary>
public sealed class BurglarBusterDbContext(DbContextOptions<BurglarBusterDbContext> options)
    : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();

    public DbSet<PersonAlias> PersonAliases => Set<PersonAlias>();

    public DbSet<PersonAddress> PersonAddresses => Set<PersonAddress>();

    public DbSet<PhysicalDescription> PhysicalDescriptions => Set<PhysicalDescription>();

    public DbSet<CaseReference> CaseReferences => Set<CaseReference>();

    public DbSet<Investigation> Investigations => Set<Investigation>();

    public DbSet<InvestigationCandidate> InvestigationCandidates => Set<InvestigationCandidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.PersonId);
            entity.Property(p => p.PersonId).HasMaxLength(16);
            entity.Property(p => p.GivenName).HasMaxLength(100);
            entity.Property(p => p.MiddleNames).HasMaxLength(150);
            entity.Property(p => p.FamilyName).HasMaxLength(100);
            entity.HasIndex(p => p.FamilyName);
            entity.HasIndex(p => new { p.FamilyName, p.GivenName });
        });

        modelBuilder.Entity<PersonAlias>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.GivenName).HasMaxLength(100);
            entity.Property(a => a.FamilyName).HasMaxLength(100);
            entity.Property(a => a.FullName).HasMaxLength(200);
            entity.HasIndex(a => a.FullName);
            entity.HasOne(a => a.Person)
                .WithMany(p => p.Aliases)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonAddress>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AddressLine).HasMaxLength(200);
            entity.Property(a => a.Locality).HasMaxLength(100);
            entity.Property(a => a.State).HasMaxLength(50);
            entity.Property(a => a.Postcode).HasMaxLength(10);
            entity.HasIndex(a => a.Locality);
            entity.HasOne(a => a.Person)
                .WithMany(p => p.Addresses)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PhysicalDescription>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.EyeColour).HasMaxLength(30);
            entity.Property(d => d.HairColour).HasMaxLength(30);
            entity.Property(d => d.DistinguishingMarks).HasMaxLength(300);
            entity.HasOne(d => d.Person)
                .WithMany(p => p.PhysicalDescriptions)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CaseReference>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ReferenceNumber).HasMaxLength(30);
            entity.Property(c => c.Summary).HasMaxLength(300);
            entity.Property(c => c.Status).HasMaxLength(30);
            entity.HasOne(c => c.Person)
                .WithMany(p => p.CaseReferences)
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Investigation>(entity =>
        {
            entity.HasKey(i => i.InvestigationId);
            entity.Property(i => i.InvestigationId).HasMaxLength(40);
            entity.Property(i => i.ModelDeploymentId).HasMaxLength(100);
            entity.HasIndex(i => i.CreatedAtUtc);
        });

        // No FK to Person: audit/history data intentionally doesn't hard-couple to the
        // person aggregate's schema (see DECISIONS.md).
        modelBuilder.Entity<InvestigationCandidate>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.PersonId).HasMaxLength(16);
            entity.HasIndex(c => c.PersonId);
            entity.HasOne(c => c.Investigation)
                .WithMany(i => i.Candidates)
                .HasForeignKey(c => c.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
