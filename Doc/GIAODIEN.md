# QUY ƯỚC GIAO DIỆN (RepairPro – Quản lý bảo hành & sửa chữa thiết bị)

> Tên thương hiệu "RepairPro" chỉ hiện ở logo sidebar và tab trình duyệt, nhóm đổi tùy ý. Đề tài không đặt nặng đồ họa (mục 17 của đề): ưu tiên **nhất quán, đúng nghiệp vụ, dễ bảo vệ**.

## 1. Ba quy tắc cốt lõi

1. **Không hard-code màu, cỡ chữ, bo góc trong `.cshtml`.** Chỉ dùng class có sẵn trong `wwwroot/css/site.css` (hoặc class Bootstrap 5.3). Không `style="color:#..."`.
2. **Trạng thái → badge luôn qua `UiHelper` + `_StatusBadge`.** Không tự viết `if (TrangThai == ...) class="..."` trong View. Một trạng thái chỉ có **một** màu trên toàn hệ thống.
3. **Muốn thêm/đổi màu, class, component dùng chung → sửa `site.css` trong Pull Request riêng và báo nhóm.** Không ai sửa riêng trong `<style>` của từng View.

## 2. Các file liên quan

| File                               | Vai trò                                                                                      |
| ---------------------------------- | -------------------------------------------------------------------------------------------- |
| `wwwroot/css/site.css`             | Design token (CSS variables) + ghi đè Bootstrap 5.3.3 + component dùng chung                 |
| `Helpers/UiHelper.cs`              | Ánh xạ enum → nhãn tiếng Việt + màu badge; định dạng tiền/ngày; tính "bảo hành", "hạn dự kiến", "tồn kho" |
| `Views/Shared/_StatusBadge.cshtml` | Partial vẽ badge từ `BadgeInfo`                                                              |
| `Views/Shared/_Layout.cshtml`      | Khung trang sau đăng nhập: sidebar + header + vùng nội dung + thông báo `TempData`           |
| `Views/Shared/_Sidebar.cshtml`     | Menu theo vai trò (Admin / Nhân viên kỹ thuật / Khách hàng)                                  |
| `Views/Shared/_AuthLayout.cshtml`  | Khung không có sidebar (Đăng nhập, 403/404/500)                                              |
| `docs/GIAO_DIEN.md`                | Chính file này                                                                               |

## 3. Bảng màu (tóm tắt)

Dùng qua class hoặc biến `var(--tên)`; mã hex chỉ để tra cứu.

| Nhóm                 | Token                    | Hex                   | Dùng cho                                                                 |
| -------------------- | ------------------------ | --------------------- | ------------------------------------------------------------------------ |
| **Primary (cobalt)** | `--primary-500`          | `#1F5FBF`             | Nút chính, link, tab active, focus                                       |
|                      | `--primary-600` / `-700` | `#174E9E` / `#103F82` | Hover / pressed                                                          |
|                      | `--primary-800`          | `#0B2F63`             | Nền sidebar                                                              |
|                      | `--primary-50` / `-100`  | `#EEF4FC` / `#D9E6F8` | Nền hover, hàng được chọn, chip                                          |
| **Accent (cam)**     | `--accent-500`           | `#F58220`             | Nút nghiệp vụ chính ("Tạo phiếu sửa chữa", "Tiếp nhận phiếu"), chấm thông báo |
|                      | `--accent-700`           | `#B3540A`             | **Chữ** cam trên nền trắng                                               |
| **Neutral**          | `--n-25`                 | `#F8FAFC`             | Nền trang                                                                |
|                      | `--n-0`                  | `#FFFFFF`             | Card, bảng, input                                                        |
|                      | `--n-100`                | `#E2E8F0`             | Viền card, đường kẻ                                                      |
|                      | `--n-500`                | `#64748B`             | Chữ phụ, placeholder                                                     |
|                      | `--n-600`                | `#5B6B80`             | Tiêu đề cột bảng                                                         |
|                      | `--n-900`                | `#0F172A`             | Chữ chính                                                                |
| **Ngữ nghĩa**        | `success`                | `#E7F6EC` / `#166534` | Hoàn thành, khả dụng, còn bảo hành, đang hoạt động                       |
|                      | `warning`                | `#FFF6DB` / `#92600A` | Chờ xử lý, sắp hết bảo hành, sắp đến hạn, sắp hết hàng                   |
|                      | `danger`                 | `#FDE9E7` / `#B42318` | Quá hạn, khẩn cấp, từ chối, lỗi, hết hàng, bị khóa, xóa                  |
|                      | `info`                   | `#E3F0FF` / `#1D4ED8` | Đang xử lý, đang sửa chữa                                                |
|                      | `muted`                  | `#EEF1F5` / `#475569` | Đã hủy, ngừng, không khả dụng, hết bảo hành                              |
|                      | `purple`                 | `#F1EBFD` / `#5B2BB8` | Chỉ dùng cho chip vai trò **Admin**                                      |

