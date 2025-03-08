using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ImageProcessing.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VariantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxSize = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VariantTypeId = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageVariants_VariantTypes_VariantTypeId",
                        column: x => x.VariantTypeId,
                        principalTable: "VariantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "VariantTypes",
                columns: new[] { "Id", "MaxSize", "Name", "Quality" },
                values: new object[,]
                {
                    { 1, 0, "Original", 100 },
                    { 2, 2048, "TwoK", 85 },
                    { 3, 1200, "Web", 75 },
                    { 4, 768, "Mobile", 70 },
                    { 5, 300, "Thumbnail", 65 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageVariants_VariantTypeId",
                table: "ImageVariants",
                column: "VariantTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageVariants");

            migrationBuilder.DropTable(
                name: "VariantTypes");
        }
    }
}
