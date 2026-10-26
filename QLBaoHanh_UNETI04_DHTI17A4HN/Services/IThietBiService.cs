// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 2 - nghiệp vụ Thiết bị (tách từ ThietBiController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    /// <summary>Dữ liệu cho 2 dropdown "Loại thiết bị" và "Khách hàng" ở form thêm/sửa.</summary>
    public record DanhSachChonThietBi(List<LoaiThietBi> DsLoai, List<KhachHang> DsKhachHang);

    public interface IThietBiService
    {
        // Khách hàng chỉ thấy thiết bị của mình; Admin/Nhân viên thấy toàn bộ.
        Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiFilter f, int? maTaiKhoan, VaiTroEnum? vaiTro);

        /// <summary>Trả về null (-> NotFound) nếu không có, hoặc khách hàng xem thiết bị của người khác.</summary>
        Task<ThietBi?> LayChiTietAsync(int id, int? maTaiKhoan, VaiTroEnum? vaiTro);

        Task<ThietBi?> LayDeSuaAsync(int id);
        Task<DanhSachChonThietBi> LayDanhSachChonAsync(int? maLoaiHienTai = null, int? maKhachHangHienTai = null);

        Task<KetQuaDichVu> ThemAsync(ThietBi model, bool modelHopLe);

        // DuLieu = (MaLoai, MaKhachHang) của bản ghi để Controller nạp lại dropdown khi form lỗi.
        Task<KetQuaDichVu<(int MaLoai, int MaKhachHang)>> CapNhatAsync(int id, ThietBi model, bool modelHopLe);

        Task<KetQuaDichVu> DoiTrangThaiAsync(int id, TrangThaiThietBiEnum trangThaiMoi);
        Task<KetQuaDichVu> XoaAsync(int id);
    }
}
