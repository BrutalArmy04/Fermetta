using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fermetta.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_Category_Id1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category_Id1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Category_Id1",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category_Id",
                table: "Products",
                column: "Category_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_Category_Id",
                table: "Products",
                column: "Category_Id",
                principalTable: "Categories",
                principalColumn: "Category_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_Category_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category_Id",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Category_Id1",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category_Id1",
                table: "Products",
                column: "Category_Id1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_Category_Id1",
                table: "Products",
                column: "Category_Id1",
                principalTable: "Categories",
                principalColumn: "Category_Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
