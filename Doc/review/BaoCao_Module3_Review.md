# BÁO CÁO REVIEW CODE VÀ THUYẾT MINH MODULE 3

**Đề tài 17:** Quản lý bảo hành và sửa chữa thiết bị (ASP.NET Core MVC)\
**Nhóm:** UNETI04 - Lớp DHTI17A4HN\
**Sinh viên phụ trách theo comment mã nguồn:** David Thành\
**Mã sinh viên:** 23103100239\
**Phạm vi:** Quản lý khách hàng -- hồ sơ cá nhân -- tạo phiếu sửa chữa
-- theo dõi và hủy phiếu của khách hàng.

------------------------------------------------------------------------

## 1. TỔNG QUAN VÀ PHẠM VI TRÁCH NHIỆM

Module 3 quản lý hồ sơ khách hàng và các yêu cầu sửa chữa do khách hàng
tạo. Module kết nối tài khoản đăng nhập với hồ sơ khách hàng, thiết bị
sở hữu và phiếu sửa chữa. Phạm vi thể hiện trong mã nguồn gồm: - Quản lý
danh sách khách hàng dành cho Admin/Nhân viên: tìm kiếm, lọc trạng
thái/loại khách hàng, phân trang, xem chi tiết, thêm, sửa, đổi trạng
thái và xóa có kiểm soát. - Khách hàng xem và cập nhật hồ sơ cá nhân gắn
với tài khoản hiện tại. - Tạo phiếu sửa chữa cho thiết bị thuộc khách
hàng, kiểm tra điều kiện thiết bị và ngăn tạo phiếu đang mở trùng cho
cùng thiết bị. - Khách hàng xem danh sách phiếu của mình, xem chi
tiết/theo dõi và hủy phiếu trong điều kiện cho phép. - Ghi lịch sử trạng
thái khi tạo hoặc hủy phiếu.

## 2. KIẾN TRÚC VÀ CÁC THÀNH PHẦN CODE CHÍNH

Luồng quản lý khách hàng:
`KhachHangController -> IKhachHangService/KhachHangService -> IUnitOfWork -> KhachHangRepository -> EF Core`.

Luồng tạo/theo dõi phiếu:
`PhieuSuaChuaController -> IPhieuSuaChuaService/PhieuSuaChuaService -> Repository/UnitOfWork -> SQL Server`.

### 2.1. Quản lý khách hàng

-   `Models/KhachHang.cs` mô tả hồ sơ khách hàng và quan hệ với tài
    khoản, thiết bị, phiếu sửa chữa.
-   `KhachHangRepository` hỗ trợ truy vấn danh sách, tìm kiếm/phân
    trang, lấy chi tiết kèm phiếu, tìm hồ sơ theo mã tài khoản, kiểm tra
    trùng CCCD và kiểm tra tài khoản đã liên kết.
-   `KhachHangService` xử lý tạo/sửa hồ sơ, đổi trạng thái, xóa và cập
    nhật hồ sơ cá nhân.
-   Khi đổi trạng thái khách hàng đang hoạt động, Service kiểm tra khách
    hàng có phiếu chưa đóng hay không.
-   Khi xóa, Service kiểm tra dữ liệu phát sinh để tránh xóa hồ sơ đang
    được thiết bị hoặc phiếu sửa chữa tham chiếu.

### 2.2. Hồ sơ cá nhân

`KhachHangController` cung cấp Action `HoSo` cho người dùng đã đăng
nhập. Service truy vấn khách hàng theo mã tài khoản trong Session thay
vì nhận tùy ý mã khách hàng từ người dùng. Việc cập nhật hồ sơ chỉ tác
động hồ sơ liên kết với tài khoản hiện tại.

### 2.3. Tạo phiếu sửa chữa

-   `Models/PhieuSuaChua.cs` lưu thiết bị, khách hàng, ngày tiếp nhận,
    nội dung lỗi, trạng thái, mức độ, hạn dự kiến và ngày hoàn thành.
-   `PhieuSuaChuaService.TaoPhieuAsync` kiểm tra dữ liệu đầu vào, khách
    hàng/thiết bị và trạng thái khả dụng của thiết bị.
-   Thiết bị được chọn phải thuộc khách hàng đang tạo phiếu và ở trạng
    thái cho phép.
-   Service kiểm tra thiết bị có phiếu đang mở hay không để tránh tạo
    yêu cầu trùng.
-   Khi tạo thành công, phiếu có trạng thái ban đầu `ChoXuLy`; đồng thời
    ghi một bản ghi lịch sử trạng thái ban đầu.

### 2.4. Theo dõi và hủy phiếu

-   Repository có truy vấn lấy phiếu theo người dùng và mã khách hàng,
    giúp giới hạn dữ liệu theo chủ sở hữu.
-   Các Action theo dõi/chi tiết sử dụng thông tin tài khoản hiện tại để
    xác định khách hàng.
