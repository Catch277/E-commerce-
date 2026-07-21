using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWeb.Migrations
{
    public partial class AddHomePageSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "CategoryID1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Products",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    NewsArticleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.NewsArticleID);
                });

            migrationBuilder.CreateTable(
                name: "PrebuiltPcs",
                columns: table => new
                {
                    PrebuiltPcID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsBestSeller = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrebuiltPcs", x => x.PrebuiltPcID);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarrantyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrebuiltPcSpecs",
                columns: table => new
                {
                    PrebuiltPcSpecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrebuiltPcID = table.Column<int>(type: "int", nullable: false),
                    SpecText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrebuiltPcSpecs", x => x.PrebuiltPcSpecID);
                    table.ForeignKey(
                        name: "FK_PrebuiltPcSpecs_PrebuiltPcs_PrebuiltPcID",
                        column: x => x.PrebuiltPcID,
                        principalTable: "PrebuiltPcs",
                        principalColumn: "PrebuiltPcID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 1, "CPU", "Bộ vi xử lý trung tâm" },
                    { 2, "VGA", "Card đồ họa" },
                    { 3, "RAM", "Bộ nhớ truy cập ngẫu nhiên" },
                    { 4, "SSD", "Ổ cứng thể rắn" },
                    { 5, "Màn hình", "Màn hình máy tính" },
                    { 6, "Bàn phím", "Bàn phím máy tính" },
                    { 7, "Chuột", "Chuột máy tính" }
                });

            migrationBuilder.InsertData(
                table: "NewsArticles",
                columns: new[] { "NewsArticleID", "Category", "Excerpt", "ImageUrl", "IsPublished", "PublishedAt", "Title" },
                values: new object[,]
                {
                    { 1, "Kinh nghiệm", "Lựa chọn card đồ họa luôn là câu hỏi đau đầu nhất khi build PC. Cùng tìm hiểu cách chọn VGA tối ưu...", "https://cdn2.fptshop.com.vn/unsafe/1920x0/filters:format(webp):quality(75)/nen_mua_card_man_hinh_nao_de_choi_game_2026_thumb_3323eef885.png", true, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hướng dẫn chọn Card Màn Hình (VGA) phù hợp cho nhu cầu chơi game 2026" },
                    { 2, "Tin tức công nghệ", "Đánh giá chi tiết hiệu năng nâng cấp của dòng xử lý mới nhất từ Intel và xem liệu nó có đáng để bạn xuống...", "https://cdn-media.sforum.vn/storage/app/media/trannghia/trannghia/1/intel-core-series-3-wildcat-lake-01.jpg", true, new DateTime(2026, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Intel ra mắt dòng CPU thế hệ mới: Có thực sự đáng nâng cấp?" },
                    { 3, "Thủ thuật", "Những tinh chỉnh đơn giản trên hệ điều hành giúp tắt các dịch vụ không cần thiết, giải phóng RAM và tăng...", "https://kimlongcenter.com/upload/image/Thu%20Thuat/C%C3%A1ch%20t%E1%BB%91i%20%C6%B0u%20%C4%91%E1%BB%83%20ch%C6%A1i%20game%20m%C6%B0%E1%BB%A3t%20h%C6%A1n/c%C3%A1ch-t%E1%BB%91i-%C6%B0u-%C4%91%E1%BB%83-ch%C6%A1i-game-m%C6%B0%E1%BB%A3t-h%C6%A1n.jpg", true, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "5 Cách tối ưu hóa hệ điều hành để chơi game mượt mà hơn" }
                });

            migrationBuilder.InsertData(
                table: "PrebuiltPcs",
                columns: new[] { "PrebuiltPcID", "DisplayOrder", "ImageUrl", "IsBestSeller", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "https://www.tncstore.vn/media/product/11995-pc-gaming-nova-a5070-bl--3-.jpg", false, "Budget King", 15990000m },
                    { 2, 2, "https://mygear.io.vn/media/product/7830-pc-gaming-cyberpower-rtx-5060-i5-12400f-u-1.png", true, "Mid-Range Beast", 28500000m },
                    { 3, 3, "https://breunor.com/cdn/shop/files/rossa_4649c942-d446-4369-9cc6-db35e9026377.jpg?v=1771404016&width=1024", false, "Ultimate Performance", 95000000m }
                });

            migrationBuilder.InsertData(
                table: "PrebuiltPcSpecs",
                columns: new[] { "PrebuiltPcSpecID", "DisplayOrder", "PrebuiltPcID", "SpecText" },
                values: new object[,]
                {
                    { 1, 1, 1, "Intel Core i5-12400F" },
                    { 2, 2, 1, "RAM 16GB DDR4 3200MHz" },
                    { 3, 3, 1, "VGA RTX 3060 12GB" },
                    { 4, 4, 1, "SSD 500GB NVMe" },
                    { 5, 1, 2, "Intel Core i5-13600K" },
                    { 6, 2, 2, "RAM 32GB DDR5 5600MHz" },
                    { 7, 3, 2, "VGA RTX 4060 Ti 8GB" },
                    { 8, 4, 2, "SSD 1TB NVMe Gen4" },
                    { 9, 1, 3, "Intel Core i9-14900K" },
                    { 10, 2, 3, "RAM 64GB DDR5 6000MHz" },
                    { 11, 3, 3, "VGA RTX 4090 24GB" },
                    { 12, 4, 3, "SSD 2TB NVMe Gen4" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "CategoryID", "CategoryID1", "CreatedAt", "Description", "ImageUrl", "IsNew", "OldPrice", "Price", "ProductName", "Quantity", "Rating", "ReviewCount" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CPU Intel Core i5-13400F 10 nhân 16 luồng, tốc độ 2.5GHz, hỗ trợ RAM DDR5, socket LGA1700, không tích hợp đồ họa.", "https://cdn2.cellphones.com.vn/x/media/catalog/product/c/p/cpu-intel-core-i5-13400f-tray_2_.png", false, 4690000m, 4290000m, "CPU Intel Core i5-13400F", 0, 4.7999999999999998, 32 },
                    { 2, 2, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Card đồ họa Gigabyte RTX 4060 Ti Super 8GB GDDR6, 128-bit, hỗ trợ DLSS 3, Ray Tracing, xuất hình 4K.", "https://product.hstatic.net/200000722513/product/9899_e1b98e8a5f63e754f57497ecf79488f5_292572e5779f431098d1236845acc095_fb8c3e65086e43388656e5b4ea0c138a_master.jpg", true, 11490000m, 9990000m, "VGA GIGABYTE GeForce RTX 4060 Ti Super", 0, 4.9000000000000004, 18 },
                    { 3, 3, null, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "RAM Kingston Fury Beast 16GB (1x16GB) DDR5 6000MHz, CL40, hỗ trợ XMP 3.0, tản nhiệt màu đen.", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQyHfoa5rzla1de4bt8URqa5v1qS-QumqoFRGh-FauusKr_2PsO1d8qleM&s=10", true, null, 1590000m, "RAM Kingston Fury Beast 16GB DDR5 6000MHz", 0, 4.7000000000000002, 45 },
                    { 4, 4, null, new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "SSD Samsung 990 PRO 2TB, PCIe 4.0 x4, tốc độ đọc lên đến 7450 MB/s, ghi 6900 MB/s, hỗ trợ công nghệ V-NAND và tản nhiệt hiệu quả.", "https://bizweb.dktcdn.net/thumb/1024x1024/100/329/122/products/ssd-samsung-990-pro-pcie-gen-4-0-x4-nvme-with-heatsink-905954f6-13ab-4baf-8150-e682df092cb6.jpg?v=1696582447337", true, 5290000m, 4850000m, "SSD Samsung 990 PRO 2TB PCIe NVMe 4.0x4", 0, 5.0, 27 },
                    { 5, 5, null, new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Màn hình LG UltraGear 27 inch OLED, độ phân giải 2560x1440, tần số quét 240Hz, thời gian phản hồi 0.03ms, hỗ trợ HDR10.", "https://cdn2.cellphones.com.vn/x/media/catalog/product/m/a/man-hinh-lg-ultragear-oled-27gr95qe-b-27-inch-1.png", true, null, 22990000m, "Màn hình LG UltraGear 27GR95QE-B 27\" OLED", 0, 4.9000000000000004, 12 },
                    { 6, 6, null, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bàn phím cơ Logitech G Pro X với switch GX Brown tùy chỉnh, đèn RGB, kích thước nhỏ gọn, chuyên cho game thủ.", "https://www.logitechg.com/content/dam/gaming/en/products/pro-x-keyboard/pro-x-keyboard-gallery-1.png", true, null, 2490000m, "Bàn phím cơ Logitech G Pro X", 0, 4.5999999999999996, 20 },
                    { 7, 7, null, new DateTime(2026, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chuột gaming Logitech G502 HERO với cảm biến HERO 25K, 11 nút lập trình, dây cáp, đèn RGB, trọng lượng có thể điều chỉnh.", "https://product.hstatic.net/200000722513/product/10001_01736316d2b443d0838e5a0741434420_master.png", true, 1590000m, 1290000m, "Chuột gaming Logitech G502 HERO", 0, 4.7999999999999998, 35 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID1",
                table: "Products",
                column: "CategoryID1");

            migrationBuilder.CreateIndex(
                name: "IX_PrebuiltPcSpecs_PrebuiltPcID",
                table: "PrebuiltPcSpecs",
                column: "PrebuiltPcID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID1",
                table: "Products",
                column: "CategoryID1",
                principalTable: "Categories",
                principalColumn: "CategoryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID1",
                table: "Products");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "PrebuiltPcSpecs");

            migrationBuilder.DropTable(
                name: "WarrantyTickets");

            migrationBuilder.DropTable(
                name: "PrebuiltPcs");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryID1",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "CategoryID1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
