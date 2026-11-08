// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Nghiệp vụ chi tiết sửa chữa và tồn kho linh kiện (tách từ ChiTietSuaChuaController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    /// <summary>Các kiểu lọc danh sách linh kiện cho dropdown (mỗi màn hình dùng một kiểu).</summary>
    public enum LocLinhKien
    {
        TatCa,
        DangHoatDong,
        DangHoatDongVaConHang,
        DangHoatDongConHangHoacDangChon
    }

    public record GiaLinhKien(int MaLinhKien, decimal DonGia);

    public interface IChiTietSuaChuaService
    {
        Task<List<ChiTietSuaChua>> LayDanhSachAsync();
        Task<ChiTietSuaChua?> LayChiTietAsync(int id);
        Task<ChiTietSuaChua?> LayDeSuaAsync(int id);

        // Dữ liệu cho các dropdown; Controller tự bọc thành SelectList.
        Task<List<PhieuSuaChua>> LayDsPhieuAsync();
        Task<List<KyThuatVien>> LayDsKyThuatVienAsync();
        Task<List<LinhKien>> LayDsLinhKienAsync(LocLinhKien loc, int? maLinhKienDangChon = null);
        Task<List<GiaLinhKien>> LayGiaLinhKienAsync();

        // modelHopLe = ModelState.IsValid của Controller. Service sẽ sửa DonGia/ThanhTien trên model (như code cũ).
        Task<KetQuaDichVu> ThemAsync(ChiTietSuaChua model, bool modelHopLe);
        Task<KetQuaDichVu> CapNhatAsync(int id, ChiTietSuaChua model, bool modelHopLe);
        Task<KetQuaDichVu> XoaAsync(int id);
    }
}
