using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotPolicyCoverageTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverageDescription",
                table: "PolicyCoverages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverageName",
                table: "PolicyCoverages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "PolicyCoverages" AS pc
                SET "CoverageName" = coverage."Name",
                    "CoverageDescription" = coverage."Description"
                FROM "Coverages" AS coverage
                WHERE coverage."Id" = pc."CoverageId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverageDescription",
                table: "PolicyCoverages");

            migrationBuilder.DropColumn(
                name: "CoverageName",
                table: "PolicyCoverages");
        }
    }
}
