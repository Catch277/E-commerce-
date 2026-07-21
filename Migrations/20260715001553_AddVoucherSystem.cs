using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddVoucherSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordUpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    VoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.VoucherID);
                });

            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    UserVoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    VoucherID = table.Column<int>(type: "int", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedInOrderID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVouchers", x => x.UserVoucherID);
                    table.ForeignKey(
                        name: "FK_UserVouchers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVouchers_Vouchers_VoucherID",
                        column: x => x.VoucherID,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Vouchers",
                columns: new[] { "VoucherID", "Code", "Description", "DiscountType", "DiscountValue", "ExpiryDate", "IsActive", "MaxDiscountAmount", "MinOrderValue", "Title", "UsageLimit", "UsedCount" },
                values: new object[] { 1, "WELCOME50", "Chào mừng thành viên mới", "Fixed", 50000m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null, 200000m, "Giảm 50.000đ", null, 0 });

            migrationBuilder.InsertData(
                table: "Vouchers",
                columns: new[] { "VoucherID", "Code", "Description", "DiscountType", "DiscountValue", "ExpiryDate", "IsActive", "MaxDiscountAmount", "MinOrderValue", "Title", "UsageLimit", "UsedCount" },
                values: new object[] { 2, "SALE10", "Áp dụng cho đơn hàng từ 500.000đ", "Percent", 10m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 100000m, 500000m, "Giảm 10%", 1000, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_UserID_VoucherID",
                table: "UserVouchers",
                columns: new[] { "UserID", "VoucherID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_VoucherID",
                table: "UserVouchers",
                column: "VoucherID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVouchers");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropColumn(
                name: "PasswordUpdatedAt",
                table: "Users");
        }
    }
}
