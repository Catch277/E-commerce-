# PC Store — Design System and Product UI Brief

## Source of truth

This document describes the UI already implemented in this repository. It is grounded in the MVC views, `wwwroot/css/site.css`, `wwwroot/css/register.css`, page-local profile styles, models, controllers, services, and migrations.

- The repository has no `README` and no `wwwroot/styles` directory. The active shared style source is `wwwroot/css/site.css`.
- No product records or product names are present in the repository. Product cards must render `ProductName`, `Category.CategoryName`, `Price`, `ImageUrl`, and `Description` from the database; do not supply sample products in product UI.
- The public product is named **PC Store** in the layout, header, hero, footer, and metadata. The payment success copy also says **TechStore**; retain the existing route-specific copy unless the product naming is deliberately reconciled in code.

## Product shape and hierarchy

PC Store is a Vietnamese computer-components catalog and purchasing service. Its native structure is:

```text
Discover catalog → browse category/search → inspect product → add to cart
                                     └→ test payment through VNPay or MoMo
Sign in → account summary → order history / membership progress and benefits
```

The home page should lead with the existing sales proposition, then discovery, then the newest four products. This follows `HomeController.Index`, which selects the four most recently created products, rather than a generic marketing landing-page sequence.

1. Global header: brand, product search, account action, cart.
2. Category navigation: Trang chủ, Tất cả, CPU, VGA, RAM, SSD, Màn hình, Bàn phím, Chuột.
3. Hero: `HOT SALE - GIẢM ĐẾN 30%`, `Build Your Dream PC`, then `Mua ngay` and `Khám phá`.
4. `Danh mục nổi bật`: CPU, VGA, RAM, SSD.
5. `Sản phẩm nổi bật`: the four newest database products.
6. Footer: category links, customer support, contact details, and social links.

The account area is not a secondary catalog. It is a compact operational dashboard organized around user identity, order status, spending, points, tier progression, and tier benefits.

## Brand expression

The public-storefront identity is dark graphite with a warm yellow purchase accent: a technical, gaming-oriented shop rather than a restrained luxury retailer.

- Dark header and hero establish the primary visual field.
- Yellow is reserved for brand emphasis, product/category icons, prices/rating stars where already used, primary purchase actions, and key metrics.
- Red communicates urgency and transactional emphasis: `HOT SALE - GIẢM ĐẾN 30%`, `NEW`, sale prices, cart-count badge, errors, and order/account actions.
- White cards float above a very light blue-gray page background.
- Storefront interactions lift cards and buttons slightly rather than changing their basic shape.

The signed-in profile and membership views intentionally use a different, red-led account pattern. Their pale red active state (`#fcebeb`), red action color (`#d70018`), white cards, and quiet gray background are established in the existing views. Do not silently replace that pattern with storefront yellow; unify it only through an explicit product-wide redesign.

## Foundations

### Typography

- Primary family: `Inter`, loaded in the shared layout at weights 300, 400, 500, 600, and 700.
- Global body text: the browser/Bootstrap base size with `Inter` and `#212529`.
- Display hero: Bootstrap `display-4`, bold; `Dream PC` uses yellow.
- Section and product headings: bold Bootstrap headings. Product titles are `17px`, weight 600, line-height `1.4`, limited to two lines in cards.
- Auth headings: `2.5rem` on the left panel and `2rem` on the form panel; both weight 700.
- Small supporting labels: Bootstrap `small`, category labels, rating count, status badges (`0.78rem`), and profile labels.

Use the project’s actual Vietnamese labels, including `Tất cả sản phẩm`, `Kết quả tìm kiếm`, `Lịch sử mua hàng`, `Hạng thành viên và ưu đãi`, and payment status text. Preserve the existing `đ`/`VNĐ` currency conventions on their respective screens.

### Color tokens

| Role                 | Value     | Established use                                              |
| -------------------- | --------- | ------------------------------------------------------------ |
| Primary yellow       | `#ffc107` | `--primary`; logo, primary buttons, icons, emphasis          |
| Primary yellow hover | `#e0a800` | `--primary-hover`; primary hover state                       |
| Dark                 | `#111827` | `--dark`; header, hero/auth left panel, dark inputs          |
| Dark secondary       | `#1f2937` | `--dark2`; hero gradient end, footer, auth card              |
| Page light           | `#f5f7fb` | `--light`; storefront page background                        |
| Text                 | `#212529` | `--text`; global body text                                   |
| Profile canvas       | `#f4f6f8` | profile, order history, and membership pages                 |
| Account action red   | `#FFC107` | profile active states, membership progress, account emphasis |
| Account active fill  | `#FFF3C2` | profile hover/active fill and membership icon hover          |
| Success green        | `#10b981` | authenticated password-rule success                          |
| Footer/support gray  | `#adb5bd` | footer links and supporting copy                             |

