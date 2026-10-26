// Họ và tên: Nguyễn Văn Vũ
// Mã sinh viên: Điền mã sinh viên của bạn
// Nội dung thực hiện: Module 5 - nghiệp vụ Quản lý linh kiện (tách từ LinhKienController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public interface ILinhKienService
    {
        Task<List<LinhKien>> TimKiemAsync(string? searchString, string? trangThai, string? sapXep);
        Task<LinhKien?> LayChiTietAsync(int id);

        Task<KetQuaDichVu> ThemAsync(LinhKien linhKien, bool modelHopLe);
        Task<KetQuaDichVu> CapNhatAsync(int id, LinhKien linhKien, bool modelHopLe);
        Task<KetQuaDichVu> XoaAsync(int id);
    }
}
