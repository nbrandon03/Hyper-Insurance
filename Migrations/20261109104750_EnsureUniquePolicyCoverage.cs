using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class EnsureUniquePolicyCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PolicyCoverages_PolicyId",
                table: "PolicyCoverages");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyCoverages_PolicyId_CoverageId",
                table: "PolicyCoverages",
                columns: new[] { "PolicyId", "CoverageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PolicyCoverages_PolicyId_CoverageId",
                table: "PolicyCoverages");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyCoverages_PolicyId",
                table: "PolicyCoverages",
                column: "PolicyId");
        }
    }
}
