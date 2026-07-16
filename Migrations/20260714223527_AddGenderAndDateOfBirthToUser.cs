using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddGenderAndDateOfBirthToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 1,
                column: "TierName",
                value: "Tech-Newie");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 2,
                column: "TierName",
                value: "Tech-Member");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 3,
                column: "TierName",
                value: "Tech-VIP");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 1,
                column: "TierName",
                value: "Tech-NULL");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 2,
                column: "TierName",
                value: "Tech-NEW");

            migrationBuilder.UpdateData(
                table: "MembershipTiers",
                keyColumn: "TierID",
                keyValue: 3,
                column: "TierName",
                value: "Tech-MEM");
        }
    }
}