Ba lưu ý dễ sai:

- **Chữ trên nền cam `#F58220` phải là màu tối `#0F172A`** (chữ trắng không đủ tương phản). Dùng `.btn-accent`, đã đúng sẵn.
- **Không dùng `--n-300` (`#94A3B8`) cho chữ.** Chỉ dùng cho icon mờ.
- **Không chỉ dùng màu để truyền nghĩa:** badge luôn có chữ.

## 4. Trạng thái → badge (bắt buộc thống nhất)

Gọi: `<partial name="_StatusBadge" model="UiHelper.Badge(item.TrangThai)" />`

| Enum                                            | Giá trị → màu                                                                                                                              |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `TrangThaiThietBiEnum`                          | KhaDung **xanh lá** · DangSuaChua **xanh dương** · KhongKhaDung **xám** (cả hàng mờ, ẩn nút "Tạo phiếu sửa chữa")                           |
| `TrangThaiPhieuEnum`                            | ChoXuLy **vàng** · DangXuLy **xanh dương** · HoanThanh **xanh lá** · Huy **xám** (hàng mờ) · TuChoi **đỏ**                                  |
| `MucDoEnum` (thẻ vuông, thanh màu bên trái)     | Thap xám · TrungBinh xanh dương · Cao **cam** · KhanCap **đỏ đặc, chữ trắng**                                                              |
| `VaiTroEnum` (chip)                             | Admin tím · NhanVien cobalt · KhachHang xám                                                                                                |
| `LoaiKhachHangEnum` (chip)                      | CaNhan xám · DoanhNghiep cobalt                                                                                                            |
| `KetQuaDangNhapEnum`                            | ThanhCong xanh lá · SaiMatKhau vàng · BiKhoa đỏ · KhongTonTai xám                                                                           |
| `TrangThaiGuiThongBaoEnum`                      | ChoGui vàng · DaGui xanh lá · Loi đỏ                                                                                                       |
| `KenhThongBaoEnum` / `LoaiDoiTuongDinhKemEnum`  | Không dùng badge màu: chỉ hiện icon + nhãn (`notifications` / `mail`, `devices` / `build`…)                                                |
| Cột `TrangThai` kiểu bit                        | `UiHelper.BadgeHoatDong(x)`: true xanh lá "Hoạt động"; false xám "Ngừng hoạt động" (với tài khoản: `laTaiKhoan: true` → đỏ "Bị khóa")        |
| **Bảo hành** (tính động từ `ThietBi.HanBaoHanh`) | `UiHelper.BaoHanh(hanBaoHanh)` → Còn bảo hành xanh lá · Sắp hết hạn (≤ 30 ngày) vàng, kèm "còn N ngày" · Hết bảo hành xám                  |
| **Hạn dự kiến** (tính động từ `PhieuSuaChua.HanDuKien`) | `UiHelper.HanDuKien(han, trangThai)` → chỉ tính khi phiếu chưa HoanThanh/Huy/TuChoi: Còn hạn xanh lá · Sắp đến hạn (≤ 2 ngày) vàng · Quá hạn đỏ, kèm ghi chú "quá N ngày" |
| **Tồn kho linh kiện** (từ `LinhKien.SoLuongTon`) | `UiHelper.TonKho(soLuong)` → Còn hàng xanh lá · Sắp hết (≤ 5) vàng · Hết hàng đỏ                                                            |

