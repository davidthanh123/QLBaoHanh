# Hệ thống Quản lý Bảo hành và Sửa chữa Thiết bị

Đề tài 17 — ASP.NET Core 10 MVC + Entity Framework Core 10 + SQL Server. Nhóm UNETI04, lớp DHTI17A4HN.

Quản lý loại thiết bị, thiết bị và khách hàng; khách hàng tạo phiếu sửa chữa và theo dõi tiến độ; nhân viên kỹ thuật tiếp nhận, chẩn đoán, sửa chữa, tính chi phí linh kiện; Dashboard và thống kê bằng LINQ.

## Tài liệu thiết kế

- [Nội dung thiết kế và các quyết định (THIETKE.md)](./Doc/THIETKE.md) — **đọc trước khi sửa Entity**
- [Sơ đồ ERD (ERD.md)](./Doc/ERD.md) · [schema.dbml](./Doc/schema.dbml) · [Script SQL tham chiếu (RepairPro.sql)](./Doc/RepairPro.sql)
- [Quy ước giao diện (GIAODIEN.md)](./Doc/GIAODIEN.md)

## Công nghệ sử dụng

- .NET 10 SDK, ASP.NET Core 10 MVC
- Entity Framework Core 10.0.12 (Code First + Migration), SQL Server
- Razor View, Bootstrap 5.3, jQuery, jQuery Validation
- Session cho đăng nhập và phân quyền

## Thành viên nhóm và phân công

| Module | Người phụ trách      | Nội dung chính                                                           | Entity phụ trách                               |
| ------ | -------------------- | ------------------------------------------------------------------------ | ---------------------------------------------- |
| 1      | Vân                  | Tài khoản, đăng nhập/đăng xuất, phân quyền (Session), loại thiết bị      | `TaiKhoan`, `LoaiThietBi`, `NhatKyDangNhap`    |
| 2      | Hoàng Thành          | Thiết bị: CRUD, tìm kiếm, lọc, sắp xếp, phân trang                       | `ThietBi`, `TepDinhKem`                        |
| 3      | David (nhóm trưởng)  | Khách hàng, hồ sơ cá nhân, tạo phiếu sửa chữa, theo dõi, hủy phiếu       | `KhachHang`, `PhieuSuaChua`                    |
| 4      | Thảo                 | Tiếp nhận, chẩn đoán, sửa chữa, cập nhật trạng thái và chi phí           | `KyThuatVien`, `LichSuTrangThaiPhieu`          |
| 5      | Vũ                   | Linh kiện, kết quả sửa chữa, Dashboard, thống kê, báo cáo                | `LinhKien`, `ChiTietSuaChua`, `ThongBao`       |

## Cấu trúc thư mục

```
QLBaoHanh/
├── README.md
├── Doc/                               # Tài liệu thiết kế: THIETKE.md, ERD.md, schema.dbml, RepairPro.sql, GIAODIEN.md
├── QLBaoHanh_UNETI04_DHTI17A4HN.slnx
└── QLBaoHanh_UNETI04_DHTI17A4HN/
    ├── Controllers/                   # 1 Controller cho mỗi Entity chính, cùng người phụ trách như Entity tương ứng
    ├── Models/                        # Entity theo ERD, mỗi Entity 1 file, đúng 1 người phụ trách
    │   └── Enums/                     # Enums.cs — 9 enum dùng chung (xem THIETKE.md mục 2.3)
    ├── ViewModels/                    # Lớp dữ liệu riêng cho View, không ánh xạ CSDL (cần thêm khi dùng)
    ├── Helpers/                       # UiHelper.cs: nhãn + màu badge cho enum, định dạng tiền/ngày (theo GIAODIEN.md mục 4, 5)
    ├── Data/                          # AppDbContext, DbSeeder (nguồn dữ liệu mẫu duy nhất)
    ├── Migrations/                    # Chỉ trưởng nhóm tạo Migration (xem THIETKE.md mục 4)
    ├── Views/                         # .cshtml chia theo Controller; Shared/ (layout, sidebar) chỉ 1 người sửa
    ├── wwwroot/                       # css/js của project; lib/ là thư viện ngoài, không sửa tay
    ├── Program.cs                     # Khai báo service/middleware; tự Migrate + Seed khi khởi động
    └── appsettings.json               # Connection string mặc định
```

**Quy tắc tránh conflict Git:** mỗi Entity/Controller là 1 file riêng do đúng 1 người phụ trách theo bảng trên. Không sửa file của người khác: cần thay đổi thì nhắn người phụ trách hoặc đi qua Pull Request. Riêng `PhieuSuaChua`, David là chủ file Entity; Thảo cần thêm cột thì báo David.

## Chạy project (cho thành viên clone/pull về)

Yêu cầu: .NET 10 SDK, SQL Server (bản đầy đủ hoặc LocalDB), Visual Studio Code hoặc Visual Studio.

