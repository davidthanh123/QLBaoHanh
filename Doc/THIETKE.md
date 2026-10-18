# THIẾT KẾ CƠ SỞ DỮ LIỆU

Hệ thống Quản lý Bảo hành và Sửa chữa Thiết bị (RepairPro) — Đề tài 17, Nhóm UNETI04, lớp DHTI17A4HN

**Phiên bản: 1.0 — 11/10/2026.** Bộ tài liệu gồm 4 file đồng bộ: [schema.dbml](./schema.dbml) (import vào dbdiagram.io), [ERD.md](./ERD.md) (sơ đồ Mermaid), [RepairPro.sql](./RepairPro.sql) (script SQL Server tham chiếu) và file này. [GIAODIEN.md](./GIAODIEN.md) là quy ước giao diện, dùng chung tên enum với mục 2.3.

> **Quy tắc đồng bộ:** đổi Entity/cột/enum ở một nơi thì phải đổi cả 4 file trên **và** Model trong cùng một Pull Request. Hiện tại 3 file ERD / dbml / SQL đã khớp nhau từng cột (tên, thứ tự, NULL/NOT NULL, giá trị enum). Phần Model xem mục 5.
>
> Các mục ghi **(đề xuất)** là quy ước nghiệp vụ đề tài không nói rõ; cả nhóm duyệt rồi mới coi là chốt.

## 1. Danh sách bảng theo Module

12 bảng chia 3 nhóm: 7 bảng bắt buộc theo mục 10 của đề, 2 bảng bổ sung phục vụ nghiệp vụ, 3 bảng nâng cao (mục 19, không làm vẫn đủ 5 Module).

| Module | Người phụ trách | Bảng bắt buộc (đề mục 10)                | Bảng bổ sung / nâng cao                         |
| ------ | --------------- | ---------------------------------------- | ----------------------------------------------- |
| 1      | Vân             | `TaiKhoan`, `LoaiThietBi`                | `NhatKyDangNhap` (nâng cao)                     |
| 2      | Hoàng Thành     | `ThietBi`                                | `TepDinhKem` (nâng cao, dùng chung)             |
| 3      | David           | `KhachHang`, `PhieuSuaChua`              | —                                               |
| 4      | Thảo            | — (xử lý trạng thái trên `PhieuSuaChua`) | `KyThuatVien`, `LichSuTrangThaiPhieu` (bổ sung) |
| 5      | Vũ              | `LinhKien`, `ChiTietSuaChua`             | `ThongBao` (nâng cao)                           |

Ghi chú:

- `PhieuSuaChua`: David là chủ file Entity. Thảo cần thêm cột thì báo David (Module 4 chủ yếu viết Controller/logic chuyển trạng thái).
- `ChiTietSuaChua`: file Entity thuộc Module 5, nhưng form thêm chi tiết nằm trong luồng sửa chữa của Module 4.
- `KyThuatVien` và `LichSuTrangThaiPhieu` không nằm trong 7 Entity tối thiểu của đề, nhưng cần thiết: `ChiTietSuaChua.MaKyThuatVien` phải trỏ tới một kỹ thuật viên, và luồng trạng thái (đề mục 8.4) cần lưu vết mỗi lần đổi.
- `TepDinhKem` dùng chung cho `ThietBi` (Module 2) và `PhieuSuaChua`/`ChiTietSuaChua` (Module 3, 4).

## 2. Giải thích các quyết định thiết kế quan trọng

### 2.1. Các giá trị KHÔNG lưu cột riêng (tính động bằng LINQ)

- **Còn/hết bảo hành**: `ThietBi.HanBaoHanh >= hôm nay` (với thiết bị) hoặc `>= NgayTiepNhan` (với phiếu). Không có cột "ConBaoHanh".
- **Tổng chi phí của phiếu** = `SUM(ChiTietSuaChua.ThanhTien)` theo `MaPhieu`. `PhieuSuaChua` **không** có cột `TongChiPhi`.
- **Thời gian sửa chữa trung bình** = trung bình `NgayHoanThanh − NgayTiepNhan` của các phiếu `HoanThanh`.
- **Tỷ lệ sửa chữa trong bảo hành** = số phiếu có `NgayTiepNhan <= ThietBi.HanBaoHanh` ÷ tổng số phiếu.
- **Chi phí linh kiện** = `SUM(ThanhTien)` các dòng có `MaLinhKien != null`. **Doanh thu sửa chữa** = `SUM(ThanhTien)` của phiếu `HoanThanh`. (đề xuất)
- **Hạn dự kiến / quá hạn**: tính từ `HanDuKien` khi hiển thị (ngưỡng ở GIAODIEN.md mục 4).

