
using HyperInsurance.Models;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Vehicle> Vehicles { get; set; }

    public DbSet<InsuranceApplication> InsuranceApplications { get; set; }

    public DbSet<Policy> Policies { get; set; }

    public DbSet<UnderwritingResult> UnderwritingResults { get; set; }
    public DbSet<UnderwritingRiskFactor> UnderwritingRiskFactors { get; set; }
    public DbSet<Coverage> Coverages { get; set; }
    public DbSet<CoverageLimitOption> CoverageLimitOptions { get; set; }
    public DbSet<CoverageDeductibleOption> CoverageDeductibleOptions { get; set; }
    public DbSet<PolicyCoverage> PolicyCoverages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Customers_YearsLicensed_NonNegative",
                    "\"YearsLicensed\" >= 0");
                table.HasCheckConstraint(
                    "CK_Customers_NameAndEmail_NotEmpty",
                    "length(trim(\"FirstName\")) > 0 AND length(trim(\"LastName\")) > 0 AND length(trim(\"Email\")) > 0");
                table.HasCheckConstraint(
                    "CK_Customers_PreviousAccidents_NonNegative",
                    "\"PreviousAccidents\" >= 0");
                table.HasCheckConstraint(
                    "CK_Customers_TrafficViolations_NonNegative",
                    "\"TrafficViolations\" >= 0");
            });
            entity.HasIndex(customer => customer.Email);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Vehicles_Year_Range",
                    "\"Year\" >= 1886 AND \"Year\" <= 2200");
                table.HasCheckConstraint(
                    "CK_Vehicles_Value_NonNegative",
                    "\"Value\" >= 0");
                table.HasCheckConstraint(
                    "CK_Vehicles_Horsepower_NonNegative",
                    "\"Horsepower\" >= 0");
                table.HasCheckConstraint(
                    "CK_Vehicles_MakeAndModel_NotEmpty",
                    "length(trim(\"Make\")) > 0 AND length(trim(\"Model\")) > 0");
            });
        });

        modelBuilder.Entity<InsuranceApplication>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InsuranceApplication>()
            .HasOne(a => a.Vehicle)
            .WithMany(v => v.Applications)
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InsuranceApplication>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_InsuranceApplications_AnnualMileage_NonNegative",
                    "\"AnnualMileage\" >= 0");
                table.HasCheckConstraint(
                    "CK_InsuranceApplications_Status",
                    ApplicationStatuses.CheckConstraintSql);
                table.HasCheckConstraint(
                    "CK_InsuranceApplications_UsageAndStorage_NotEmpty",
                    "length(trim(\"Usage\")) > 0 AND length(trim(\"StorageType\")) > 0");
            });
            entity.HasIndex(application => new
            {
                application.Status,
                application.SubmittedAt
            });
        });

        modelBuilder.Entity<Policy>()
            .HasOne(p => p.Application)
            .WithMany(a => a.Policies)
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Policies_AnnualPremium_NonNegative",
                    "\"AnnualPremium\" >= 0");
                table.HasCheckConstraint(
                    "CK_Policies_Deductible_NonNegative",
                    "\"Deductible\" IS NULL OR \"Deductible\" >= 0");
                table.HasCheckConstraint(
                    "CK_Policies_DateRange",
                    "\"ExpiryDate\" >= \"StartDate\"");
                table.HasCheckConstraint(
                    "CK_Policies_Status",
                    PolicyStatuses.CheckConstraintSql);
                table.HasCheckConstraint(
                    "CK_Policies_PolicyNumber_NotEmpty",
                    "length(trim(\"PolicyNumber\")) > 0");
            });
            entity.HasIndex(policy => policy.ApplicationId).IsUnique();
            entity.HasIndex(policy => policy.PolicyNumber).IsUnique();
            entity.HasIndex(policy => new { policy.Status, policy.ExpiryDate });
        });

        modelBuilder.Entity<UnderwritingResult>()
            .HasOne(u => u.Application)
            .WithMany(a => a.UnderwritingResults)
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UnderwritingResult>()
            .HasIndex(u => u.ApplicationId)
            .IsUnique();

        modelBuilder.Entity<UnderwritingResult>()
            .ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_UnderwritingResults_Decision",
                    UnderwritingDecisions.CheckConstraintSql);
                table.HasCheckConstraint(
                    "CK_UnderwritingResults_RiskScore_NonNegative",
                    "\"RiskScore\" >= 0");
            });

        modelBuilder.Entity<UnderwritingRiskFactor>()
            .HasOne(factor => factor.UnderwritingResult)
            .WithMany(result => result.RiskFactors)
            .HasForeignKey(factor => factor.UnderwritingResultId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PolicyCoverage>()
            .ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PolicyCoverages_Limit_NonNegative",
                    "\"CoverageLimit\" > 0");
                table.HasCheckConstraint(
                    "CK_PolicyCoverages_Deductible_NonNegative",
                    "\"Deductible\" >= 0");
                table.HasCheckConstraint(
                    "CK_PolicyCoverages_LimitPremium_NonNegative",
                    "\"LimitPremium\" >= 0");
                table.HasCheckConstraint(
                    "CK_PolicyCoverages_PremiumComponents",
                    "\"AdditionalPremium\" = \"LimitPremium\" + \"DeductiblePremiumAdjustment\"");
            });

        modelBuilder.Entity<PolicyCoverage>()
            .HasOne(pc => pc.Policy)
            .WithMany(p => p.Coverages)
            .HasForeignKey(pc => pc.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PolicyCoverage>()
            .HasOne(pc => pc.Coverage)
            .WithMany()
            .HasForeignKey(pc => pc.CoverageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PolicyCoverage>()
            .HasIndex(pc => new { pc.PolicyId, pc.CoverageId })
            .IsUnique();

        modelBuilder.Entity<CoverageLimitOption>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_CoverageLimitOptions_Values",
                "\"CoverageLimit\" > 0 AND \"AdditionalPremium\" >= 0"));

        modelBuilder.Entity<CoverageLimitOption>()
            .HasOne(option => option.Coverage)
            .WithMany(coverage => coverage.LimitOptions)
            .HasForeignKey(option => option.CoverageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoverageLimitOption>()
            .HasIndex(option => new { option.CoverageId, option.CoverageLimit })
            .IsUnique();

        modelBuilder.Entity<CoverageDeductibleOption>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_CoverageDeductibleOptions_Deductible_NonNegative",
                "\"Deductible\" >= 0"));

        modelBuilder.Entity<CoverageDeductibleOption>()
            .HasOne(option => option.Coverage)
            .WithMany(coverage => coverage.DeductibleOptions)
            .HasForeignKey(option => option.CoverageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoverageDeductibleOption>()
            .HasIndex(option => new { option.CoverageId, option.Deductible })
            .IsUnique();

        modelBuilder.Entity<Coverage>()
            .HasIndex(coverage => new { coverage.IsActive, coverage.Name });
    }
}
