// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - hợp đồng (interface) của nghiệp vụ phiếu sửa chữa.

using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    // Service KHÔNG đụng tới HttpContext/Session/TempData/ViewBag.
    // Controller lấy maTaiKhoan, vaiTro từ Session rồi truyền vào.
    public interface IPhieuSuaChuaService
    {
        Task<DanhSachPhieuKetQua> LayDanhSachAsync(PhieuSuaChuaBoLoc boLoc, int? maTaiKhoan, VaiTroEnum? vaiTro);
        Task<ChiTietPhieuKetQua?> LayChiTietAsync(int maPhieu);

        Task<KetQuaDichVu> ThemChiTietSuaChuaAsync(int maPhieu, ThemChiTietSuaChuaViewModel model);
        Task<KetQuaDichVu> CapNhatTrangThaiAsync(int maPhieu, TrangThaiPhieuEnum trangThaiMoi, string? ghiChu, int maTaiKhoan);

        Task<int?> LayMaKhachHangHienTaiAsync(int? maTaiKhoan);
        Task<List<SelectListItem>> LayThietBiCoTheSuaAsync(int maKhachHang);
        Task<KetQuaDichVu> TaoPhieuAsync(int maKhachHang, TaoPhieuSuaChuaViewModel model, bool modelHopLe);

        Task<KetQuaDichVu> KiemTraCoTheHuyAsync(int maPhieu, int? maTaiKhoan, VaiTroEnum? vaiTro);
        Task<KetQuaDichVu> HuyPhieuAsync(HuyPhieuViewModel model, int? maTaiKhoan, VaiTroEnum? vaiTro, bool modelHopLe);
    }
}
