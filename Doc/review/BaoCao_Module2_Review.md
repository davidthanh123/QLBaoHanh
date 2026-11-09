# BÁO CÁO REVIEW CODE VÀ THUYẾT MINH MODULE 2

**Đề tài 17:** Quản lý bảo hành và sửa chữa thiết bị (ASP.NET Core MVC)\
**Nhóm:** UNETI04 - Lớp DHTI17A4HN\
**Sinh viên phụ trách theo comment mã nguồn:** Đào Hoàng Thành\
**Mã sinh viên:** 23103100223\
**Phạm vi:** Quản lý thiết bị -- tìm kiếm -- lọc -- sắp xếp -- phân
trang.

------------------------------------------------------------------------

## 1. TỔNG QUAN VÀ PHẠM VI TRÁCH NHIỆM

Module 2 quản lý thông tin thiết bị được đưa vào hệ thống bảo hành và
sửa chữa. Module liên kết dữ liệu thiết bị với loại thiết bị và khách
hàng; đồng thời cung cấp chức năng tra cứu, cập nhật và kiểm soát trạng
thái thiết bị.

Các chức năng thể hiện trong mã nguồn: - Xem danh sách, xem chi tiết,
thêm mới, cập nhật và xóa thiết bị. - Tìm kiếm theo tên thiết bị, Serial
Number và thông tin khách hàng. - Lọc theo loại thiết bị, hãng sản xuất,
trạng thái thiết bị, tình trạng bảo hành và khoảng giá trị. - Sắp xếp
theo tên, ngày mua, hạn bảo hành và giá trị. - Phân trang truy vấn tại
cơ sở dữ liệu, kích thước trang được cấu hình là 10. - Kiểm tra Serial
Number không trùng; kiểm tra loại thiết bị và khách hàng được chọn đang
hoạt động. - Ngăn xóa thiết bị đã phát sinh phiếu sửa chữa; ngăn thay
đổi khách hàng khi thiết bị đã có phiếu.

## 2. KIẾN TRÚC VÀ CÁC THÀNH PHẦN CODE CHÍNH

Luồng xử lý chính:
`ThietBiController -> IThietBiService/ThietBiService -> IUnitOfWork -> ThietBiRepository -> Entity Framework Core -> SQL Server`.

### 2.1. Entity và dữ liệu liên kết

-   `Models/ThietBi.cs`: thực thể thiết bị, gồm mã thiết bị, mã loại, mã
    khách hàng, Serial Number, tên thiết bị, ngày mua, hạn bảo hành,
    trạng thái, giá trị và các navigation property.
-   Thiết bị liên kết với `LoaiThietBi` và `KhachHang`.
-   `ViewModels/ThietBiViewModels.cs`: chứa điều kiện lọc và ViewModel
    danh sách.

### 2.2. Danh sách, tìm kiếm, lọc và sắp xếp

-   `Controllers/ThietBiController.cs` nhận `ThietBiFilter` từ
    query/form và chuyển cho Service.
-   `Services/ThietBiVaLinhKien/ThietBiService.cs` xử lý phạm vi dữ liệu
    theo vai trò. Với khách hàng, Service xác định mã khách hàng từ mã
    tài khoản để chỉ trả về thiết bị thuộc khách hàng đó.
-   `Repositories/ThietBiRepository.cs` xây dựng truy vấn LINQ trên
    `IQueryable`, kết hợp từ khóa, loại, hãng, trạng thái, giá trị và
    tình trạng bảo hành.
-   Các lựa chọn sắp xếp gồm tên tăng/giảm, ngày mua tăng/giảm, hạn bảo
    hành tăng/giảm và giá trị tăng/giảm.

### 2.3. Phân trang

Repository đếm tổng số bản ghi bằng `CountAsync()`, sau đó áp dụng
`Skip()` và `Take()` trước khi gọi `ToListAsync()`. Vì vậy dữ liệu được
phân trang ở truy vấn EF Core thay vì tải toàn bộ danh sách về trình
duyệt. Kích thước trang trong Service là 10 bản ghi.

### 2.4. Thêm và cập nhật thiết bị

Service chuẩn hóa chuỗi nhập bằng `Trim()`, kiểm tra Serial Number
trùng, loại thiết bị đang hoạt động và khách hàng đang hoạt động. Khi
thêm mới, trạng thái ban đầu được đặt là `KhaDung`. Khi cập nhật,
Service chỉ gán các trường dữ liệu được phép cập nhật, không nhận trạng
thái tùy ý từ form.

### 2.5. Kiểm soát trạng thái và xóa an toàn

