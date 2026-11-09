# BÁO CÁO REVIEW CODE VÀ THUYẾT MINH MODULE 5

**Đề tài 17:** Quản lý bảo hành và sửa chữa thiết bị (ASP.NET Core MVC)\
**Nhóm:** UNETI04 - Lớp DHTI17A4HN\
**Sinh viên phụ trách theo comment mã nguồn:** Nguyễn Văn Vũ\
**Mã sinh viên:** \[CẦN BỔ SUNG MSSV\]\
**Phạm vi:** Quản lý linh kiện -- chi tiết sửa chữa -- Dashboard --
thống kê.

------------------------------------------------------------------------

## 1. TỔNG QUAN VÀ PHẠM VI TRÁCH NHIỆM

Module 5 quản lý danh mục linh kiện và cung cấp số liệu tổng hợp phục vụ
theo dõi hoạt động sửa chữa. Mã nguồn hiện có các phần: - Quản lý linh
kiện: danh sách, tìm kiếm, lọc trạng thái, sắp xếp, thêm, sửa, xem chi
tiết và xóa. - Kiểm tra tên linh kiện không trùng; quản lý đơn giá, đơn
vị tính, số lượng tồn và trạng thái hoạt động. - Cung cấp danh sách linh
kiện khả dụng/còn hàng cho nghiệp vụ thêm chi tiết sửa chữa. - Entity
chi tiết sửa chữa lưu kỹ thuật viên, linh kiện, số lượng, đơn giá, thành
tiền và kết quả chẩn đoán. - Dashboard tổng hợp số khách hàng, thiết bị,
phiếu, linh kiện, kỹ thuật viên, chi tiết sửa chữa và giá trị phát sinh;
thống kê theo trạng thái, loại thiết bị và các nhóm dữ liệu liên quan.

## 2. KIẾN TRÚC VÀ CÁC THÀNH PHẦN CODE CHÍNH

Luồng quản lý linh kiện:
`LinhKienController -> ILinhKienService/LinhKienService -> IUnitOfWork -> LinhKienRepository -> EF Core`.

Luồng Dashboard:
`DashboardController -> IDashboardService/DashboardService -> AppDbContext/EF Core -> SQL Server`.

### 2.1. Quản lý linh kiện

-   `Models/LinhKien.cs` định nghĩa mã linh kiện, tên, đơn vị tính, đơn
    giá, số lượng tồn và trạng thái.
-   `LinhKienController` tiếp nhận thao tác từ giao diện.
-   `LinhKienService` xử lý nghiệp vụ và kiểm tra dữ liệu.
-   `LinhKienRepository` hỗ trợ tìm kiếm theo tên, lọc đang hoạt
    động/ngừng hoạt động và sắp xếp theo giá hoặc số lượng tồn.
-   Repository có truy vấn lấy linh kiện đang hoạt động và còn hàng;
    cũng có chế độ giữ lại linh kiện đang được chọn khi chỉnh sửa dữ
    liệu liên quan.
-   Kiểm tra trùng tên được thực hiện bằng `AnyAsync()` và có hỗ trợ
    loại trừ mã hiện tại khi cập nhật.

### 2.2. Chi tiết sửa chữa và liên kết kho

`Models/ChiTietSuaChua.cs` lưu mã phiếu, mã kỹ thuật viên, mã linh kiện,
số lượng, đơn giá và thành tiền. Quan hệ này kết nối phiếu sửa chữa với
kỹ thuật viên và linh kiện.

`ChiTietSuaChuaService` thực hiện nghiệp vụ chi tiết sửa chữa và tồn
kho. Theo mã nguồn, thao tác thêm chi tiết được kiểm tra phiếu đang ở
trạng thái `DangXuLy`; khi chọn linh kiện, Service truy vấn linh kiện
theo mã và trạng thái khả dụng. Các thao tác liên quan được thực hiện
trong transaction để hạn chế tình trạng lưu chi tiết nhưng không đồng bộ
kho.

### 2.3. Dashboard và thống kê

`DashboardService.LayThongKeAsync` sử dụng Entity Framework Core/LINQ để
tổng hợp: - Tổng khách hàng, thiết bị, phiếu sửa chữa, linh kiện, kỹ
thuật viên và chi tiết sửa chữa. - Tổng thành tiền từ các chi tiết sửa
chữa bằng `SumAsync`. - Số lượng phiếu theo trạng thái bằng `GroupBy`. -
Thống kê thiết bị theo loại thiết bị. - Các nhóm thống kê theo dữ liệu
phiếu và chi tiết sửa chữa được ánh xạ sang đối tượng thống kê
Dashboard.

`DashboardController.Index` gọi Service và chuyển dữ liệu sang View.
`Views/Dashboard/Index.cshtml` hiển thị số liệu tổng hợp.

