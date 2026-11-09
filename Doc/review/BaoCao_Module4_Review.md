# BÁO CÁO REVIEW CODE VÀ THUYẾT MINH MODULE 4

**Đề tài 17:** Quản lý bảo hành và sửa chữa thiết bị (ASP.NET Core MVC)\
**Nhóm:** UNETI04 - Lớp DHTI17A4HN\
**Sinh viên phụ trách:** \[CẦN BỔ SUNG HỌ TÊN\]\
**Mã sinh viên:** \[CẦN BỔ SUNG MSSV\]\
**Phạm vi:** Tiếp nhận -- chẩn đoán -- sửa chữa -- quản lý kỹ thuật viên
-- cập nhật trạng thái phiếu và chi phí.

------------------------------------------------------------------------

## 1. TỔNG QUAN VÀ PHẠM VI TRÁCH NHIỆM

Module 4 đảm nhiệm luồng xử lý nghiệp vụ sau khi khách hàng gửi phiếu
sửa chữa. Các phần việc thể hiện trong mã nguồn gồm: - Quản lý hồ sơ kỹ
thuật viên và tài khoản kỹ thuật viên. - Xem danh sách phiếu cần xử lý,
lọc theo từ khóa, loại thiết bị, trạng thái và khoảng ngày; sắp xếp và
phân trang. - Xem chi tiết phiếu, thông tin khách hàng, thiết bị và lịch
sử trạng thái. - Kiểm tra điều kiện tiếp nhận và chuyển trạng thái theo
luồng hợp lệ. - Ghi nhận kỹ thuật viên, chẩn đoán, công sửa chữa và
thông tin chi tiết liên quan. - Ghi lịch sử thay đổi trạng thái, người
thực hiện và thời điểm. - Đồng bộ trạng thái phiếu với trạng thái thiết
bị.

## 2. KIẾN TRÚC VÀ CÁC THÀNH PHẦN CODE CHÍNH

Luồng nghiệp vụ phiếu:
`PhieuSuaChuaController -> PhieuSuaChuaService -> IUnitOfWork/Repositories -> EF Core -> SQL Server`.

Luồng kỹ thuật viên:
`KyThuatVienController -> KyThuatVienService -> KyThuatVienRepository/TaiKhoanRepository`.

### 2.1. Hồ sơ kỹ thuật viên

-   `Models/KyThuatVien.cs` liên kết hồ sơ kỹ thuật viên với tài khoản
    qua `MaTaiKhoan`.
-   `KyThuatVienController`, `KyThuatVienService` và
    `IKyThuatVienService` cung cấp phần quản lý hồ sơ kỹ thuật viên.
-   `KyThuatVienRepository` hỗ trợ truy vấn hồ sơ và danh sách phiếu
    được phân công.
-   Các ViewModel nằm trong `ViewModels/KyThuatVienViewModels.cs`.
-   Comment trong các file này hiện vẫn để placeholder tên và mã sinh
    viên; cần xác nhận người phụ trách trước khi hoàn thiện báo cáo cá
    nhân.

### 2.2. Danh sách và chi tiết phiếu cần xử lý

`PhieuSuaChuaRepository.LayDanhSachPhanTrangAsync` xây dựng truy vấn có
`Include` khách hàng, thiết bị và loại thiết bị. Truy vấn hỗ trợ: - Từ
khóa theo tên khách hàng, số điện thoại, tên thiết bị và Serial
Number. - Lọc theo loại thiết bị, trạng thái, ngày bắt đầu và ngày kết
thúc. - Sắp xếp theo ngày tiếp nhận, tên khách hàng hoặc tên thiết bị. -
Đếm tổng số phiếu, sau đó dùng `Skip()`/`Take()` để phân trang.
Repository cũng có truy vấn lấy chi tiết phiếu kèm thông tin khách hàng,
thiết bị, tài khoản tiếp nhận, chi tiết sửa chữa, kỹ thuật viên, linh
kiện và lịch sử.

### 2.3. Kiểm soát luồng trạng thái

`PhieuSuaChuaService.CapNhatTrangThaiAsync` thực hiện chuyển trạng thái
trong transaction: - Kiểm tra chuyển trạng thái bằng hàm
`ChuyenTrangThaiHopLe`. - Khi chuyển sang `DangXuLy`, kiểm tra thiết bị
đang ở trạng thái `KhaDung`, sau đó cập nhật thiết bị thành
`DangSuaChua`. - Khi phiếu `HoanThanh`, thiết bị được đưa về
`KhaDung`. - Khi hủy phiếu đang xử lý, thiết bị cũng được đưa về
`KhaDung`. - Khi chuyển sang `Huy` hoặc `TuChoi`, Service xử lý điều
kiện ghi chú theo quy tắc của hệ thống. - Tạo bản ghi
`LichSuTrangThaiPhieu` với trạng thái cũ, trạng thái mới, tài khoản thực
hiện và thời điểm. - Lưu thay đổi và commit transaction để giữ tính nhất
quán giữa phiếu, thiết bị và lịch sử.

### 2.4. Chẩn đoán và chi tiết sửa chữa

