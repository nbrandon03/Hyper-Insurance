using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDatabaseRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceApplications_CustomerId",
                table: "InsuranceApplications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceApplications_VehicleId",
                table: "InsuranceApplications",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceApplications_Customers_CustomerId",
                table: "InsuranceApplications",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceApplications_Vehicles_VehicleId",
                table: "InsuranceApplications",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_InsuranceApplications_ApplicationId",
                table: "Policies",
                column: "ApplicationId",
                principalTable: "InsuranceApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UnderwritingResults_InsuranceApplications_ApplicationId",
                table: "UnderwritingResults",
                column: "ApplicationId",
                principalTable: "InsuranceApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceApplications_Customers_CustomerId",
                table: "InsuranceApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceApplications_Vehicles_VehicleId",
                table: "InsuranceApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_InsuranceApplications_ApplicationId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_UnderwritingResults_InsuranceApplications_ApplicationId",
                table: "UnderwritingResults");

            migrationBuilder.DropIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults");

            migrationBuilder.DropIndex(
                name: "IX_Policies_ApplicationId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceApplications_CustomerId",
                table: "InsuranceApplications");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceApplications_VehicleId",
                table: "InsuranceApplications");
        }
    }
}
