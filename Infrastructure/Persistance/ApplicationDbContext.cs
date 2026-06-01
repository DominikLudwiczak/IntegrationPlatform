using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public virtual DbSet<OperationRecord> OperationRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<OperationRecord>()
            .Property(x => x.Progress)
            .HasColumnType("int")
            .IsRequired();

        builder.Entity<OperationRecord>()
            .ToTable(t => t.HasCheckConstraint("CK_Progress_Range", "\"Progress\" >= 0 AND \"Progress\" <= 100"));

        builder.Entity<OperationRecord>()
            .ToTable(t => t.HasCheckConstraint("CK_RetryCount", "\"RetryCount\" <= \"MaxRetries\""));
    }
}