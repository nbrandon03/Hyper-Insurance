using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class AddApiDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_Horsepower_NonNegative",
                table: "Vehicles",
                sql: "\"Horsepower\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_Value_NonNegative",
                table: "Vehicles",
                sql: "\"Value\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_Year_Range",
                table: "Vehicles",
                sql: "\"Year\" >= 1886 AND \"Year\" <= 2200");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UnderwritingResults_Decision",
                table: "UnderwritingResults",
                sql: "\"Decision\" IN ('Pending', 'Approved', 'Declined', 'Referral')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UnderwritingResults_RiskScore_NonNegative",
                table: "UnderwritingResults",
                sql: "\"RiskScore\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PolicyCoverages_Deductible_NonNegative",
                table: "PolicyCoverages",
                sql: "\"Deductible\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PolicyCoverages_Limit_NonNegative",
                table: "PolicyCoverages",
                sql: "\"CoverageLimit\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PolicyCoverages_LimitPremium_NonNegative",
                table: "PolicyCoverages",
                sql: "\"LimitPremium\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Status_ExpiryDate",
                table: "Policies",
                columns: new[] { "Status", "ExpiryDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Policies_AnnualPremium_NonNegative",
                table: "Policies",
                sql: "\"AnnualPremium\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Policies_DateRange",
                table: "Policies",
                sql: "\"ExpiryDate\" >= \"StartDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Policies_Deductible_NonNegative",
                table: "Policies",
                sql: "\"Deductible\" IS NULL OR \"Deductible\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Policies_Status",
                table: "Policies",
                sql: "\"Status\" IN ('Active', 'Cancelled')");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceApplications_Status_SubmittedAt",
                table: "InsuranceApplications",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InsuranceApplications_AnnualMileage_NonNegative",
                table: "InsuranceApplications",
                sql: "\"AnnualMileage\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InsuranceApplications_Status",
                table: "InsuranceApplications",
                sql: "\"Status\" IN ('Pending', 'Approved', 'Declined', 'Referral')");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_PreviousAccidents_NonNegative",
                table: "Customers",
                sql: "\"PreviousAccidents\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_TrafficViolations_NonNegative",
                table: "Customers",
                sql: "\"TrafficViolations\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_YearsLicensed_NonNegative",
                table: "Customers",
                sql: "\"YearsLicensed\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Coverages_IsActive_Name",
                table: "Coverages",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoverageLimitOptions_Values",
                table: "CoverageLimitOptions",
                sql: "\"CoverageLimit\" > 0 AND \"AdditionalPremium\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoverageDeductibleOptions_Deductible_NonNegative",
                table: "CoverageDeductibleOptions",
                sql: "\"Deductible\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_Horsepower_NonNegative",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_Value_NonNegative",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_Year_Range",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UnderwritingResults_Decision",
                table: "UnderwritingResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UnderwritingResults_RiskScore_NonNegative",
                table: "UnderwritingResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PolicyCoverages_Deductible_NonNegative",
                table: "PolicyCoverages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PolicyCoverages_Limit_NonNegative",
                table: "PolicyCoverages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PolicyCoverages_LimitPremium_NonNegative",
                table: "PolicyCoverages");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_Status_ExpiryDate",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Policies_AnnualPremium_NonNegative",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Policies_DateRange",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Policies_Deductible_NonNegative",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Policies_Status",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceApplications_Status_SubmittedAt",
                table: "InsuranceApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InsuranceApplications_AnnualMileage_NonNegative",
                table: "InsuranceApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InsuranceApplications_Status",
                table: "InsuranceApplications");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_PreviousAccidents_NonNegative",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_TrafficViolations_NonNegative",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_YearsLicensed_NonNegative",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Coverages_IsActive_Name",
                table: "Coverages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoverageLimitOptions_Values",
                table: "CoverageLimitOptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoverageDeductibleOptions_Deductible_NonNegative",
                table: "CoverageDeductibleOptions");
        }
    }
}
