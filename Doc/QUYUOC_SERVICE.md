# Quy ước viết Service

Mục đích: các module viết Service theo cùng một khuôn để tên hàm và cách trả kết quả khớp nhau, Controller của người khác gọi được mà không phải đoán.

## 1. Cấu trúc

- Mỗi Entity chính có 1 cặp file trong `Services/`: `IXxxService.cs` (interface) và `XxxService.cs` (cài đặt). Tên theo tên Entity, PascalCase, số ít.
- Service nhận `IUnitOfWork` (hoặc Repository chuyên biệt) qua constructor, tách biệt tầng truy cập dữ liệu (Data Access Layer) khỏi logic nghiệp vụ.
- Đăng ký trong `Program.cs`: `builder.Services.AddScoped<IXxxService, XxxService>();`
- Controller chỉ làm 3 việc: nhận request, kiểm tra quyền (`[PhanQuyen]`), chọn View/Redirect theo kết quả Service. Truy vấn LINQ và quy tắc nghiệp vụ nằm trong Service.
- Service **không** dùng `TempData`, `ModelState`, `Session`, `View`, `Redirect`. Cần thông tin phiên đăng nhập thì Controller lấy rồi truyền vào tham số (`maTaiKhoan`, `vaiTro`).

## 2. Tên hàm (bắt buộc dùng đúng tên)

Mọi hàm đều `async` và có hậu tố `Async`.

| Việc | Tên hàm | Ghi chú |
| --- | --- | --- |
| Danh sách có từ khóa/bộ lọc | `TimKiemAsync(string? tuKhoa, ...)` | Tham số từ khóa luôn tên `tuKhoa` |
| Danh sách kèm phân trang, thống kê, hoặc không có từ khóa | `LayDanhSachAsync(...)` | |
| Xem chi tiết 1 bản ghi | `LayChiTietAsync(int id)` | Trả `null` nếu không có |
| Lấy dữ liệu đổ vào form sửa | `LayDeSuaAsync(int id)` | Trả `null` nếu không có |
| Thêm | `ThemAsync(model, bool modelHopLe)` | |
| Sửa | `CapNhatAsync(int id, model, bool modelHopLe)` | **Không** dùng `SuaAsync` |
| Xóa | `XoaAsync(int id)` | Chỉ viết nếu nghiệp vụ cho phép xóa |
| Khóa/mở khóa, bật/tắt | `DoiTrangThaiAsync(int id)` | |

Hành động nghiệp vụ riêng (không phải CRUD) đặt tên theo động từ tiếng Việt không dấu, ví dụ `TaoPhieuAsync`, `HuyPhieuAsync`, `CapNhatTrangThaiAsync`.

Thứ tự tham số: `id` trước, `model` sau, `modelHopLe` cuối. Thông tin người dùng hiện tại (`maTaiKhoan`, `vaiTro`) đặt sau `model`, trước `modelHopLe`.

## 3. Kiểu trả về cho Thêm / Sửa / Xóa / Đổi trạng thái

Dùng `KetQuaDichVu` (hoặc `KetQuaDichVu<T>` khi cần trả thêm dữ liệu) trong `Services/KetQuaDichVu.cs`:

| `Loai` | Khi nào | Controller làm gì |
| --- | --- | --- |
| `ThanhCong` | Lưu được | `this.DatThongBao(ketQua)` rồi `RedirectToAction` |
| `KhongTimThay` | Không có bản ghi theo `id` | `return NotFound()` |
| `LoiNghiepVu` | Vi phạm quy tắc nghiệp vụ (đang được dùng, trùng...) | `this.DatThongBao(ketQua)` rồi Redirect |
| `DuLieuKhongHopLe` | `modelHopLe == false` hoặc lỗi theo từng ô | `ketQua.GopLoiVaoModelState(ModelState)` rồi `return View(model)` |

Service tạo kết quả bằng `KetQuaDichVu.Ok(...)`, `.Loi(...)`, `.KhongTimThay()`, `.DuLieuKhongHopLe(...)`. Câu thông báo nằm trong Service, không nằm ở Controller.

Mẫu Controller (POST sửa):

```csharp
var ketQua = await _service.CapNhatAsync(id, model, ModelState.IsValid);
switch (ketQua.Loai)
{
    case LoaiKetQua.KhongTimThay: return NotFound();
    case LoaiKetQua.DuLieuKhongHopLe:
        ketQua.GopLoiVaoModelState(ModelState);
        return View(model);
    default:
        this.DatThongBao(ketQua);
        return RedirectToAction(nameof(Index));
}
```

## 4. Ngoại lệ đã biết

- `IKyThuatVienService.CapNhatAsync` trả `KetQuaDichVu<int>` (`DuLieu` = tài khoản đang gắn với hồ sơ) để Controller nạp lại dropdown khi form lỗi.
- `ILinhKienService` và `IChiTietSuaChuaService`: Controller dùng khóa TempData `"Success"`/`"Error"` (khớp View cũ) nên gọi `DatThongBao(ketQua, "Success", "Error")`.
- `KhachHang`, `KyThuatVien`, `PhieuSuaChua` hiện không có chức năng xóa nên không có `XoaAsync`; theo `THIETKE.md` mục 2.7 thì ngừng sử dụng bằng đổi `TrangThai`, riêng phiếu sửa chữa dùng `HuyPhieuAsync`. Cần thêm xóa thì phải kiểm tra bản ghi tham chiếu bằng `Any()` trước.

## 5. Thêm hàm mới

Cần hàm chưa có trong bảng mục 2: đặt tên theo mẫu động từ + đối tượng, nhắn nhóm trưởng để bổ sung vào bảng này trong cùng Pull Request.
