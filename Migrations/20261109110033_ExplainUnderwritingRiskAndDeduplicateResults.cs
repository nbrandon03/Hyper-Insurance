using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class ExplainUnderwritingRiskAndDeduplicateResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults");

            migrationBuilder.Sql(
                """
                DELETE FROM "UnderwritingResults" AS older
                USING "UnderwritingResults" AS newer
                WHERE older."ApplicationId" = newer."ApplicationId"
                  AND (
                    newer."EvaluatedAt" > older."EvaluatedAt"
                    OR (
                        newer."EvaluatedAt" = older."EvaluatedAt"
                        AND newer."Id" > older."Id"
                    )
                  );
                """);

            migrationBuilder.CreateTable(
                name: "UnderwritingRiskFactors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnderwritingResultId = table.Column<int>(type: "integer", nullable: false),
                    Rule = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnderwritingRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnderwritingRiskFactors_UnderwritingResults_UnderwritingRes~",
                        column: x => x.UnderwritingResultId,
                        principalTable: "UnderwritingResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingRiskFactors_UnderwritingResultId",
                table: "UnderwritingRiskFactors",
                column: "UnderwritingResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnderwritingRiskFactors");

            migrationBuilder.DropIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults");

            migrationBuilder.CreateIndex(
                name: "IX_UnderwritingResults_ApplicationId",
                table: "UnderwritingResults",
                column: "ApplicationId");
        }
    }
}
