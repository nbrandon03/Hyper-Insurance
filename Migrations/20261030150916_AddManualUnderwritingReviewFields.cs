using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyper_Insurance.Migrations
{
    /// <inheritdoc />
    public partial class AddManualUnderwritingReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "UnderwritingResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "UnderwritingResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "UnderwritingResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "UnderwritingResults");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "UnderwritingResults");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "UnderwritingResults");
        }
    }
}
