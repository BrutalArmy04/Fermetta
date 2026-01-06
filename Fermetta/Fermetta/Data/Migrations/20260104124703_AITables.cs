using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fermetta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AITables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAssistantLogs",
                columns: table => new
                {
                    ProductAssistantLog_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Id = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAssistantLogs", x => x.ProductAssistantLog_Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductFaqs",
                columns: table => new
                {
                    ProductFaq_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Id = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AskedCount = table.Column<int>(type: "int", nullable: false),
                    LastAskedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFaqs", x => x.ProductFaq_Id);
                    table.ForeignKey(
                        name: "FK_ProductFaqs_Products_Product_Id",
                        column: x => x.Product_Id,
                        principalTable: "Products",
                        principalColumn: "Product_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductFaqs_Product_Id",
                table: "ProductFaqs",
                column: "Product_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAssistantLogs");

            migrationBuilder.DropTable(
                name: "ProductFaqs");
        }
    }
}
