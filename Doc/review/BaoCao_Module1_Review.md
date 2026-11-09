# BÁO CÁO REVIEW CODE VÀ THUYẾT MINH MODULE 1
**Đề tài 17:** Quản lý bảo hành và sửa chữa thiết bị (ASP.NET Core 10 MVC)  
**Nhóm:** UNETI04 - Lớp DHTI17A4HN  
**Sinh viên phụ trách Module 1:** Bùi Hồng Vân  
**Mã sinh viên:** 23103100203  
**Phạm vi Module 1:** Tài khoản – Đăng nhập – Đăng xuất – Phân quyền Controller – Loại thiết bị – Nhật ký đăng nhập.

---

## 1. TỔNG QUAN VÀ PHẠM VI TRÁCH NHIỆM (MODULE 1)

Module 1 đóng vai trò là **hạ tầng dữ liệu nền tảng và cổng bảo mật** cho toàn bộ hệ thống quản lý sửa chữa thiết bị. Toàn bộ các module khác (Module 2: Quản lý thiết bị, Module 3: Khách hàng & Phiếu, Module 4: Luồng xử lý kỹ thuật viên, Module 5: Kho linh kiện & Dashboard) đều phụ thuộc trực tiếp vào cơ chế xác thực, phiên làm việc (Session) và phân quyền của Module 1.

### Các bảng CSDL trực tiếp phụ trách:
1. **`TaiKhoan`** (Bảng bắt buộc - mục 10 của đề): Quản lý định danh người dùng, mật khẩu đã mã hóa băm, họ tên, email, vai trò và trạng thái khóa/mở.
2. **`LoaiThietBi`** (Bảng bắt buộc - mục 10 của đề): Dữ liệu nền danh mục thiết bị, quy định hãng sản xuất và thời hạn bảo hành mặc định.
3. **`NhatKyDangNhap`** (Bảng nâng cao - mục 19 của đề): Lưu vết toàn bộ lịch sử đăng nhập, địa chỉ IP, thời gian và kết quả (Thành công, Sai mật khẩu, Tài khoản bị khóa, v.v.).

---

## 2. KIẾN TRÚC VÀ CÁC THÀNH PHẦN CODE CHÍNH

Hệ thống được thiết kế theo mô hình phân lớp rõ ràng (Clean Architecture / N-Tier pattern):
```
Controllers ──> Services (Business Logic) ──> Repositories / UnitOfWork ──> Entity Framework Core ──> SQL Server
     │
     └──> Helpers (PhanQuyenAttribute, DangNhapFilter, PhienDangNhap, MatKhauHelper)
```

### 2.1. Quản lý Phiên làm việc (Session Management)
- **Tập tin:** `Helpers/PhienDangNhap.cs`, cấu hình `Program.cs`.
- **Cơ chế hoạt động:**
  - Không sử dụng Cookie Authentication thuần hay lưu trữ token phức tạp ở client, hệ thống sử dụng `ISession` tích hợp sẵn trong ASP.NET Core với cookie mã hóa HttpOnly (`.QLBaoHanh.Session`), chống giả mạo XSS.
  - Khi người dùng đăng nhập thành công, hệ thống ghi nhận 3 thông tin chính vào Session:
    1. `MaTaiKhoan` (int) -> Key: `DangNhap.MaTaiKhoan`
    2. `HoTen` (string) -> Key: `DangNhap.HoTen`
    3. `VaiTro` (VaiTroEnum: Admin / NhanVien / KhachHang) -> Key: `DangNhap.VaiTro`
  - Khi đăng xuất, hàm `HttpContext.Session.Clear()` dọn dẹp sạch toàn bộ dữ liệu phiên làm việc.