Ghi chú: "Hết bảo hành" dùng màu **xám** (không phải lỗi, chỉ là điều kiện tính phí). Ngưỡng 30 ngày / 2 ngày / 5 cái là mặc định, đổi ở một chỗ duy nhất trong `UiHelper`.

## 5. Chữ, khoảng cách, định dạng

- **Font:** Be Vietnam Pro (chữ), JetBrains Mono (mã, serial, tiền, giờ). Dùng class `.mono` / `.num` (`.num` còn căn phải).
- **Cỡ chữ:** thân 14px; `h1` 24 · `h2` 20 · `h3` 16; nhãn cột bảng 12px IN HOA; số KPI 32px.
- **Khoảng cách:** bội của 4px (4, 8, 12, 16, 20, 24, 32). Bo góc: input/nút 8px, card 12px, modal 16px, badge tròn.
- **Tiền:** `UiHelper.Tien(x)` → `1.250.000 ₫`. **Ngày:** `UiHelper.Ngay(x)` → `07/10/2026`; **ngày giờ:** `UiHelper.NgayGio(x)`. **Mã hiển thị:** `UiHelper.Ma("PSC", id)` → `PSC-0007` (PSC = phiếu sửa chữa, TB = thiết bị, KH = khách hàng, LK = linh kiện).
- **Số và tiền trong bảng:** căn phải, font mono. Tên trong bảng: in đậm, dòng phụ (serial, số điện thoại…) nhỏ và xám bên dưới (`.cell-title` + `.cell-sub`).
- **Serial / CCCD / mã số thuế:** font `.mono`.

## 6. Component → class

| Cần                                                 | Dùng                                                                                                                                                                                                                                         |
| --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Nút chính / phụ                                     | `btn btn-primary` / `btn btn-outline-secondary`                                                                                                                                                                                              |
| Nút nghiệp vụ nổi bật                               | `btn btn-accent` (chỉ **một** nút accent mỗi màn)                                                                                                                                                                                            |
| Nút nhẹ, xóa/khóa                                   | `btn btn-ghost` · `btn btn-danger` · `btn btn-outline-danger`                                                                                                                                                                                |
| Nút chỉ icon                                        | `btn btn-ghost btn-icon` (**bắt buộc** có `title`)                                                                                                                                                                                           |
| Form                                                | `form-label` (thêm `required` nếu bắt buộc) · `form-control` · `form-select` · `form-text`                                                                                                                                                   |
| Trường hệ thống tự tính (Thành tiền, Ngày tiếp nhận, Ngày hoàn thành…) | `form-control is-system` + `readonly` + tooltip "Hệ thống tự tính"                                                                                                                                                  |
| Lỗi validation                                      | `<span asp-validation-for="..."></span>` và `<div asp-validation-summary="All"></div>` — **không tự gắn class**: ASP.NET tự thêm `field-validation-error` / `validation-summary-errors` khi có lỗi, `site.css` đã tạo kiểu cho hai class này |
| Card / KPI                                          | `card` + `card-body` · `card kpi-card` (+ `is-warning` / `is-danger`)                                                                                                                                                                        |
| Bảng                                                | `<div class="table-card"><table class="data-table">…</table><div class="table-footer">…</div></div>` · hàng mờ: `<tr class="is-muted">`                                                                                                      |
| Thanh lọc                                           | `filter-bar` (ô tìm: `search`) · chip lọc: `filter-chips` > `filter-chip`                                                                                                                                                                    |
| Phân trang                                          | Đặt trong `table-footer`: nút Trước / số trang / Sau, trang hiện tại có `active` (Bootstrap `pagination`). Link luôn mang theo toàn bộ điều kiện tìm/lọc/sắp xếp                                                                              |
| Badge                                               | partial `_StatusBadge` (đừng tự viết `<span class="status-badge …">` nếu đã có enum)                                                                                                                                                         |
| Tab                                                 | `nav nav-tabs` (Bootstrap)                                                                                                                                                                                                                   |
| Stepper vòng đời phiếu                              | `<ol class="stepper"><li class="step is-done">…</li><li class="step is-current">…</li><li class="step">…</li></ol>` (Chờ xử lý → Đang xử lý → Hoàn thành; phiếu Hủy/Từ chối hiện badge thay stepper)                                          |
| Timeline lịch sử                                    | `ul.timeline` > `li.timeline-item` + `is-created` / `is-processing` / `is-done` / `is-cancelled` (dữ liệu từ `LichSuTrangThaiPhieu`)                                                                                                         |
| Avatar                                              | `avatar` (`avatar-lg`, `is-accent`, `is-success`)                                                                                                                                                                                            |
| Trạng thái rỗng                                     | `empty-state`                                                                                                                                                                                                                                |
| Khung trang                                         | `app-shell` > `sidebar` + (`app-header` + `app-content`); đầu trang: `page-header` + `page-actions`                                                                                                                                          |
| Đăng nhập / lỗi                                     | `auth-split` > `auth-hero` + `auth-form`                                                                                                                                                                                                     |