## 3. BẢNG TỔNG HỢP FILE CHÍNH

  -----------------------------------------------------------------------------------------------------------
                    STT File                                                Thành phần       Trách nhiệm
  --------------------- --------------------------------------------------- ---------------- ----------------
                      1 `Models/LinhKien.cs`                                Entity           Dữ liệu danh mục
                                                                                             linh kiện và tồn
                                                                                             kho

                      2 `Controllers/LinhKienController.cs`                 Controller       Các Action quản
                                                                                             lý linh kiện

                      3 `Services/ThietBiVaLinhKien/LinhKienService.cs`     Service          Nghiệp vụ
                                                                                             thêm/sửa/xóa và
                                                                                             kiểm tra linh
                                                                                             kiện

                      4 `Services/ThietBiVaLinhKien/ILinhKienService.cs`    Interface        Hợp đồng nghiệp
                                                                                             vụ linh kiện

                      5 `Repositories/LinhKienRepository.cs`                Repository       Tìm kiếm, lọc,
                                                                                             sắp xếp và truy
                                                                                             vấn linh kiện
                                                                                             khả dụng

                      6 `Models/ChiTietSuaChua.cs`                          Entity           Chi tiết sửa
                                                                                             chữa, kỹ thuật
                                                                                             viên, linh kiện
                                                                                             và chi phí

                      7 `Services/PhieuSuaChua/ChiTietSuaChuaService.cs`    Service          Nghiệp vụ chi
                                                                                             tiết sửa chữa và
                                                                                             tồn kho

                      8 `Services/PhieuSuaChua/IChiTietSuaChuaService.cs`   Interface        Hợp đồng nghiệp
                                                                                             vụ chi tiết sửa
                                                                                             chữa

                      9 `Repositories/ChiTietSuaChuaRepository.cs`          Repository       Truy vấn dữ liệu
                                                                                             chi tiết sửa
                                                                                             chữa

                     10 `Controllers/ChiTietSuaChuaController.cs`           Controller       Các thao tác
                                                                                             giao diện chi
                                                                                             tiết sửa chữa

                     11 `ViewModels/ThemChiTietSuaChuaViewModel.cs`         ViewModel        Dữ liệu nhập
                                                                                             chẩn đoán, công
                                                                                             sửa chữa và linh
                                                                                             kiện

                     12 `Controllers/DashboardController.cs`                Controller       Điều phối trang
                                                                                             Dashboard

                     13 `Services/HeThong/DashboardService.cs`              Service          Truy vấn và tổng
                                                                                             hợp số liệu
                                                                                             Dashboard

                     14 `Services/HeThong/IDashboardService.cs`             Interface        Hợp đồng lấy số
                                                                                             liệu Dashboard

                     15 `Views/LinhKien/*`                                  Razor Views      Danh sách, thêm,
                                                                                             sửa, chi tiết,
                                                                                             xóa linh kiện

                     16 `Views/ChiTietSuaChua/*`                            Razor Views      Giao diện chi
                                                                                             tiết sửa chữa

                     17 `Views/Dashboard/Index.cshtml`                      Razor View       Hiển thị số liệu
                                                                                             thống kê
  -----------------------------------------------------------------------------------------------------------

## 4. KẾT QUẢ ĐỐI SOÁT VÀ KIỂM THỬ ĐỀ XUẤT

Mã nguồn thể hiện các truy vấn thống kê bằng `CountAsync`, `SumAsync`,
`GroupBy`, `OrderBy` và các truy vấn kiểm tra bằng `AnyAsync`. Cần kiểm
thử thực tế: 1. Thêm linh kiện mới, thêm tên trùng và cập nhật tên linh
kiện. 2. Lọc linh kiện hoạt động/ngừng hoạt động; sắp xếp theo đơn giá
và số lượng tồn. 3. Kiểm tra linh kiện hết hàng hoặc ngừng hoạt động
không xuất hiện trong danh sách lựa chọn phù hợp. 4. Thêm chi tiết sửa
chữa khi phiếu đang xử lý; thử thao tác khi phiếu chưa ở trạng thái cho
phép. 5. Kiểm tra số lượng tồn và tổng tiền sau khi thêm chi tiết; thử
số lượng vượt tồn. 6. Đối chiếu số liệu Dashboard với dữ liệu trong các
bảng tương ứng. 7. Thêm/sửa dữ liệu và xác nhận số liệu tổng hợp thay
đổi đúng. 8. Kiểm tra quyền truy cập Dashboard và các màn hình quản lý
linh kiện.

**Lưu ý đối soát:** Comment của các file `LinhKienController.cs`,
`LinhKienService.cs`, `ILinhKienService.cs` ghi Nguyễn Văn Vũ nhưng MSSV
vẫn là placeholder. Các file `ChiTietSuaChua*` cũng còn placeholder và
có phần nghiệp vụ giao thoa Module 4. Cần bổ sung MSSV và xác nhận phân
công thực tế trước khi nộp báo cáo cá nhân.
