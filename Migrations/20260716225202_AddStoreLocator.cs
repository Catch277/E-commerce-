using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddStoreLocator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OpenHours = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreID);
                });

            migrationBuilder.InsertData(
                table: "Stores",
                columns: new[] { "StoreID", "Address", "District", "IsActive", "Latitude", "Longitude", "Name", "OpenHours", "Phone" },
                values: new object[,]
                {
                    { 1, "250 Lê Hồng Phong, Phường 4, Quận 10, TP. Hồ Chí Minh", "Quận 10", true, 10.762499999999999, 106.66800000000001, "PC Master - Quận 10", "08:00 - 21:00", "028 3833 1234" },
                    { 2, "123 Nguyễn Thị Minh Khai, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh", "Quận 1", true, 10.772500000000001, 106.69499999999999, "PC Master - Quận 1", "08:30 - 21:30", "028 3925 5678" },
                    { 3, "456 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh", "Bình Thạnh", true, 10.795, 106.715, "PC Master - Bình Thạnh", "08:00 - 21:00", "028 3512 9876" },
                    { 4, "789 Nguyễn Ảnh Thủ, Phường Hiệp Thành, Quận 12, TP. Hồ Chí Minh", "Quận 12", true, 10.85, 106.63, "PC Master - Quận 12", "08:30 - 21:30", "028 3717 3456" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