Membership data also provides tier colors, not arbitrary decorative colors: Tech-Newie `#e0e0e0`, Tech-Member `#f5c3a6`, and Tech-VIP `#f5d17e`.

### Surface, border, and elevation

- Storefront card radius: `18px` (`--radius`). Category and product cards are white, borderless, and use `0 8px 25px rgba(0,0,0,.08)`.
- Storefront hover elevation: `0 18px 40px rgba(0,0,0,.15)` and `translateY(-8px)`.
- Hero radius: `24px`; auth card: `1rem`; profile cards: `12px`; order items/tier boxes: `10px`; compact controls: `8px` or `10px`.
- Profile card shadow: `0 1px 4px rgba(0,0,0,.05)`; membership card shadow: `0 2px 8px rgba(0,0,0,.05)`.
- Auth inputs and password-rule panel use `#374151` borders against a dark field.

### Spacing and layout

- Shared content uses Bootstrap `.container`, `.row`, and responsive column classes.
- Storefront hero: `70px 50px` padding, `30px auto 60px` margin, minimum height `520px`.
- Catalog and feature grids use Bootstrap `g-4`; product cards are four columns at `xl`, three at `lg`, and two at `md`/`sm`.
- Product image area: `240px` tall, `25px` inset, with image max-height `180px`.
- Auth panels: `4rem` padding on desktop; `3rem 2rem` when stacked.
- Profile layout: sidebar `col-lg-3`, content `col-lg-9`, with scrollable quick links on narrow widths.

## Components

### Global shell

The header is sticky (`top: 0`, `z-index: 1030`) and dark. It holds:

- `PC Store` in bold yellow;
- the search prompt `Tìm CPU, VGA, RAM, SSD...` and a yellow search button;
- `Đăng nhập`, or the authenticated `Chào, {name}!` menu;
- `Giỏ hàng` with the existing red `0` badge.

Below it, the yellow navigation bar contains the category destinations. The footer is dark secondary and repeats the product name, category/support links, `pcstore@gmail.com`, `0123 456 789`, `TP. Hồ Chí Minh`, and `08:00 - 22:00`.

### Hero

Use the existing dark-to-dark-secondary 135° gradient, yellow translucent circle at the upper right, white supporting copy, and a two-column desktop layout. The hero’s exact message is:

> Chuyên cung cấp linh kiện máy tính, Gaming Gear và phụ kiện chính hãng với mức giá cạnh tranh cùng chính sách bảo hành uy tín.

Keep its existing paired actions: yellow filled `Mua ngay` and light outline `Khám phá`. The implementation currently uses a remote placeholder image labeled `Gaming PC`; it is an existing asset reference, not content to expand with invented imagery.

The metrics are existing promotional content: `500+ Sản phẩm`, `5K+ Khách hàng`, and `24/7 Hỗ trợ`.

### Category card

Use a white, borderless, 18px-rounded card with a large yellow Bootstrap icon, bold category title, and muted descriptor. The implemented categories and descriptors are:

| Category | Descriptor   | Icon            |
| -------- | ------------ | --------------- |
| CPU      | Intel • AMD  | `bi-cpu`        |
| VGA      | NVIDIA • AMD | `bi-gpu-card`   |
| RAM      | DDR4 • DDR5  | `bi-memory`     |
| SSD      | NVMe • SATA  | `bi-device-ssd` |

Hover: lift `8px`, raise shadow, and scale the icon to `1.15`.

### Product card and catalog

The product record is the content source. Each card contains:

1. The fixed `NEW` red badge.
2. A linked product image (database `ImageUrl`, falling back to `/images/no-image.png` when empty).
3. Uppercase muted category name.
4. The database product name, clamped to two lines.
5. Existing presentation-only rating text: `★★★★★ (5.0)`.
6. A struck-through price calculated as `Price × 1.15` and a red current database price.
7. `Chi tiết` and `Thêm vào giỏ` actions.

Catalog list pages preserve their existing data-aware framing:

- `Tất cả sản phẩm` shows the controller’s total, paginates eight products per page, newest first.
- Category pages show `Danh mục: {CategoryName}` and the category count.
- Search shows `Kết quả tìm kiếm` and `Từ khóa: {keyword}`; when empty it shows `Không tìm thấy sản phẩm.`
- The product detail view puts image and product data in a 5/7 desktop split and ends with `🛒 Thêm vào giỏ`.

### Buttons and feedback

