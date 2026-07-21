using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddMembershipSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TierID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MembershipTiers",
                columns: table => new
                {
                    TierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinPoints = table.Column<int>(type: "int", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTiers", x => x.TierID);
                });

            migrationBuilder.CreateTable(
                name: "PointsHistories",
                columns: table => new
                {
                    PointsHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    PointsChange = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsHistories", x => x.PointsHistoryID);
                    table.ForeignKey(
                        name: "FK_PointsHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TierBenefits",
                columns: table => new
                {
                    BenefitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierBenefits", x => x.BenefitID);
                    table.ForeignKey(
                        name: "FK_TierBenefits_MembershipTiers_TierID",
                        column: x => x.TierID,
                        principalTable: "MembershipTiers",
                        principalColumn: "TierID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MembershipTiers",
                columns: new[] { "TierID", "ColorHex", "IconClass", "MinPoints", "TierName" },
                values: new object[] { 1, "#e0e0e0", "fa-solid fa-user", 0, "Tech-NULL" });

            migrationBuilder.InsertData(
                table: "MembershipTiers",
                columns: new[] { "TierID", "ColorHex", "IconClass", "MinPoints", "TierName" },
                values: new object[] { 2, "#f5c3a6", "fa-solid fa-star", 300, "Tech-NEW" });

            migrationBuilder.InsertData(
                table: "MembershipTiers",
                columns: new[] { "TierID", "ColorHex", "IconClass", "MinPoints", "TierName" },
                values: new object[] { 3, "#f5d17e", "fa-solid fa-crown", 1500, "Tech-MEM" });

            migrationBuilder.InsertData(
                table: "TierBenefits",
                columns: new[] { "BenefitID", "Description", "IconClass", "TierID" },
                values: new object[] { 1, "Tặng voucher 50K khi lên hạng", "fa-solid fa-ticket", 2 });

            migrationBuilder.InsertData(
                table: "TierBenefits",
                columns: new[] { "BenefitID", "Description", "IconClass", "TierID" },
                values: new object[] { 2, "Giảm thêm 0.5% khi mua linh kiện PC", "fa-solid fa-gift", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TierID",
                table: "Users",
                column: "TierID");

            migrationBuilder.CreateIndex(
                name: "IX_PointsHistories_UserID",
                table: "PointsHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TierBenefits_TierID",
                table: "TierBenefits",
                column: "TierID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_MembershipTiers_TierID",
                table: "Users",
                column: "TierID",
                principalTable: "MembershipTiers",
                principalColumn: "TierID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_MembershipTiers_TierID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "PointsHistories");

            migrationBuilder.DropTable(
                name: "TierBenefits");

            migrationBuilder.DropTable(
                name: "MembershipTiers");

            migrationBuilder.DropIndex(
                name: "IX_Users_TierID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TierID",
                table: "Users");
        }
    }
}
