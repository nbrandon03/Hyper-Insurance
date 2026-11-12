using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCoverageProductTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Deductible",
                table: "PolicyCoverages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeductiblePremiumAdjustment",
                table: "PolicyCoverages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LimitPremium",
                table: "PolicyCoverages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE "PolicyCoverages"
                SET "LimitPremium" = "AdditionalPremium";

                UPDATE "PolicyCoverages" AS pc
                SET "Deductible" = p."Deductible"
                FROM "Policies" AS p
                WHERE p."Id" = pc."PolicyId";
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "Deductible",
                table: "Policies",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateTable(
                name: "CoverageDeductibleOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoverageId = table.Column<int>(type: "integer", nullable: false),
                    Deductible = table.Column<decimal>(type: "numeric", nullable: false),
                    PremiumAdjustment = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverageDeductibleOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverageDeductibleOptions_Coverages_CoverageId",
                        column: x => x.CoverageId,
                        principalTable: "Coverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoverageLimitOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoverageId = table.Column<int>(type: "integer", nullable: false),
                    CoverageLimit = table.Column<decimal>(type: "numeric", nullable: false),
                    AdditionalPremium = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverageLimitOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverageLimitOptions_Coverages_CoverageId",
                        column: x => x.CoverageId,
                        principalTable: "Coverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoverageDeductibleOptions_CoverageId_Deductible",
                table: "CoverageDeductibleOptions",
                columns: new[] { "CoverageId", "Deductible" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoverageLimitOptions_CoverageId_CoverageLimit",
                table: "CoverageLimitOptions",
                columns: new[] { "CoverageId", "CoverageLimit" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "CoverageLimitOptions" ("CoverageId", "CoverageLimit", "AdditionalPremium")
                SELECT "Id", "CoverageLimit", "AdditionalPremium"
                FROM "Coverages";

                INSERT INTO "CoverageDeductibleOptions" ("CoverageId", "Deductible", "PremiumAdjustment")
                SELECT "Id", 1000, 0
                FROM "Coverages";
                """);

            migrationBuilder.DropColumn(
                name: "AdditionalPremium",
                table: "Coverages");

            migrationBuilder.DropColumn(
                name: "CoverageLimit",
                table: "Coverages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deductible",
                table: "PolicyCoverages");

            migrationBuilder.DropColumn(
                name: "DeductiblePremiumAdjustment",
                table: "PolicyCoverages");

            migrationBuilder.DropColumn(
                name: "LimitPremium",
                table: "PolicyCoverages");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalPremium",
                table: "Coverages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CoverageLimit",
                table: "Coverages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE "Coverages" AS coverage
                SET "CoverageLimit" = option."CoverageLimit",
                    "AdditionalPremium" = option."AdditionalPremium"
                FROM "CoverageLimitOptions" AS option
                WHERE option."Id" = (
                    SELECT first_option."Id"
                    FROM "CoverageLimitOptions" AS first_option
                    WHERE first_option."CoverageId" = coverage."Id"
                    ORDER BY first_option."Id"
                    LIMIT 1
                );

                UPDATE "Policies"
                SET "Deductible" = 0
                WHERE "Deductible" IS NULL;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "Deductible",
                table: "Policies",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "CoverageDeductibleOptions");

            migrationBuilder.DropTable(
                name: "CoverageLimitOptions");
        }
    }
}