## 7. Quy tắc nội dung & UX

- **100% tiếng Việt có dấu** (nhãn, nút, placeholder, thông báo). Nút là **động từ** rõ nghĩa: "Lưu thiết bị", "Tạo phiếu sửa chữa", "Tiếp nhận phiếu" — không dùng "OK/Submit".
- Form: **[Hủy] bên trái, [Lưu] bên phải**.
- **Không hiện nút mà người dùng không có quyền** (ẩn hẳn bằng `User.IsInRole(...)`, không chỉ disable). Menu sidebar thay đổi theo vai trò. Ẩn nút chỉ để gọn giao diện; **quyền vẫn phải kiểm tra tại Controller** (mục 5.4 của đề).
- **Nút phải phù hợp với trạng thái** (mục 14 của đề): "Hủy phiếu" chỉ hiện khi phiếu `ChoXuLy`; "Bắt đầu xử lý" chỉ khi `ChoXuLy`; "Hoàn thành" chỉ khi `DangXuLy`; phiếu `HoanThanh/Huy/TuChoi` không còn nút chuyển trạng thái.
- **Dropdown trạng thái không liệt kê trạng thái trái luồng:** chỉ hiện các bước tiếp theo hợp lệ.
- **Không xóa vật lý dữ liệu có lịch sử:** nút chính là "Ngừng sử dụng / Hủy / Khóa". Xóa luôn qua modal xác nhận. Loại thiết bị/linh kiện/thiết bị đã có phiếu → hiện thông báo lỗi `TempData["Loi"]` thay vì xóa.
- **Khóa ngoại dùng `<select>` lấy từ DB** và hiển thị **tên** (Tên loại, Tên khách hàng, Tên thiết bị) thay cho mã.
- **Kiểu input:** ngày `type="date"`, giờ `type="datetime-local"`, mô tả/ghi chú/nội dung lỗi `<textarea>`, tiền/số lượng `type="number"` có `min`.
- **Khách hàng chỉ thấy dữ liệu của mình:** danh sách và chi tiết lọc theo tài khoản đăng nhập; đổi mã trên URL không được lộ dữ liệu người khác (trả 403/404).
- Giữ nguyên bộ lọc/tìm kiếm/sắp xếp khi phân trang và khi quay lại từ trang chi tiết.
- Mỗi danh sách phải có đủ: **trạng thái rỗng**, **không có kết quả lọc**, và **thông báo thành công/lỗi** (toast hoặc alert).
- Hàng bị ngừng/hủy/khóa: `is-muted`, ẩn nút Sửa.
- **Cảnh báo khả năng đáp ứng** (mục 8.5 của đề): khi linh kiện không đủ tồn hoặc thiết bị không khả dụng, hiện alert vàng/đỏ ở form xử lý, không xóa dữ liệu lịch sử.
- **Ghi tên người thực hiện ở đầu mỗi file View** (mục 18 của đề), dùng Razor comment:
  ```cshtml
  @*
     Họ và tên: Nguyễn Văn A
     Mã sinh viên: 22103100001
     Nội dung thực hiện: Giao diện danh sách thiết bị, tìm kiếm, lọc, sắp xếp, phân trang.
  *@
  ```

## 8. Phạm vi và lý do chọn một số giá trị