- Storefront primary actions use Bootstrap yellow (`btn-warning`), medium/bold weight, and a `10px` radius in product cards or `12px` in the hero.
- General button hover is a subtle `translateY(-2px)` movement over `.25s`.
- Card transitions are `.3s`/`.35s`; no large or unrelated animation.
- Global success/error feedback uses Bootstrap alerts and a SweetAlert toast in the upper right for three seconds.
- Payment outcomes use large success/error icons and clear `Giao dịch thành công!` or `Giao dịch thất bại!` headings, followed by `Về trang chủ`.

### Authentication

Authentication is a dark two-panel card on the light canvas. The left is narrative and uses yellow heading text; the right is the form. Use existing form labels and validation only:

- Registration: `Họ và tên`, `Email`, `Địa chỉ`, `Số điện thoại`, `Mật khẩu`, `Xác nhận mật khẩu`.
- Phone entry begins with `+84` and accepts nine following digits.
- Password rules: at least 6 characters, 1 uppercase letter, 1 number, and 1 special character. The progress bar moves through danger, warning, and success.
- Login includes `Ghi nhớ đăng nhập` and its persistent session behavior.

On screens at or below `768px`, stack the narrative panel above the form.

### Account, orders, and membership

Profile pages use the profile-specific white-card/red-action vocabulary. The account summary surfaces the actual `FullName`, phone, member tier, `Tổng số đơn hàng đã mua`, and `Tổng tiền tích lũy`.

Quick links and sidebar terminology must stay aligned with the code: `Tổng quan`, `Lịch sử mua hàng`, `Tra cứu bảo hành`, `Hạng thành viên và ưu đãi`, `Thông tin tài khoản`, `Tìm kiếm cửa hàng`, and `Góp ý - Phản hồi`.

Order history is an operational list, not a product grid:

- Filter by `Tất cả`, `Chờ xác nhận`, `Đang xử lý`, `Đang vận chuyển`, `Đã nhận hàng`, and `Đã huỷ`.
- Filter with the existing date range.
- Each order row exposes order number, time, first-product thumbnail/name, product count, total, status, and `Xem chi tiết`.
- Status colors are semantic: yellow for waiting/processing, blue for shipping, green for received, red for cancelled.

The membership screen is a progress path driven by actual records:

- Tiers in ascending `MinPoints`: Tech-Newie (0), Tech-Member (300), Tech-VIP (1500).
- One point equals `10.000đ` in spend; payment success adds `floor(TotalAmount / 10000)` points.
- The Swiper tier cards distinguish `Hạng hiện tại`, `Đã mở khóa`, and `Cần thêm {n} điểm`; the selected card uses the red preview outline and scale.
- Benefits are data-driven. Current seeded content for Tech-Member is `Tặng voucher 50K khi lên hạng` and `Giảm thêm 0.5% khi mua linh kiện PC`.

## Responsive behavior

- At `≤992px`, center hero text, reduce hero padding to `50px 30px`, and position the hero image below with `40px` top margin.
- At `≤768px`, use hero `40px 20px` padding, a `2rem` hero heading, automatic product-title height, a vertical authentication card, centered footer, and more compact navigation spacing.
- The desktop category/product grids already collapse through Bootstrap’s column definitions; retain those breakpoints rather than introducing a separate grid system.
- Profile quick links remain horizontally scrollable; tier cards use Swiper with auto-width slides and the existing navigation buttons.

## Implementation constraints and consistency notes

- Reuse Bootstrap 5 layout and controls, Bootstrap Icons, and `Inter`; those are loaded globally. Profile templates also use Font Awesome class names, but Font Awesome is not loaded in the shared layout. Treat this as an implementation dependency to resolve before relying on those icons for new UI.
- Keep all content backed by existing view text, database fields, or configuration/model terminology. There is no checked-in catalog seed data, so do not fabricate products, prices, reviews, addresses, support policies, or additional membership benefits.
- Preserve the current navigation and controller reality: the cart is a visual counter/link placeholder, `AddToCart` currently confirms with `Đã thêm sản phẩm vào giỏ hàng.`, payment is accessed through an order-ID test form, and payment methods include VNPay and MoMo.
- Do not expose configuration secrets in user-facing UI. Payment labels should remain `Thanh toán qua VNPay`, `Chuyển đến VNPay`, and the existing MoMo callback/success messages.
- Existing legacy defaults in `Views/Shared/_Layout.cshtml.css`, including blue `btn-primary`, coexist with the yellow storefront system. New storefront UI should draw from the shared CSS tokens above; only retain blue where a current Bootstrap payment/success view already intentionally uses `btn-primary`.
