using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fermetta.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_Products_ProductId",
                table: "ProductReviews");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductReviews",
                newName: "Product_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductReviews_ProductId_UserId",
                table: "ProductReviews",
                newName: "IX_ProductReviews_Product_Id_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_Products_Product_Id",
                table: "ProductReviews",
                column: "Product_Id",
                principalTable: "Products",
                principalColumn: "Product_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_Products_Product_Id",
                table: "ProductReviews");

            migrationBuilder.RenameColumn(
                name: "Product_Id",
                table: "ProductReviews",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductReviews_Product_Id_UserId",
                table: "ProductReviews",
                newName: "IX_ProductReviews_ProductId_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_Products_ProductId",
                table: "ProductReviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Product_Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