| Giá trị chọn                         | Lý do                                                                             |
| ------------------------------------ | --------------------------------------------------------------------------------- |
| Placeholder `#64748B` (`--n-500`)    | `#94A3B8` chỉ đạt 2,56:1; chuẩn AA cần ≥ 4,5:1                                    |
| Tiêu đề cột bảng `#5B6B80` (`--n-600`) | `#64748B` trên nền `#F1F5F9` chỉ 4,34:1 (hụt AA); `#5B6B80` đạt 4,97:1           |
| Icon **Material Symbols Outlined**   | Thống nhất một bộ icon, đã nạp sẵn trong `_Layout`: `<span class="material-symbols-outlined">build</span>` |
| Class `.status-badge.is-success`, Bootstrap `.btn-*` | Tránh trùng/đụng tên với Bootstrap 5                              |

**Phạm vi:** Đề có phần **chức năng nâng cao, không bắt buộc** (mục 19). Xem là **tùy chọn, làm sau cùng nếu nghiệp vụ bắt buộc đã xong**: Dark mode, biểu đồ Chart.js, tìm kiếm AJAX không tải lại trang, xuất Excel/PDF, upload ảnh đính kèm (bảng `TepDinhKem`), thông báo email (bảng `ThongBao`). Không cộng ưu tiên cho giao diện đẹp nếu nghiệp vụ bắt buộc chưa hoàn thành.

## 9. Mẫu prompt khi nhờ AI viết View

Dán nguyên khối dưới đây, rồi thêm yêu cầu cụ thể:

```
Viết file Razor .cshtml cho project ASP.NET Core 10 MVC + Bootstrap 5.3.3 + jQuery.
Tuân thủ nghiêm docs/GIAO_DIEN.md (tôi dán kèm bên dưới) và wwwroot/css/site.css.

Bắt buộc:
- Không hard-code màu/cỡ chữ/bo góc, không style="" và không <style> trong View. Chỉ dùng class có trong site.css hoặc Bootstrap 5.3.
- Badge trạng thái: <partial name="_StatusBadge" model="UiHelper.Badge(...)" />. Bảo hành: UiHelper.BaoHanh(...). Hạn dự kiến: UiHelper.HanDuKien(...). Tồn kho: UiHelper.TonKho(...).
- Tiền/ngày: UiHelper.Tien / UiHelper.Ngay / UiHelper.NgayGio. Mã hiển thị: UiHelper.Ma("PSC", id).
- 100% tiếng Việt có dấu. Nút là động từ. Form: [Hủy] trái, [Lưu] phải.
- Bảng dùng <div class="table-card"><table class="data-table">; số/tiền class="num"; tên dùng .cell-title + .cell-sub.
- Form dùng asp-for, <span asp-validation-for="..."></span> và <div asp-validation-summary="All"></div> (không tự gắn class, ASP.NET tự thêm khi có lỗi).
- Khóa ngoại dùng <select> lấy từ ViewBag/ViewModel, hiển thị tên thay cho mã. Ngày type="date", ngày giờ type="datetime-local", ghi chú dùng textarea.
- Trường hệ thống tự tính: class "form-control is-system" + readonly.
- Ẩn nút theo quyền bằng User.IsInRole(...) và theo trạng thái phiếu/thiết bị.
- Icon: Material Symbols Outlined (<span class="material-symbols-outlined">ten_icon</span>). Thanh tiến độ: bar-row / bar-track / bar-fill. Thông báo sau khi lưu: TempData["ThanhCong"] / TempData["Loi"] rồi RedirectToAction (không tự viết alert).
- Đầu file ghi Razor comment: Họ và tên / Mã sinh viên / Nội dung thực hiện.
- Namespace/model: QuanLy_17_UNETI[STTNhóm]_[MãLớp].Models. Enum trong .Models.Enums. Không đổi Entity/DbContext.

Yêu cầu cụ thể: <mô tả màn hình, model/ViewModel, các cột, bộ lọc...>
```

## 10. Khi giao diện cần thay đổi

1. Báo nhóm trước khi đổi token màu / thêm component dùng chung.
2. Một người sửa `site.css` (và `UiHelper.cs` nếu liên quan enum) trong một Pull Request; ghi rõ thay đổi.
3. Cập nhật mục tương ứng trong file này cùng PR.
4. Commit theo mẫu của đề: `[Mã SV] [Module] Nội dung công việc`, ví dụ `[22103100001] [GiaoDien] Them badge bao hanh`.