### 2.2. Kiến trúc Bảo mật & Phân quyền Kiểm tra Tại Controller (Task 25 & 26)
- **Tập tin:** `Helpers/PhanQuyenAttribute.cs`, `Program.cs`.
- **Nguyên lý bảo mật đa lớp:**
  1. **Lớp 1 - Filter Toàn cục (`DangNhapFilter` - Global IAsyncAuthorizationFilter):**
     - Đăng ký toàn cục tại `Program.cs`: `options.Filters.Add<DangNhapFilter>()`.
     - Mọi Request vào bất kỳ Controller/Action nào của ứng dụng đều mặc định **bắt buộc phải đăng nhập**.
     - Ngoại lệ: Chỉ những Action có đánh dấu `[AllowAnonymous]` (như trang Giới thiệu công khai, Trang Đăng nhập, Trang 403 Từ chối) mới được bỏ qua.
     - **Tính năng bảo mật nâng cao:** Trong mỗi Request, filter kiểm tra trực tiếp trạng thái tài khoản trong CSDL qua `ITaiKhoanRepository.LayThongTinXacThucAsync()`. Nếu tài khoản vừa bị Admin vô hiệu hóa/khóa hoặc thay đổi vai trò, phiên làm việc sẽ bị hủy ngay lập tức mà không cần đợi Session timeout.
  2. **Lớp 2 - Kiểm tra Vai trò tại Controller/Action (`PhanQuyenAttribute` - IAuthorizationFilter):**
     - Gắn trực tiếp lên Controller hoặc Action, ví dụ:
       - `[PhanQuyen(VaiTroEnum.Admin)]`: Chỉ cho phép Quản trị viên.
       - `[PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]`: Cho phép Admin và Kỹ thuật viên/Nhân viên.
       - `[PhanQuyen]`: Bất kỳ ai đã đăng nhập hợp lệ.
     - Nếu chưa đăng nhập: Tự động điều hướng về `/TaiKhoan/DangNhap?returnUrl=...`.
     - Nếu đăng nhập nhưng không đủ quyền: Tự động chặn và chuyển hướng về `/TaiKhoan/TuChoi` (HTTP 403 Forbidden), ngăn chặn hoàn toàn việc đổi ID trên thanh địa chỉ URL.

### 2.3. Quy trình Đăng nhập, Băm mật khẩu và Khóa phòng chống Brute-Force (STT 12)
- **Tập tin:** `Services/NguoiDung/DangNhapService.cs`, `Helpers/MatKhauHelper.cs`.
- **Băm mật khẩu:** Sử dụng `PasswordHasher<TaiKhoan>` tiêu chuẩn của Microsoft ASP.NET Core Identity (thuật toán PBKDF2 với HMAC-SHA256, muối ngẫu nhiên 128-bit). Tuyệt đối không lưu plain-text dưới CSDL.
- **Phòng chống dò mật khẩu:**
  - Theo dõi số lần đăng nhập sai (`SoLanDangNhapSai`).
  - Nếu sai liên tiếp 5 lần: Khóa tạm tài khoản trong vòng 15 phút (`KhoaDenNgay = Now + 15 phút`).
  - Thông báo rõ số lần thử còn lại hoặc thời gian mở khóa.
  - Tự động ghi nhận thông tin truy vết vào bảng `NhatKyDangNhap` (kết quả, thời gian, IP client).

### 2.4. Quản lý Tài khoản & Kiểm soát Xóa An toàn (STT 11)
- **Tập tin:** `Controllers/TaiKhoanController.cs`, `Services/NguoiDung/TaiKhoanService.cs`.
- **Quy tắc nghiệp vụ:**
  - Không cho phép trùng `TenDangNhap` (bảo vệ cả ở tầng Service lẫn Unique Index CSDL).
  - Không cho phép Admin tự đổi vai trò của chính mình hoặc tự khóa tài khoản của mình.
  - Bảo vệ Admin hoạt động cuối cùng: Không cho phép khóa/xóa/đổi vai trò nếu đó là Quản trị viên duy nhất còn lại trong hệ thống (`LaAdminHoatDongCuoiCungAsync`).
  - **Xóa an toàn:** Kiểm tra liên kết dữ liệu (`CoDuLieuLienQuanAsync`). Nếu tài khoản đã phát sinh hồ sơ khách hàng, kỹ thuật viên hoặc nhật ký, hệ thống từ chối xóa vật lý và hướng dẫn chuyển sang trạng thái "Khóa tài khoản".

### 2.5. Quản lý Loại Thiết bị (STT 13)
- **Tập tin:** `Controllers/LoaiThietBiController.cs`, `Services/ThietBiVaLinhKien/LoaiThietBiService.cs`.
- **Quy tắc nghiệp vụ:**
  - Bắt buộc nhập tên loại, hãng sản xuất và thời hạn bảo hành mặc định.
  - Kiểm tra không trùng tên loại thiết bị (`KiemTraTrungTenLoaiAsync`).
  - **Kiểm soát xóa an toàn:** Chỉ cho phép xóa khi chưa có bất kỳ thiết bị (`ThietBi`) nào thuộc loại này tham chiếu tới (`DemThietBiTheoLoaiAsync(id) == 0`). Nếu đã có dữ liệu, báo lỗi và hướng dẫn chuyển sang "Ngừng sử dụng".

---

## 3. BẢNG TỔNG HỢP CÁC FILE ĐÃ HOÀN THIỆN KÈM COMMENT CHUẨN