### 2.2. `ThietBi.HanBaoHanh` lưu thành cột

Đề yêu cầu lọc theo "trạng thái bảo hành" (mục 6.4) và xác định còn/hết hạn từ ngày mua + hạn bảo hành (mục 8.3), nên `NgayMua` và `HanBaoHanh` đều `NOT NULL`. Khi thêm thiết bị, form gợi ý sẵn `HanBaoHanh = NgayMua + LoaiThietBi.ThoiHanBaoHanhMacDinh (tháng)`, vẫn cho sửa tay. `GiaTri` `NOT NULL`, mặc định 0.

### 2.3. Enum lưu dưới DB dạng chuỗi

Mọi cột trạng thái/vai trò/mức độ là `enum` C# trong `Models/Enums/Enums.cs` (namespace `QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums`), lưu DB dạng `nvarchar` + CHECK, cấu hình EF Core bằng `.HasConversion<string>()`. Giá trị là ASCII không dấu; nhãn tiếng Việt chỉ xuất hiện khi hiển thị (qua `UiHelper`, xem GIAODIEN.md mục 4). Không dùng chuỗi tự do trong code (ví dụ không viết `"Chờ xử lý"`).

| Enum                       | Giá trị                                    | Cột dùng                                                         |
| -------------------------- | ------------------------------------------ | ---------------------------------------------------------------- |
| `VaiTroEnum`               | Admin, NhanVien, KhachHang                 | `TaiKhoan.VaiTro`                                                |
| `LoaiKhachHangEnum`        | CaNhan, DoanhNghiep                        | `KhachHang.LoaiKhachHang`                                        |
| `TrangThaiThietBiEnum`     | KhaDung, DangSuaChua, KhongKhaDung         | `ThietBi.TrangThai`                                              |
| `TrangThaiPhieuEnum`       | ChoXuLy, DangXuLy, HoanThanh, Huy, TuChoi  | `PhieuSuaChua.TrangThai`, `LichSuTrangThaiPhieu.TrangThaiCu/Moi` |
| `MucDoEnum`                | Thap, TrungBinh, Cao, KhanCap              | `PhieuSuaChua.MucDo`                                             |
| `KetQuaDangNhapEnum`       | ThanhCong, SaiMatKhau, KhongTonTai, BiKhoa | `NhatKyDangNhap.KetQua`                                          |
| `LoaiDoiTuongDinhKemEnum`  | ThietBi, PhieuSuaChua, ChiTietSuaChua      | `TepDinhKem.LoaiDoiTuong`                                        |
| `KenhThongBaoEnum`         | TrongHeThong, Email                        | `ThongBao.Kenh`                                                  |
| `TrangThaiGuiThongBaoEnum` | ChoGui, DaGui, Loi                         | `ThongBao.TrangThai`                                             |

Các cột `TrangThai` kiểu `bit` (TaiKhoan, LoaiThietBi, KhachHang, KyThuatVien, LinhKien): `true` = hoạt động, `false` = khóa/ngừng.

### 2.4. Luồng trạng thái của phiếu sửa chữa (đề xuất)

Chỉ cho chuyển theo bảng dưới; mọi chuyển trạng thái đều ghi 1 dòng vào `LichSuTrangThaiPhieu`. Kiểm tra ở Controller, không chỉ ẩn nút trên View.

| Từ         | Sang        | Ai được làm                      | Điều kiện / việc hệ thống tự làm                                                      |
| ---------- | ----------- | -------------------------------- | ------------------------------------------------------------------------------------- |
| (tạo mới)  | `ChoXuLy`   | Khách hàng / NV                  | Kiểm tra mục 2.5; `NgayTiepNhan` do hệ thống gán                                      |
| `ChoXuLy`  | `DangXuLy`  | Nhân viên                        | Gán `MaTaiKhoanTiepNhan`; thiết bị chuyển `DangSuaChua`                               |
| `ChoXuLy`  | `Huy`       | Khách hàng (phiếu của mình) / NV | Bắt buộc `LyDoHuyTuChoi`                                                              |
| `ChoXuLy`  | `TuChoi`    | Nhân viên                        | Bắt buộc `LyDoHuyTuChoi` (ví dụ không thuộc phạm vi bảo hành)                         |
| `DangXuLy` | `HoanThanh` | Nhân viên                        | Đã có ít nhất 1 `ChiTietSuaChua`; hệ thống gán `NgayHoanThanh`; thiết bị về `KhaDung` |
| `DangXuLy` | `Huy`       | Nhân viên / Admin                | Bắt buộc `LyDoHuyTuChoi`; thiết bị về `KhaDung`                                       |