## 11. Khung trang và menu theo vai trò

- Mọi trang sau đăng nhập dùng `_Layout` (mặc định qua `_ViewStart`). Đăng nhập và trang lỗi: `@{ Layout = "_AuthLayout"; }`.
- Tiêu đề trang: `@{ ViewData["Title"] = "Danh sách thiết bị"; }` → hiện ở tab trình duyệt và breadcrumb.
- Menu ở `_Sidebar.cshtml` liệt kê sẵn theo 3 vai trò. Tên controller/action là **dự kiến**; mỗi người đổi cho khớp controller của module mình. Thêm mục mới: một dòng `Muc("Controller", "Action", "ten_icon", "Nhãn");`.

| Vai trò                | Mục menu (Controller/Action → icon → nhãn)                                                                                                                                                                                                                  |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Admin**              | `ThongKe/Index` → `dashboard` → Tổng quan · `TaiKhoan/Index` → `manage_accounts` → Tài khoản · `LoaiThietBi/Index` → `category` → Loại thiết bị · `LinhKien/Index` → `memory` → Linh kiện · `ThietBi/Index` → `devices` → Thiết bị · `KhachHang/Index` → `group` → Khách hàng · `PhieuSuaChua/Index` → `build` → Phiếu sửa chữa · `ThongKe/ThongKe` → `bar_chart` → Thống kê |
| **Nhân viên kỹ thuật** | `ThongKe/Index` → `dashboard` → Tổng quan · `PhieuSuaChua/Index` → `build` → Phiếu sửa chữa · `ThietBi/Index` → `devices` → Thiết bị · `KhachHang/Index` → `group` → Khách hàng · `LinhKien/Index` → `memory` → Linh kiện · `ThongKe/ThongKe` → `bar_chart` → Thống kê                                    |
| **Khách hàng**         | `Home/Index` → `home` → Trang chủ · `ThietBi/Index` → `devices` → Thiết bị của tôi · `PhieuSuaChua/Create` → `add_circle` → Tạo phiếu sửa chữa · `PhieuSuaChua/CuaToi` → `receipt_long` → Phiếu của tôi · `KhachHang/HoSo` → `person` → Hồ sơ cá nhân                                                    |

- Thông báo sau khi lưu: controller gán `TempData["ThanhCong"] = "Đã lưu thiết bị thành công."` hoặc `TempData["Loi"] = "Không thể xóa: loại thiết bị đang có thiết bị sử dụng."` rồi `RedirectToAction`. `_Layout` tự hiển thị alert xanh/đỏ, không cần viết lại ở từng View.
- Đầu mỗi trang nội dung:
  ```html
  <div class="page-header">
    <div>
      <h1>Danh sách thiết bị</h1>
      <p>Mô tả ngắn…</p>
    </div>
    <div class="page-actions">
      <a class="btn btn-primary" asp-action="Create">Thêm thiết bị</a>
    </div>
  </div>
  ```

## 12. Icon, biểu đồ, thanh tiến độ

- **Icon:** Material Symbols Outlined, 20px mặc định (`icon-16`, `icon-24` để đổi cỡ). Nút chỉ icon bắt buộc có `title`.
- **KPI Dashboard** (mục 9.3 của đề), mỗi KPI là một `card kpi-card`:

  | KPI                          | Biến thể                  |
  | ---------------------------- | ------------------------- |
  | Tổng số loại thiết bị        | mặc định                  |
  | Tổng số thiết bị             | mặc định                  |
  | Số thiết bị đang khả dụng    | mặc định                  |
  | Tổng số khách hàng           | mặc định                  |
  | Tổng số phiếu sửa chữa       | mặc định                  |
  | Phiếu chờ xử lý              | `is-warning`              |
  | Phiếu đang xử lý             | mặc định                  |
  | Phiếu hoàn thành             | mặc định                  |
  | Tổng chi phí / doanh thu     | mặc định, dùng `.num`     |
  | Phiếu quá hạn dự kiến        | `is-danger` (nếu > 0)     |