Tất cả các file do **Bùi Hồng Vân (MSSV: 23103100203)** phụ trách đều có comment chuẩn ở đầu file theo quy định của đề tài:
```csharp
// Họ và tên: Bùi Hồng Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - ...
```

| STT | File đường dẫn | Thành phần | Trách nhiệm |
| :---: | :--- | :--- | :--- |
| 1 | `Models/TaiKhoan.cs` | Entity | Định nghĩa thực thể Tài khoản, Data Annotations |
| 2 | `Models/LoaiThietBi.cs` | Entity | Định nghĩa thực thể Loại thiết bị, Data Annotations |
| 3 | `Models/NhatKyDangNhap.cs` | Entity | Định nghĩa thực thể Nhật ký đăng nhập |
| 4 | `Helpers/PhanQuyenAttribute.cs` | Filter/Helper | Bộ lọc phân quyền Controller và kiểm tra phiên đăng nhập |
| 5 | `Helpers/PhienDangNhap.cs` | Helper | Các hàm mở rộng đọc/ghi thông tin Session người dùng |
| 6 | `Helpers/MatKhauHelper.cs` | Helper | Thuật toán băm và kiểm tra mật khẩu an toàn |
| 7 | `Controllers/TaiKhoanController.cs` | Controller | Xử lý Đăng nhập, Đăng xuất, Trang 403, CRUD Tài khoản |
| 8 | `Controllers/LoaiThietBiController.cs` | Controller | CRUD Loại thiết bị, chặn xóa khi có thiết bị |
| 9 | `Controllers/NhatKyDangNhapController.cs`| Controller | Tra cứu lịch sử đăng nhập dành cho Admin |
| 10 | `Repositories/TaiKhoanRepository.cs` | Repository | Thao tác LINQ/EF Core cho thực thể TaiKhoan |
| 11 | `Repositories/LoaiThietBiRepository.cs`| Repository | Thao tác LINQ/EF Core cho thực thể LoaiThietBi |
| 12 | `Repositories/NhatKyDangNhapRepository.cs` | Repository | Thao tác LINQ/EF Core cho thực thể NhatKyDangNhap |
| 13 | `Services/NguoiDung/DangNhapService.cs` | Service | Nghiệp vụ xác thực đăng nhập, khóa tạm, băm pass |
| 14 | `Services/NguoiDung/TaiKhoanService.cs` | Service | Nghiệp vụ CRUD tài khoản, bảo vệ Admin cuối cùng |
| 15 | `Services/ThietBiVaLinhKien/LoaiThietBiService.cs`| Service | Nghiệp vụ kiểm tra trùng tên loại, kiểm soát xóa |
| 16 | `Services/HeThong/NhatKyDangNhapService.cs` | Service | Nghiệp vụ tra cứu nhật ký hệ thống |
| 17 | `ViewModels/TaiKhoanViewModels.cs` | ViewModel | Data binding & validation cho form Đăng nhập / Tài khoản |
| 18 | `ViewModels/LoaiThietBiViewModels.cs` | ViewModel | Data binding & validation cho form Loại thiết bị |
| 19 | `Views/TaiKhoan/*` | Views (Razor) | 7 views: Index, Create, Edit, Details, DangNhap, TuChoi, _Form |
| 20 | `Views/LoaiThietBi/*` | Views (Razor) | 5 views: Index, Create, Edit, Details, _Form |
| 21 | `Views/NhatKyDangNhap/Index.cshtml` | View (Razor) | Giao diện danh sách nhật ký đăng nhập |

---

## 4. KẾT QUẢ ĐỐI SOÁT VÀ SẴN SÀNG BẢO VỆ

- **Đăng nhập & Phiên làm việc:** Kiểm tra thử nghiệm đăng nhập đúng/sai mật khẩu, hiển thị thông tin chào mừng, phân nhóm menu Sidebar chính xác theo vai trò.
- **Chặn truy cập URL trái phép:** Đã kiểm thử thành công bằng cách đăng nhập tài khoản Khách hàng hoặc Nhân viên và thử gõ tay URL Quản trị (`/TaiKhoan`, `/LoaiThietBi`, `/Dashboard`, `/KyThuatVien`) -> Hệ thống lập tức kích hoạt bộ lọc và trả về trang từ chối quyền `TaiKhoan/TuChoi` (HTTP 403).
- **Tính toàn vẹn mã nguồn:** Toàn bộ project biên dịch không có lỗi (`0 Error`), sẵn sàng phục vụ báo cáo bảo vệ bài tập lớn.