`HoanThanh`, `Huy`, `TuChoi` là trạng thái cuối: không đổi tiếp, không xóa phiếu (đề mục 7.4).

### 2.5. Điều kiện tạo phiếu và chống tạo trùng

Khi tạo `PhieuSuaChua` (đề mục 7.3, 8.3), Controller dùng LINQ kiểm tra theo thứ tự: tài khoản/khách hàng tồn tại và `TrangThai = true`; thiết bị tồn tại và **thuộc đúng khách hàng đang tạo phiếu**; thiết bị đang `KhaDung`; **không có phiếu `ChoXuLy`/`DangXuLy` nào khác** của cùng thiết bị (chống trùng); `NoiDungLoi` đã nhập; `HanDuKien >= ngày tiếp nhận` nếu có. Trạng thái `KhongKhaDung` do nhân viên/Admin đặt khi ngừng nhận sửa thiết bị đó; View ẩn nút "Tạo phiếu sửa chữa" với trạng thái này.

### 2.6. `KyThuatVien` — quan hệ 1–1 với `TaiKhoan`

`KyThuatVien.MaTaiKhoan` là FK `NOT NULL` + `UNIQUE`, trỏ tới tài khoản có `VaiTro = NhanVien`. Tách bảng riêng vì kỹ thuật viên có thuộc tính nghiệp vụ riêng (`ChuyenMon`, số điện thoại) mà `TaiKhoan` dùng chung 3 vai trò không cần. Module 4 viết màn hình quản lý kỹ thuật viên: Admin chọn một tài khoản `NhanVien` chưa có hồ sơ rồi bổ sung thông tin. `ChiTietSuaChua.MaKyThuatVien` trỏ tới `KyThuatVien.MaKyThuatVien` (không trỏ thẳng `TaiKhoan`); thống kê "kỹ thuật viên xử lý nhiều phiếu nhất" (đề mục 9.4) đếm theo bảng này.

`KhachHang.MaTaiKhoan` là FK **nullable** (khách do nhân viên nhập tay có thể chưa có tài khoản), `UNIQUE` có điều kiện `WHERE MaTaiKhoan IS NOT NULL`. Tương tự `KhachHang.CCCD`. EF Core `HasIndex().IsUnique()` tự thêm filter này cho cột nullable.

### 2.7. Xóa mềm thay vì xóa cứng; mọi FK là Restrict

Đề yêu cầu không xóa đối tượng đã phát sinh lịch sử (mục 6.2, 8.5). Vì vậy:

- Mọi FK cấu hình `DeleteBehavior.Restrict` (trong SQL là `NO ACTION`). Code hiện đã làm bằng vòng lặp ở `AppDbContext.OnModelCreating`; cách này cũng tránh lỗi "multiple cascade paths" của SQL Server (`KhachHang → PhieuSuaChua` đi hai đường).
- Ngừng sử dụng thì đổi `TrangThai`, không `Remove()`. Xóa chỉ được phép khi không còn bản ghi tham chiếu (kiểm tra bằng `Any()`), ví dụ xóa `LoaiThietBi` còn thiết bị thì báo lỗi (đề mục 5.5).
- `LichSuTrangThaiPhieu` và `NhatKyDangNhap` là nhật ký: **không có chức năng sửa/xóa**.

### 2.8. `ChiTietSuaChua.ThanhTien`

Lưu thành cột thường (không phải computed column) để không phải tính lại khi đọc chi tiết cũ. Người dùng **không được nhập**: Controller gán `ThanhTien = SoLuong * DonGia` trước `SaveChanges`. DB có thêm `CK_CT_ThanhTien` để chặn dữ liệu sai nếu ai đó ghi trực tiếp.

### 2.9. Mật khẩu