- **Thanh tiến độ ngang** (biểu đồ cột ngang: số phiếu theo trạng thái, số thiết bị theo loại) dùng CSS thuần: `.bar-row` > `.bar-head` (nhãn + số) + `.bar-track` > `.bar-fill`. Chiều rộng là giá trị động nên **được phép** dùng `style="width:@pct%"` (đây là ngoại lệ duy nhất cho quy tắc "không style inline").
- **Màu cột theo trạng thái** (phải khớp với badge, mục 4):

  | Nhóm trạng thái          | Class `.bar-fill` |
  | ------------------------ | ----------------- |
  | ChoXuLy                  | `is-warning`      |
  | DangXuLy / DangSuaChua   | `is-info`         |
  | HoanThanh / KhaDung      | `is-success`      |
  | Huy / KhongKhaDung       | `is-muted`        |
  | TuChoi                   | `is-danger`       |

- **Donut / đường (không bắt buộc):** đề xuất **Chart.js 4** (CDN jsdelivr); nhóm xác nhận trước khi dùng. Màu series theo thứ tự: `#1F5FBF`, `#F58220`, `#16A34A`, `#7C3AED`, `#0EA5A4`, `#DC2626`, `#94A3B8` (tối đa 6 màu/biểu đồ). Donut trạng thái phiếu dùng màu badge: vàng / xanh dương / xanh lá / xám / đỏ. Biểu đồ số phiếu theo tháng dùng đường hoặc cột màu cobalt. Trục tiền ghi `12,2 tr`, không ghi `12.2M`.

## 13. Port từ mockup HTML (Tailwind) sang Razor

Nếu dùng công cụ AI (Stitch…) sinh mockup bằng **Tailwind CDN**, nhớ project dùng **Bootstrap + site.css**: lấy bố cục làm tham khảo, **không copy nguyên HTML**. Quy đổi:

| Mockup (Tailwind)                                                | Project                                                               |
| ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| `bg-neutral-0 rounded-xl p-5 shadow-sm`                          | `card` + `card-body` (KPI: `card kpi-card`)                           |
| Khối `<span class="inline-flex … rounded-full bg-success-bg …">` | partial `_StatusBadge` + `UiHelper.Badge(...)`                        |
| `w-8 h-8 rounded-full bg-primary-100 …` + chữ viết tắt           | `<span class="avatar">@UiHelper.VietTat(ten)</span>`                  |
| `<table class="w-full …">` với `thead` tự tô                     | `table-card` > `data-table`                                           |
| `flex items-center gap-2` / `grid grid-cols-3 gap-6`             | Bootstrap: `d-flex align-items-center gap-2` / `row g-4` + `col-lg-4` |
| `font-mono-code`                                                 | `mono` hoặc `num`                                                     |
| Thanh `<div class="bg-… h-2 rounded-full" style="width:…">`      | `bar-row` / `bar-track` / `bar-fill is-…`                             |

**Những thứ trong mockup AI hay sinh ra nhưng KHÔNG có trong CSDL/đề, đừng làm theo:**

| #   | Thường gặp trong mockup                                          | Xử lý                                                                                                                          |
| --- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| 1   | Logo lặp ở cả sidebar và header                                  | Logo chỉ ở sidebar; breadcrumb theo trang (đã làm trong `_Layout`)                                                             |
| 2   | Chữ 10–11px                                                      | Tối thiểu 12px                                                                                                                 |
| 3   | Màu cam cho trạng thái "Đang xử lý"                              | Phải là xanh dương; cam chỉ dành cho mức độ Cao và hành động nghiệp vụ                                                         |
| 4   | Số liệu cố định, tự bịa, tổng không khớp                         | Số liệu thật lấy từ truy vấn LINQ, tính phần trăm trên đúng tổng                                                               |
| 5   | "vi phạm SLA", "ngân sách", "đồng bộ mỗi 5 phút", "Trực tuyến"   | **Bỏ**: CSDL không có các khái niệm này                                                                                        |
| 6   | Khối "Hoạt động gần đây" với nhiều vai trò lạ                    | Hệ thống chỉ có 3 vai trò. Nếu làm, lấy từ bảng `LichSuTrangThaiPhieu` (đã có trong ERD) hoặc bỏ khối này                      |
| 7   | Biểu đồ đường SVG vẽ tay, số cố định                             | Dùng Chart.js (mục 12) hoặc `bar-row`, dữ liệu từ truy vấn                                                                     |
| 8   | Nhãn nhỏ in hoa trên mỗi KPI                                     | Bỏ cho gọn; chỉ giữ nhãn + số + một dòng ghi chú tính được từ dữ liệu                                                          |
| 9   | Mục "Chờ nghiệm thu", "Điều phối viên", "SLA", "kế hoạch bảo trì" | Không thuộc đề 17 (đề này chỉ có sửa chữa khi khách tạo phiếu, không có bảo trì định kỳ)                                        |

