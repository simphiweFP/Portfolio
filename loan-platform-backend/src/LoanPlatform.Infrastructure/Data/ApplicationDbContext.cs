using LoanPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoanPlatform.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<LoanApplication> LoanApplications { get; set; }
    public DbSet<LoanType> LoanTypes { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ApplicationHistory> ApplicationHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        });

        // Configure LoanApplication entity
        builder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApplicationNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Purpose).HasMaxLength(500);
            entity.Property(e => e.EmploymentStatus).HasMaxLength(100);
            entity.Property(e => e.EmployerName).HasMaxLength(200);
            entity.Property(e => e.ReviewNotes).HasMaxLength(1000);
            
            entity.Property(e => e.RequestedAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MonthlyIncome).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MonthlyExpenses).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.MonthlyPayment).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.LoanApplications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewerUser)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewerUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.LoanType)
                  .WithMany(lt => lt.LoanApplications)
                  .HasForeignKey(e => e.LoanTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure LoanType entity
        builder.Entity<LoanType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.Property(e => e.MinAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BaseInterestRate).HasColumnType("decimal(5,2)");
        });

        // Configure Document entity
        builder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.LoanApplication)
                  .WithMany(la => la.Documents)
                  .HasForeignKey(e => e.LoanApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UploadedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Notification entity
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.RelatedEntityType).HasMaxLength(100);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ApplicationHistory entity
        builder.Entity<ApplicationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.LoanApplication)
                  .WithMany(la => la.History)
                  .HasForeignKey(e => e.LoanApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChangedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ChangedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed LoanTypes
        builder.Entity<LoanType>().HasData(
            new LoanType
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Personal Loan",
                Description = "Unsecured personal loan for various purposes",
                MinAmount = 1000,
                MaxAmount = 50000,
                BaseInterestRate = 8.5m,
                MinTermMonths = 6,
                MaxTermMonths = 60,
                IsActive = true
            },
            new LoanType
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Home Loan",
                Description = "Secured loan for purchasing or refinancing a home",
                MinAmount = 50000,
                MaxAmount = 1000000,
                BaseInterestRate = 4.5m,
                MinTermMonths = 120,
                MaxTermMonths = 360,
                IsActive = true
            },
            new LoanType
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Auto Loan",
                Description = "Secured loan for purchasing a vehicle",
                MinAmount = 5000,
                MaxAmount = 100000,
                BaseInterestRate = 6.0m,
                MinTermMonths = 24,
                MaxTermMonths = 84,
                IsActive = true
            }
        );
    }
}