Dùng `PasswordHasher<TaiKhoan>` của ASP.NET Core (không cài cả ASP.NET Core Identity vì nó thay `TaiKhoan` bằng `AspNetUsers` khóa kiểu string, lệch Entity mà đề yêu cầu). Salt nằm chung trong chuỗi `MatKhau`, nên `MatKhauSalt` để `NULL`; cột chỉ dùng nếu nhóm tự viết hàm băm. Khóa tạm sau N lần sai: `SoLanDangNhapSai` + `KhoaDenNgay`; khóa vĩnh viễn do Admin: `TrangThai = false`.

## 3. Bảng ràng buộc bắt buộc (map vào Data Annotation + kiểm tra ở Controller)

| Bảng           | Ràng buộc                                                                     | Nơi kiểm tra                                                  |
| -------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------- |
| TaiKhoan       | `TenDangNhap` bắt buộc, không trùng                                           | `[Required]` + unique index + LINQ báo lỗi thân thiện         |
| TaiKhoan       | `Email` đúng định dạng                                                        | `[EmailAddress]` + CHECK                                      |
| TaiKhoan       | Tài khoản `TrangThai = false` hoặc còn trong `KhoaDenNgay` không đăng nhập    | Controller đăng nhập                                          |
| LoaiThietBi    | `TenLoai` không trùng; `HangSanXuat` bắt buộc                                 | `[Required]` + unique index + LINQ                            |
| LoaiThietBi    | `ThoiHanBaoHanhMacDinh` từ 0 đến 120 (tháng)                                  | `[Range(0, 120)]` + CHECK                                     |
| LoaiThietBi    | Không xóa khi đã có thiết bị tham chiếu                                       | LINQ `Any()` + FK Restrict                                    |
| ThietBi        | `SerialNumber` không trùng                                                    | `[Required]` + unique index + LINQ                            |
| ThietBi        | `HanBaoHanh >= NgayMua`                                                       | `IValidatableObject` hoặc Controller + CHECK                  |
| ThietBi        | `GiaTri >= 0`                                                                 | `[Range]` + CHECK                                             |
| KhachHang      | `CCCD` không trùng (nếu có); `SoDienThoai` chỉ gồm số/dấu `+`                 | unique index có điều kiện; `[Phone]` + CHECK                  |
| KhachHang      | `MaSoThue` chỉ dùng khi `LoaiKhachHang = DoanhNghiep`                         | Controller + CHECK                                            |
| KhachHang      | Người dùng chỉ sửa hồ sơ của chính mình                                       | Controller so `MaTaiKhoan` với Session                        |
| PhieuSuaChua   | Điều kiện tạo phiếu và chống trùng                                            | LINQ ở Controller (mục 2.5)                                   |
| PhieuSuaChua   | `Huy`/`TuChoi` bắt buộc `LyDoHuyTuChoi`; `HoanThanh` bắt buộc `NgayHoanThanh` | CHECK + Controller (hệ thống gán `NgayHoanThanh`)             |
| PhieuSuaChua   | `HanDuKien >= ngày tiếp nhận`                                                 | CHECK + Controller                                            |
| PhieuSuaChua   | Chỉ chuyển trạng thái theo luồng mục 2.4                                      | Controller                                                    |
| PhieuSuaChua   | Khách chỉ xem/hủy phiếu của mình (đổi mã trên URL không lộ dữ liệu)           | Controller so `MaKhachHang` với Session                       |
| ChiTietSuaChua | `SoLuong > 0`, `DonGia >= 0`, `ThanhTien = SoLuong * DonGia`                  | `[Range]` + CHECK; Controller gán `ThanhTien`                 |
| ChiTietSuaChua | Chỉ thêm/sửa khi phiếu đang `DangXuLy`                                        | Controller                                                    |
| ChiTietSuaChua | `SoLuong <= LinhKien.SoLuongTon`; lưu xong thì trừ tồn kho (đề xuất)          | LINQ ở Controller/Service, cảnh báo khi không đủ (đề mục 8.5) |
| LinhKien       | `TenLinhKien` không trùng; `DonGia >= 0`; `SoLuongTon >= 0`                   | `[Required]` + unique index + `[Range]` + CHECK               |

## 4. Migration

Quy trình theo đề (mục 11.1, 21): thống nhất Entity → DbContext → **một** Migration đầu tiên → các thành viên pull về dùng.

Thứ tự phụ thuộc giữa các bảng (EF Core tự sắp xếp khi tạo Migration, nhưng cần nhớ khi viết `DbSeeder`):