Mã nguồn có `ThemChiTietSuaChuaViewModel` và `ChiTietSuaChuaService` để
ghi nhận chi tiết sửa chữa. Việc thêm chi tiết được kiểm tra phiếu đang
ở trạng thái `DangXuLy`. Khi có linh kiện, Service kiểm tra linh kiện
khả dụng và thao tác trong transaction để cập nhật chi tiết và dữ liệu
kho liên quan. Phần quản lý chi tiết/chi phí có giao thoa với Module 5;
cần căn cứ phân công thực tế của nhóm khi chốt trách nhiệm cá nhân.

## 3. BẢNG TỔNG HỢP FILE CHÍNH

  ----------------------------------------------------------------------------------------------------------
                    STT File                                               Thành phần       Trách nhiệm
  --------------------- -------------------------------------------------- ---------------- ----------------
                      1 `Models/KyThuatVien.cs`                            Entity           Hồ sơ kỹ thuật
                                                                                            viên liên kết
                                                                                            tài khoản

                      2 `Models/LichSuTrangThaiPhieu.cs`                   Entity           Lưu lịch sử
                                                                                            chuyển trạng
                                                                                            thái

                      3 `Controllers/KyThuatVienController.cs`             Controller       Quản lý hồ sơ và
                                                                                            danh sách phiếu
                                                                                            theo kỹ thuật
                                                                                            viên

                      4 `Services/NguoiDung/KyThuatVienService.cs`         Service          Nghiệp vụ hồ sơ
                                                                                            kỹ thuật viên

                      5 `Services/NguoiDung/IKyThuatVienService.cs`        Interface        Hợp đồng nghiệp
                                                                                            vụ kỹ thuật viên

                      6 `Repositories/KyThuatVienRepository.cs`            Repository       Truy vấn kỹ
                                                                                            thuật viên và
                                                                                            phiếu được phân
                                                                                            công

                      7 `ViewModels/KyThuatVienViewModels.cs`              ViewModel        Dữ liệu form kỹ
                                                                                            thuật viên

                      8 `Models/PhieuSuaChua.cs`                           Entity           Dữ liệu phiếu và
                                                                                            trạng thái
                                                                                            nghiệp vụ

                      9 `Repositories/PhieuSuaChuaRepository.cs`           Repository       Danh sách
                                                                                            lọc/phân trang
                                                                                            và truy vấn chi
                                                                                            tiết phiếu

                     10 `Services/PhieuSuaChua/PhieuSuaChuaService.cs`     Service          Chuyển trạng
                                                                                            thái, kiểm tra
                                                                                            điều kiện và ghi
                                                                                            lịch sử

                     11 `Services/PhieuSuaChua/IPhieuSuaChuaService.cs`    Interface        Hợp đồng nghiệp
                                                                                            vụ phiếu

                     12 `ViewModels/PhieuSuaChuaDanhSachViewModel.cs`      ViewModel        Bộ lọc và phân
                                                                                            trang phiếu

                     13 `ViewModels/ThemChiTietSuaChuaViewModel.cs`        ViewModel        Dữ liệu chẩn
                                                                                            đoán, công sửa
                                                                                            chữa, linh kiện

                     14 `Services/PhieuSuaChua/ChiTietSuaChuaService.cs`   Service          Nghiệp vụ chi
                                                                                            tiết sửa chữa và
                                                                                            tồn kho

                     15 `Controllers/ChiTietSuaChuaController.cs`          Controller       Thao tác chi
                                                                                            tiết sửa chữa

                     16 `Views/KyThuatVien/*`                              Razor Views      Quản lý kỹ thuật
                                                                                            viên và danh
                                                                                            sách phiếu

                     17 `Views/PhieuSuaChua/*`, `Views/ChiTietSuaChua/*`   Razor Views      Giao diện xử lý
                                                                                            phiếu và chi
                                                                                            tiết sửa chữa
  ----------------------------------------------------------------------------------------------------------

## 4. KẾT QUẢ ĐỐI SOÁT VÀ KIỂM THỬ ĐỀ XUẤT

Các nhánh logic chuyển trạng thái và cập nhật thiết bị đã được thể hiện
trong Service. Cần chạy kiểm thử thực tế: 1. Chuyển phiếu từ `ChoXuLy`
sang `DangXuLy` khi thiết bị khả dụng. 2. Từ chối chuyển trạng thái
không hợp lệ; thử tiếp nhận thiết bị không khả dụng. 3. Chuyển phiếu
sang `HoanThanh` và kiểm tra thiết bị trở về khả dụng. 4. Hủy/từ chối
phiếu và kiểm tra điều kiện ghi chú. 5. Đối chiếu lịch sử trạng thái:
trạng thái cũ/mới, tài khoản và thời gian. 6. Lọc danh sách phiếu theo
từ khóa, loại, trạng thái, khoảng ngày; thử sắp xếp và phân trang. 7.
Thử thêm chi tiết sửa chữa khi phiếu chưa ở `DangXuLy` và khi đã ở
`DangXuLy`. 8. Kiểm tra danh sách phiếu theo kỹ thuật viên được phân
công.

**Lưu ý cần xác nhận:** Nhiều file thuộc Module 4 còn comment
placeholder người thực hiện. Một số file `PhieuSuaChua*` và
`ChiTietSuaChua*` có comment phân chia nhiều module. Không nên ghi nhận
toàn bộ file là công việc cá nhân của một thành viên nếu chưa đối chiếu
lịch sử Git và phân công nhóm.
