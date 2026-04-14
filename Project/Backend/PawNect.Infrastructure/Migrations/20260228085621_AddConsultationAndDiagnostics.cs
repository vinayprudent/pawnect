using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PawNect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationAndDiagnostics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VetId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    VetId = table.Column<int>(type: "int", nullable: false),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisionalDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrescriptionUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VitalsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsultationComplete = table.Column<bool>(type: "bit", nullable: false),
                    DiagnosticsRecommended = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultations_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultations_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Consultations_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LabTestCatalogItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SampleType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestsIncludedJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiagnosticOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationId = table.Column<int>(type: "int", nullable: false),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    VetId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CollectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedLabName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SampleCollectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosticOrders_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagnosticOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosticOrderId = table.Column<int>(type: "int", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LabTestCatalogItemId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagnosticOrderLines_DiagnosticOrders_DiagnosticOrderId",
                        column: x => x.DiagnosticOrderId,
                        principalTable: "DiagnosticOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OwnerId",
                table: "Appointments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VetId",
                table: "Appointments",
                column: "VetId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_AppointmentId",
                table: "Consultations",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_OwnerId",
                table: "Consultations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_PetId",
                table: "Consultations",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticOrderLines_DiagnosticOrderId",
                table: "DiagnosticOrderLines",
                column: "DiagnosticOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticOrders_ConsultationId",
                table: "DiagnosticOrders",
                column: "ConsultationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_OwnerId",
                table: "Appointments",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_VetId",
                table: "Appointments",
                column: "VetId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_OwnerId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_VetId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "DiagnosticOrderLines");

            migrationBuilder.DropTable(
                name: "LabTestCatalogItems");

            migrationBuilder.DropTable(
                name: "DiagnosticOrders");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_OwnerId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_VetId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "VetId",
                table: "Appointments");
        }
    }
}