1. `TaiKhoan`, `LoaiThietBi`, `LinhKien` (không phụ thuộc bảng nào)
2. `KhachHang`, `KyThuatVien`, `NhatKyDangNhap` (phụ thuộc `TaiKhoan`)
3. `ThietBi` (phụ thuộc `LoaiThietBi`, `KhachHang`)
4. `PhieuSuaChua` (phụ thuộc `ThietBi`, `KhachHang`, `TaiKhoan`)
5. `LichSuTrangThaiPhieu`, `ChiTietSuaChua`, `ThongBao` (phụ thuộc `PhieuSuaChua`), `TepDinhKem`

Repo hiện có Migration `InitialCreate` (7 bảng, 08/10/2026). Sau khi Model đã khớp 12 bảng, **một người** (trưởng nhóm) làm trên `main`: xóa database local, xóa thư mục `Migrations`, chạy `dotnet ef migrations add InitialCreate`, push. Cả nhóm pull về, chạy `dotnet ef database drop -f` rồi `dotnet run` (ứng dụng tự `Migrate()` + seed). Từ lúc đó không ai tự `Add-Migration`; muốn đổi Entity thì báo trưởng nhóm, đặt tên Migration rõ nghĩa (ví dụ `ThemCotXyz`).

## 5. Model trong code

Bản 11/10/2026 có đủ **12/12 Model** (mỗi Model một file trong `Models/`, tên bảng khai báo bằng `[Table("...")]` trùng ERD) cùng `Models/Enums/Enums.cs` (9 enum, mục 2.3). `Data/AppDbContext.cs` khai báo 12 `DbSet`, enum lưu chuỗi, unique index, index lọc/thống kê, quan hệ 1–1 `KhachHang`/`KyThuatVien` với `TaiKhoan`, mọi FK `Restrict`, và 26 CHECK constraint có tên trùng từng dòng trong `RepairPro.sql`.

Cách kiểm tra Model ↔ ERD: so tên cột, thứ tự cột và NULL/NOT NULL của Model với `schema.dbml`; giữ đúng thứ tự khai báo thuộc tính như trong ERD.

Việc còn lại của nhóm:

- [ ] Đưa Model, Enums, `AppDbContext`, `DbSeeder` vào `main` (trưởng nhóm).
- [ ] Xóa Migration cũ và tạo lại `InitialCreate` theo mục 4.
- [ ] Mỗi người điền comment đầu file (Họ và tên, Mã sinh viên) vào các Model mình phụ trách (README mục Quy ước code).
- [x] Module 1: `Program.cs` thêm `AddSession()` / `UseSession()` và băm mật khẩu bằng `PasswordHasher<TaiKhoan>` (sửa lại `TaoTaiKhoan` trong `DbSeeder`). Controller dùng `[PhanQuyen(VaiTroEnum.Admin, ...)]` (Helpers/PhanQuyenAttribute.cs) và lấy người dùng bằng `HttpContext.Session.LayMaTaiKhoan()` / `LayVaiTro()` (Helpers/PhienDangNhap.cs).
- [ ] Khi sửa Entity/enum: cập nhật cả 4 file trong `Doc/` cùng PR (quy tắc đầu file).

## 6. Dữ liệu mẫu

Nguồn dữ liệu mẫu **duy nhất** là `Data/DbSeeder.cs`; `RepairPro.sql` không chứa `INSERT` để tránh hai bộ dữ liệu lệch nhau. Số lượng theo đề mục 16: 5 loại thiết bị, 15 thiết bị, 30 khách hàng, 2 Admin, 3 nhân viên, 45 phiếu sửa chữa (nhiều trạng thái), 18 chi tiết sửa chữa; ngoài ra có 3 hồ sơ `KyThuatVien` (mỗi nhân viên một hồ sơ) và lịch sử trạng thái của từng phiếu. Mỗi thiết bị có 3 phiếu: phiếu mới nhất lần lượt rơi vào 5 trạng thái, 2 phiếu cũ đều đã đóng, nên không thiết bị nào có quá 1 phiếu đang mở (mục 2.5). Trạng thái thiết bị suy ra từ phiếu mới nhất (`DangXuLy` → `DangSuaChua`, `TuChoi` → `KhongKhaDung`). Danh sách tài khoản kiểm thử ở [README](../README.md).