-   Hủy phiếu được kiểm tra theo trạng thái nghiệp vụ; không cho hủy tùy
    ý các phiếu đã hoàn tất.
-   Lịch sử trạng thái được ghi nhận khi phiếu được tạo hoặc hủy. Phần
    chuyển trạng thái xử lý kỹ thuật chuyên sâu được thực hiện trong
    luồng Module 4.

## 3. BẢNG TỔNG HỢP FILE CHÍNH

  ---------------------------------------------------------------------------------------------------------
                    STT File                                              Thành phần       Trách nhiệm
  --------------------- ------------------------------------------------- ---------------- ----------------
                      1 `Models/KhachHang.cs`                             Entity           Hồ sơ khách hàng
                                                                                           và quan hệ dữ
                                                                                           liệu

                      2 `Models/PhieuSuaChua.cs`                          Entity           Thông tin phiếu
                                                                                           sửa chữa

                      3 `Controllers/KhachHangController.cs`              Controller       CRUD khách hàng
                                                                                           và hồ sơ cá nhân

                      4 `Services/NguoiDung/KhachHangService.cs`          Service          Nghiệp vụ quản
                                                                                           lý khách hàng và
                                                                                           hồ sơ

                      5 `Services/NguoiDung/IKhachHangService.cs`         Interface        Hợp đồng nghiệp
                                                                                           vụ khách hàng

                      6 `Repositories/KhachHangRepository.cs`             Repository       Tìm kiếm, lọc,
                                                                                           phân trang và
                                                                                           kiểm tra liên
                                                                                           kết

                      7 `ViewModels/KhachHangViewModels.cs`               ViewModel        Form, danh sách
                                                                                           phân trang và hồ
                                                                                           sơ

                      8 `Controllers/PhieuSuaChuaController.cs`           Controller       Tạo, theo dõi,
                                                                                           xem và hủy phiếu

                      9 `Services/PhieuSuaChua/PhieuSuaChuaService.cs`    Service          Nghiệp vụ
                                                                                           tạo/theo dõi/hủy
                                                                                           phiếu; có phần
                                                                                           giao thoa với
                                                                                           Module 4

                     10 `Services/PhieuSuaChua/IPhieuSuaChuaService.cs`   Interface        Hợp đồng xử lý
                                                                                           phiếu

                     11 `Services/PhieuSuaChuaDtos.cs`                    DTO              Kiểu dữ liệu kết
                                                                                           quả luồng phiếu

                     12 `Repositories/PhieuSuaChuaRepository.cs`          Repository       Truy vấn phiếu,
                                                                                           lọc và phân
                                                                                           trang

                     13 `ViewModels/PhieuSuaChuaViewModels.cs`            ViewModel        Dữ liệu tạo
                                                                                           phiếu và hủy
                                                                                           phiếu

                     14 `Views/KhachHang/*`                               Razor Views      Danh sách, form,
                                                                                           chi tiết và hồ
                                                                                           sơ khách hàng

                     15 `Views/PhieuSuaChua/*`                            Razor Views      Tạo phiếu, theo
                                                                                           dõi, chi tiết và
                                                                                           hủy phiếu
  ---------------------------------------------------------------------------------------------------------

## 4. KẾT QUẢ ĐỐI SOÁT VÀ KIỂM THỬ ĐỀ XUẤT

Mã nguồn có các thành phần xử lý khách hàng, hồ sơ cá nhân, tạo phiếu và
truy vấn phiếu theo khách hàng. Các tình huống cần kiểm thử thực tế: 1.
Tìm kiếm khách hàng theo thông tin; lọc theo trạng thái và loại; chuyển
trang. 2. Tạo khách hàng với CCCD hoặc tài khoản đã liên kết. 3. Cập
nhật hồ sơ cá nhân và thử truy cập hồ sơ bằng tài khoản khác. 4. Tạo
phiếu với thiết bị thuộc chính khách hàng và thiết bị của khách hàng
khác. 5. Tạo phiếu với thiết bị không khả dụng hoặc đã có phiếu đang mở.
6. Theo dõi phiếu của khách hàng; thay đổi mã phiếu trên URL để kiểm tra
quyền dữ liệu. 7. Hủy phiếu ở trạng thái được phép và không được phép.
8. Đối chiếu lịch sử trạng thái sau khi tạo/hủy phiếu.

**Lưu ý ranh giới Module:** `PhieuSuaChuaService.cs`,
`IPhieuSuaChuaService.cs`, `PhieuSuaChuaDtos.cs` có comment phân công cả
Module 3 và Module 4. Báo cáo này chỉ quy phần tạo/theo dõi/hủy phiếu
của khách hàng cho Module 3; phần tiếp nhận, phân công, cập nhật trạng
thái và xử lý kỹ thuật thuộc Module 4.
