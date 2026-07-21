using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddFavoriteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryID1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryID1",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => new { x.UserID, x.ProductID });
                    table.ForeignKey(
                        name: "FK_Favorites_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_ProductID",
                table: "Favorites",
                column: "ProductID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.AddColumn<int>(
                name: "CategoryID1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID1",
                table: "Products",
                column: "CategoryID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID1",
                table: "Products",
                column: "CategoryID1",
                principalTable: "Categories",
                principalColumn: "CategoryID");
        }
    }
}