Dữ liệu mẫu trong mockup khác với dữ liệu seed. Khi viết View thật, **dùng dữ liệu từ DB**; để kiểm tra giao diện, dùng chính dữ liệu seed (30 khách hàng, 40–50 phiếu nhiều trạng thái, mục 16 của đề).

## 14. Màn hình cần có theo module

Lấy từ mục 17 và mục 4 của đề. Mỗi người làm View cho module mình.

| Module (Sinh viên) | Phía Khách hàng                                                                         | Phía Nhân viên / Admin                                                                                          |
| ------------------ | --------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| **1** Tài khoản, đăng nhập, phân quyền, loại thiết bị | Đăng nhập (`_AuthLayout`) · Đăng xuất · 403 "Không có quyền"                            | Danh sách/Thêm/Sửa/Chi tiết tài khoản · Danh sách/Thêm/Sửa/Chi tiết loại thiết bị                               |
| **2** Quản lý thiết bị | Danh sách thiết bị của tôi + Chi tiết (hiện badge khả dụng/bảo hành)                    | Danh sách thiết bị **có Tìm kiếm + Lọc + Sắp xếp + Phân trang** · Thêm/Sửa/Chi tiết/Đổi trạng thái              |
| **3** Khách hàng, hồ sơ, tạo phiếu, theo dõi | Trang chủ · Hồ sơ cá nhân · Tạo phiếu sửa chữa · Danh sách phiếu của tôi · Chi tiết + stepper theo dõi · Hủy phiếu | Danh sách/Thêm/Sửa/Chi tiết khách hàng                                                                          |
| **4** Tiếp nhận, chẩn đoán, sửa chữa, trạng thái, chi phí | Xem kết quả xử lý trong chi tiết phiếu                                                  | Danh sách phiếu cần xử lý (tìm theo khách/thiết bị, lọc trạng thái/khoảng ngày/loại, sắp xếp) · Chi tiết phiếu · Tiếp nhận · Chẩn đoán · Thêm chi tiết sửa chữa (linh kiện, số lượng, đơn giá, thành tiền) · Đổi trạng thái · Từ chối/Hủy có lý do |
| **5** Linh kiện, dashboard, thống kê, báo cáo | —                                                                                       | Danh sách/Thêm/Sửa linh kiện · Dashboard · Thống kê (theo loại, theo trạng thái, theo tháng, kỹ thuật viên nhiều phiếu nhất, thời gian sửa trung bình, chi phí linh kiện, doanh thu, tỷ lệ trong bảo hành) |

## 15. Checklist trước khi tạo Pull Request giao diện

- [ ] Không còn `style="..."` (trừ `width` của `bar-fill`) và không có `<style>` trong View.
- [ ] Mọi trạng thái đi qua `_StatusBadge`; màu khớp bảng mục 4.
- [ ] Có Razor comment Họ tên / Mã SV / Nội dung ở đầu file.
- [ ] Danh sách có trạng thái rỗng, không có kết quả lọc, thông báo thành công/lỗi.
- [ ] Phân trang giữ nguyên điều kiện tìm/lọc/sắp xếp.
- [ ] Ẩn nút theo quyền **và** theo trạng thái; Controller vẫn kiểm tra quyền.
- [ ] Khóa ngoại dùng `<select>`, hiển thị tên thay mã; ngày/giờ đúng `type`.
- [ ] Thử bằng dữ liệu seed ở cả 3 tài khoản (Admin, Nhân viên, Khách hàng).