```bash
git clone https://github.com/davidthanh123/QLBaoHanh.git
cd QLBaoHanh/QLBaoHanh_UNETI04_DHTI17A4HN
dotnet restore
dotnet run
```

`Program.cs` tự chạy `Migrate()` rồi `DbSeeder.Seed()` khi khởi động, nên lần chạy đầu sẽ tự tạo database `QLBaoHanh` cùng dữ liệu mẫu, không cần chạy `RepairPro.sql`.

Connection string mặc định trong `appsettings.json`:

```
Server=.;Database=QLBaoHanh;Trusted_Connection=True;TrustServerCertificate=True;
```

Nếu SQL Server của máy bạn khác (ví dụ `(localdb)\MSSQLLocalDB` hoặc `.\SQLEXPRESS`), tạo file `appsettings.Development.json` cạnh `appsettings.json` (file này nằm trong `.gitignore`, mỗi người tự tạo) và ghi đè:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QLBaoHanh;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Muốn làm lại database từ đầu: `dotnet ef database drop -f` rồi `dotnet run` (cài công cụ một lần: `dotnet tool install --global dotnet-ef`).

## Tài khoản kiểm thử

Mật khẩu của mọi tài khoản mẫu: `123456` (do `DbSeeder` tạo).

| Vai trò    | Tên đăng nhập                         | Ghi chú                          |
| ---------- | ------------------------------------- | -------------------------------- |
| Admin      | `admin`, `admin2`                     |                                  |
| Nhân viên  | `nhanvien1`, `nhanvien2`, `nhanvien3` |                                  |
| Khách hàng | `khachhang1`, `khachhang2`            |                                  |
| Khách hàng | `khachhang3`                          | Tài khoản **bị khóa**, dùng để kiểm thử đăng nhập |

## Quy ước code

- **Comment đầu mỗi file mình phụ trách** (bắt buộc, đề mục 18; file nhiều người làm thì ghi từng người):
  ```csharp
  // Họ và tên: Nguyễn Văn A
  // Mã sinh viên: 23103100000
  // Nội dung thực hiện: Quản lý thiết bị, tìm kiếm, lọc, sắp xếp và phân trang.
  ```
  File `.cshtml` dùng Razor comment `@* ... *@` với cùng 3 dòng.
- **Tên class/file:** PascalCase, số ít, trùng tên bảng trong ERD (`ThietBi.cs`, không viết `ThietBis.cs`). Biến cục bộ camelCase.
- **Trạng thái/vai trò/mức độ:** dùng enum trong `Models/Enums`, không viết chuỗi tự do như `"Chờ xử lý"`.
- **Validation:** `[Required]`, `[StringLength]`, `[Range]`... đặt ngay trong Entity; quy tắc nghiệp vụ không biểu diễn được bằng Annotation thì kiểm tra ở Controller bằng LINQ.
- **Phân quyền:** kiểm tra ở Controller, không chỉ ẩn menu trên View.
- **Giao diện:** theo [GIAODIEN.md](./Doc/GIAODIEN.md), không hard-code màu trong `.cshtml`.
- **Đổi Entity/cột/enum:** cập nhật cả 4 file trong `Doc/` cùng Model trong cùng một Pull Request.

## Quy ước nhánh Git

Mỗi người làm trên 1 nhánh riêng theo module, **không code thẳng lên `main`**. Tên nhánh theo dạng `<Viết tắt tên>_module_<số>` (ví dụ `NDT-module_3`).

```bash
git checkout main
git pull                                  # cập nhật code mới nhất trước khi bắt đầu
git checkout NDT-module_3                 # nhánh của mình
git merge main                            # kéo thay đổi mới của main về nhánh
# ... code ...
git add .
git commit -m "[23103100239] [Module 3] Them Controller KhachHang"
git push origin NDT-module_3
```

Sau đó tạo **Pull Request** vào `main` trên GitHub, nhờ trưởng nhóm (hoặc 1 bạn khác) xem qua rồi mới Merge.

## Quy ước commit message

Format theo đề (mục 21): `[Mã SV] [Module] Nội dung công việc` (tiếng Việt không dấu cho gọn).

- `[23103100239] [Module 3] Them Entity KhachHang`
- `[23103100239] [Setup] Them Models, AppDbContext, DbSeeder, Migration InitialCreate`

Commit đều theo tiến độ, không dồn vào cuối; lịch sử commit phải khớp phần việc ghi ở comment đầu file.

## Checklist trước khi merge vào main

- [ ] Code build được, không lỗi đỏ
- [ ] Đã `merge main` vào nhánh của mình và giải quyết conflict
- [ ] Đã ghi comment đầu file (Họ và tên, Mã sinh viên, Nội dung thực hiện)
- [ ] Đã chạy thử chức năng (không chỉ build), gồm cả trường hợp dữ liệu không hợp lệ và sai quyền
- [ ] Nếu đụng Entity: đã cập nhật `Doc/` và báo trưởng nhóm để tạo lại Migration