-   Không cho phép người dùng tự chuyển thiết bị sang `DangSuaChua`;
    trạng thái này do luồng tiếp nhận/xử lý phiếu sửa chữa cập nhật.
-   Không cho đổi trạng thái thiết bị đang sửa chữa.
-   Không cho chuyển sang `KhongKhaDung` nếu còn phiếu sửa chữa chưa
    đóng.
-   Không cho xóa thiết bị đã có phiếu sửa chữa; hướng xử lý là chuyển
    thiết bị sang không khả dụng.
-   Không cho đổi khách hàng sở hữu thiết bị nếu thiết bị đã phát sinh
    phiếu sửa chữa.

## 3. BẢNG TỔNG HỢP FILE CHÍNH

  ---------------------------------------------------------------------------------------------------------
                    STT File                                              Thành phần       Trách nhiệm
  --------------------- ------------------------------------------------- ---------------- ----------------
                      1 `Models/ThietBi.cs`                               Entity           Cấu trúc dữ liệu
                                                                                           thiết bị và liên
                                                                                           kết loại/khách
                                                                                           hàng

                      2 `Controllers/ThietBiController.cs`                Controller       Các Action danh
                                                                                           sách, chi tiết,
                                                                                           thêm, sửa, đổi
                                                                                           trạng thái, xóa

                      3 `Services/ThietBiVaLinhKien/ThietBiService.cs`    Service          Nghiệp vụ thiết
                                                                                           bị, kiểm tra
                                                                                           điều kiện và
                                                                                           phân quyền dữ
                                                                                           liệu

                      4 `Services/ThietBiVaLinhKien/IThietBiService.cs`   Interface        Hợp đồng nghiệp
                                                                                           vụ thiết bị

                      5 `Repositories/ThietBiRepository.cs`               Repository       LINQ tìm kiếm,
                                                                                           lọc, sắp xếp,
                                                                                           phân trang và
                                                                                           kiểm tra liên
                                                                                           quan

                      6 `ViewModels/ThietBiViewModels.cs`                 ViewModel        Điều kiện lọc và
                                                                                           dữ liệu danh
                                                                                           sách

                      7 `Views/ThietBi/Index.cshtml`                      Razor View       Danh sách và
                                                                                           điều kiện tìm
                                                                                           kiếm/lọc/sắp xếp

                      8 `Views/ThietBi/_Form.cshtml`                      Razor View       Form thêm/sửa,
                                                                                           chọn loại thiết
                                                                                           bị và khách hàng

                      9 `Views/ThietBi/Create.cshtml`                     Razor View       Giao diện thêm
                                                                                           thiết bị

                     10 `Views/ThietBi/Edit.cshtml`                       Razor View       Giao diện cập
                                                                                           nhật thiết bị

                     11 `Views/ThietBi/Details.cshtml`                    Razor View       Thông tin chi
                                                                                           tiết thiết bị
  ---------------------------------------------------------------------------------------------------------

## 4. KẾT QUẢ ĐỐI SOÁT VÀ KIỂM THỬ ĐỀ XUẤT

Đối chiếu mã nguồn cho thấy đã có các nhánh xử lý cho danh sách, lọc,
sắp xếp, phân trang, CRUD, kiểm tra trùng Serial Number và ràng buộc dữ
liệu liên quan. Các tình huống cần chạy kiểm thử thực tế trước khi ghi
nhận là đã kiểm thử thành công:

1.  Tìm kiếm theo tên thiết bị, Serial Number và tên khách hàng.
2.  Kết hợp loại thiết bị, hãng sản xuất, tình trạng bảo hành, trạng
    thái và khoảng giá trị.
3.  Sắp xếp tăng/giảm theo từng tiêu chí; chuyển trang vẫn giữ điều kiện
    lọc.
4.  Thêm thiết bị với Serial Number mới và Serial Number đã tồn tại.
5.  Chọn loại thiết bị hoặc khách hàng đã ngừng hoạt động.
6.  Cập nhật thiết bị đã có phiếu sửa chữa; thử đổi khách hàng sở hữu.
7.  Thử xóa thiết bị đã phát sinh phiếu và thiết bị chưa phát sinh
    phiếu.
8.  Đăng nhập bằng khách hàng và xác nhận chỉ xem được thiết bị thuộc hồ
    sơ của mình.

**Lưu ý đối soát:** Một số comment đầu file `ThietBiService.cs`,
`IThietBiService.cs` và `Models/ThietBi.cs` vẫn để tên/mã sinh viên dạng
placeholder. Báo cáo ghi nhận người phụ trách theo comment đã hoàn chỉnh
ở `ThietBiController.cs`; cần bổ sung đồng nhất thông tin cá nhân ở các
file còn thiếu trước khi nộp.
