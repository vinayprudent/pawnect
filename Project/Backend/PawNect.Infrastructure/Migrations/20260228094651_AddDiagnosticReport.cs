using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PawNect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiagnosticReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosticOrderId = table.Column<int>(type: "int", nullable: false),
                    ReportFileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VetAdvice = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NextSteps = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosticReports_DiagnosticOrders_DiagnosticOrderId",
                        column: x => x.DiagnosticOrderId,
                        principalTable: "DiagnosticOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticReports_DiagnosticOrderId",
                table: "DiagnosticReports",
                column: "DiagnosticOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagnosticReports");
        }
    }
}
