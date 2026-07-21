using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class MakeOrderVoucherNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Vouchers_VoucherID",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "VoucherID",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Vouchers_VoucherID",
                table: "Orders",
                column: "VoucherID",
                principalTable: "Vouchers",
                principalColumn: "VoucherID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Vouchers_VoucherID",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "VoucherID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Vouchers_VoucherID",
                table: "Orders",
                column: "VoucherID",
                principalTable: "Vouchers",
                principalColumn: "VoucherID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
