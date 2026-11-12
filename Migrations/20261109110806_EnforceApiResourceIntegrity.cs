using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class EnforceApiResourceIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_MakeAndModel_NotEmpty",
                table: "Vehicles",
                sql: "length(trim(\"Make\")) > 0 AND length(trim(\"Model\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PolicyCoverages_PremiumComponents",
                table: "PolicyCoverages",
                sql: "\"AdditionalPremium\" = \"LimitPremium\" + \"DeductiblePremiumAdjustment\"");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Policies_PolicyNumber_NotEmpty",
                table: "Policies",
                sql: "length(trim(\"PolicyNumber\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InsuranceApplications_UsageAndStorage_NotEmpty",
                table: "InsuranceApplications",
                sql: "length(trim(\"Usage\")) > 0 AND length(trim(\"StorageType\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_NameAndEmail_NotEmpty",
                table: "Customers",
                sql: "length(trim(\"FirstName\")) > 0 AND length(trim(\"LastName\")) > 0 AND length(trim(\"Email\")) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_MakeAndModel_NotEmpty",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PolicyCoverages_PremiumComponents",
                table: "PolicyCoverages");

            migrationBuilder.DropIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Policies_PolicyNumber_NotEmpty",
                table: "Policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InsuranceApplications_UsageAndStorage_NotEmpty",
                table: "InsuranceApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_NameAndEmail_NotEmpty",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies",
                column: "ApplicationId");
        }
    }
}
