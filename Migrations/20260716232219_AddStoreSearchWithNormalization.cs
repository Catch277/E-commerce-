using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddStoreSearchWithNormalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OpenHours",
                table: "Stores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNormalized",
                table: "Stores",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DistrictNormalized",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Stores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 1,
                columns: new[] { "Address", "AddressNormalized", "District", "DistrictNormalized", "Latitude", "Longitude", "Name", "NameNormalized", "Phone" },
                values: new object[] { "123 Xuân Thủy, Dịch Vọng Hậu, Cầu Giấy, Hà Nội", "123 xuan thuy, dich vong hau, cau giay, ha noi", "Cầu Giấy", "cau giay", 21.036799999999999, 105.78270000000001, "PC Master - Cầu Giấy", "pc master - cau giay", "024 1234 5678" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 2,
                columns: new[] { "Address", "AddressNormalized", "District", "DistrictNormalized", "Latitude", "Longitude", "Name", "NameNormalized", "Phone" },
                values: new object[] { "45 Thái Hà, Trung Liệt, Đống Đa, Hà Nội", "45 thai ha, trung liet, dong da, ha noi", "Đống Đa", "dong da", 21.0136, 105.82129999999999, "PC Master - Đống Đa", "pc master - dong da", "024 8765 4321" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 3,
                columns: new[] { "Address", "AddressNormalized", "District", "DistrictNormalized", "Latitude", "Longitude", "Name", "NameNormalized", "Phone" },
                values: new object[] { "78 Lê Thanh Nghị, Bách Khoa, Hai Bà Trưng, Hà Nội", "78 le thanh nghi, bach khoa, hai ba trung, ha noi", "Hai Bà Trưng", "hai ba trung", 21.002099999999999, 105.8437, "PC Master - Hai Bà Trưng", "pc master - hai ba trung", "024 2468 1357" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 4,
                columns: new[] { "Address", "AddressNormalized", "District", "DistrictNormalized", "Latitude", "Longitude", "Name", "NameNormalized", "Phone" },
                values: new object[] { "234 Quang Trung, Hà Đông, Hà Nội", "234 quang trung, ha dong, ha noi", "Hà Đông", "ha dong", 20.971699999999998, 105.777, "PC Master - Hà Đông", "pc master - ha dong", "024 1357 2468" });

            migrationBuilder.InsertData(
                table: "Stores",
                columns: new[] { "StoreID", "Address", "AddressNormalized", "District", "DistrictNormalized", "IsActive", "Latitude", "Longitude", "Name", "NameNormalized", "OpenHours", "Phone" },
                values: new object[,]
                {
                    { 5, "56 Điện Biên Phủ, Phường 15, Bình Thạnh, TP. Hồ Chí Minh", "56 dien bien phu, phuong 15, binh thanh, tp. ho chi minh", "Bình Thạnh", "binh thanh", true, 10.803100000000001, 106.715, "PC Master - Bình Thạnh", "pc master - binh thanh", "08:00 - 21:30", "028 3512 6789" },
                    { 6, "88 Nguyễn Huệ, Bến Nghé, Quận 1, TP. Hồ Chí Minh", "88 nguyen hue, ben nghe, quan 1, tp. ho chi minh", "Quận 1", "quan 1", true, 10.775600000000001, 106.7025, "PC Master - Quận 1", "pc master - quan 1", "08:00 - 22:00", "028 3822 4567" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "AddressNormalized",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "DistrictNormalized",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Stores");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "OpenHours",
                table: "Stores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 1,
                columns: new[] { "Address", "District", "Latitude", "Longitude", "Name", "Phone" },
                values: new object[] { "250 Lê Hồng Phong, Phường 4, Quận 10, TP. Hồ Chí Minh", "Quận 10", 10.762499999999999, 106.66800000000001, "PC Master - Quận 10", "028 3833 1234" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 2,
                columns: new[] { "Address", "District", "Latitude", "Longitude", "Name", "Phone" },
                values: new object[] { "123 Nguyễn Thị Minh Khai, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh", "Quận 1", 10.772500000000001, 106.69499999999999, "PC Master - Quận 1", "028 3925 5678" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 3,
                columns: new[] { "Address", "District", "Latitude", "Longitude", "Name", "Phone" },
                values: new object[] { "456 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh", "Bình Thạnh", 10.795, 106.715, "PC Master - Bình Thạnh", "028 3512 9876" });

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "StoreID",
                keyValue: 4,
                columns: new[] { "Address", "District", "Latitude", "Longitude", "Name", "Phone" },
                values: new object[] { "789 Nguyễn Ảnh Thủ, Phường Hiệp Thành, Quận 12, TP. Hồ Chí Minh", "Quận 12", 10.85, 106.63, "PC Master - Quận 12", "028 3717 3456" });
        }
    }
}
