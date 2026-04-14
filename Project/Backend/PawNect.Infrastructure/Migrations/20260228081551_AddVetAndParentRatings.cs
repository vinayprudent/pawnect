using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PawNect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVetAndParentRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParentRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentUserId = table.Column<int>(type: "int", nullable: false),
                    VetId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VetRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VetId = table.Column<int>(type: "int", nullable: false),
                    ParentUserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VetRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentRatings_ParentUserId_VetId_BookingId",
                table: "ParentRatings",
                columns: new[] { "ParentUserId", "VetId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VetRatings_VetId_ParentUserId_BookingId",
                table: "VetRatings",
                columns: new[] { "VetId", "ParentUserId", "BookingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentRatings");

            migrationBuilder.DropTable(
                name: "VetRatings");
        }
    }
}
