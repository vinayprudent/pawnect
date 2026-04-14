using Microsoft.EntityFrameworkCore;
using PawNect.Domain.Entities;
using PawNect.Domain.Enums;

namespace PawNect.Infrastructure.DbContext;

/// <summary>
/// PawNect Database Context
/// </summary>
public class PawNectDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public PawNectDbContext(DbContextOptions<PawNectDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<LabTestCatalogItem> LabTestCatalogItems { get; set; }
    public DbSet<DiagnosticOrder> DiagnosticOrders { get; set; }
    public DbSet<DiagnosticOrderLine> DiagnosticOrderLines { get; set; }
    public DbSet<DiagnosticReport> DiagnosticReports { get; set; }
    public DbSet<VetRating> VetRatings { get; set; }
    public DbSet<ParentRating> ParentRatings { get; set; }
    public DbSet<OtpChallenge> OtpChallenges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.OrganizationName).HasMaxLength(200);
            entity.Property(e => e.Role).HasConversion<int>();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(e => e.OwnedPets)
                .WithOne(p => p.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Pet configuration
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Breed).HasMaxLength(50);
            entity.Property(e => e.Species).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedPets)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.MedicalRecords)
                .WithOne(m => m.Pet)
                .HasForeignKey(m => m.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Appointments)
                .WithOne(a => a.Pet)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Medical Record configuration
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecordType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Cost).HasPrecision(18, 2);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AppointmentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Vet)
                .WithMany()
                .HasForeignKey(e => e.VetId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });

        // Consultation
        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Pet)
                .WithMany()
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // LabTestCatalogItem
        modelBuilder.Entity<LabTestCatalogItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TestType).HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // DiagnosticOrder
        modelBuilder.Entity<DiagnosticOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.CollectionType).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiagnosticOrderLine
        modelBuilder.Entity<DiagnosticOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.DiagnosticOrder)
                .WithMany(d => d.Lines)
                .HasForeignKey(e => e.DiagnosticOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiagnosticReport — one per diagnostic order
        modelBuilder.Entity<DiagnosticReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportFileUrl).HasMaxLength(500);
            entity.Property(e => e.ReportFileName).HasMaxLength(255);
            entity.Property(e => e.VetAdvice).HasMaxLength(4000);
            entity.Property(e => e.NextSteps).HasMaxLength(2000);
            entity.HasOne(e => e.DiagnosticOrder)
                .WithMany()
                .HasForeignKey(e => e.DiagnosticOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.DiagnosticOrderId).IsUnique();
        });

        // VetRating: parent rates vet
        modelBuilder.Entity<VetRating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.HasIndex(e => new { e.VetId, e.ParentUserId, e.BookingId }).IsUnique();
        });

        // ParentRating: vet rates parent
        modelBuilder.Entity<ParentRating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.HasIndex(e => new { e.ParentUserId, e.VetId, e.BookingId }).IsUnique();
        });

        // OtpChallenge
        modelBuilder.Entity<OtpChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChallengeId).IsRequired();
            entity.Property(e => e.Purpose).HasConversion<int>();
            entity.Property(e => e.Channel).HasConversion<int>();
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CodeHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.MetadataJson).HasMaxLength(4000);
            entity.HasIndex(e => e.ChallengeId).IsUnique();
            entity.HasIndex(e => new { e.Purpose, e.Destination, e.ConsumedAt, e.ExpiresAt });
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });
    }
}
