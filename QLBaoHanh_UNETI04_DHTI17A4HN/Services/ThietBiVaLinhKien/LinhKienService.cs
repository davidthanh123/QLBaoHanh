// Họ và tên: Nguyễn Văn Vũ
// Mã sinh viên: Điền mã sinh viên của bạn
// Nội dung thực hiện: Module 5 - nghiệp vụ Quản lý linh kiện (tách từ LinhKienController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class LinhKienService : ILinhKienService
    {
        private const string LoiTrungTen = "Tên linh kiện đã tồn tại.";
        private readonly AppDbContext _db;

        public LinhKienService(AppDbContext db) => _db = db;

        public async Task<List<LinhKien>> TimKiemAsync(string? tuKhoa, string? trangThai, string? sapXep)
        {
            var query = _db.LinhKiens.AsNoTracking().AsQueryable();

            // Tìm theo tên linh kiện
            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(l => l.TenLinhKien.Contains(tuKhoa));

            // Lọc trạng thái
            if (trangThai == "hoatdong")
                query = query.Where(l => l.TrangThai);
            else if (trangThai == "ngung")
                query = query.Where(l => !l.TrangThai);

            // Sắp xếp
            query = sapXep switch
            {
                "gia_tang" => query.OrderBy(l => l.DonGia),
                "gia_giam" => query.OrderByDescending(l => l.DonGia),
                "ton_tang" => query.OrderBy(l => l.SoLuongTon),
                "ton_giam" => query.OrderByDescending(l => l.SoLuongTon),
                _ => query.OrderBy(l => l.MaLinhKien)
            };

            return await query.ToListAsync();
        }

        public Task<LinhKien?> LayChiTietAsync(int id) =>
            _db.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);

        public async Task<KetQuaDichVu> ThemAsync(LinhKien linhKien, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();
            if (await _db.LinhKiens.AnyAsync(l => l.TenLinhKien == linhKien.TenLinhKien))
                loi.Add(new(nameof(linhKien.TenLinhKien), LoiTrungTen));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            _db.LinhKiens.Add(linhKien);
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Thêm linh kiện thành công.");
        }

        public async Task<KetQuaDichVu> CapNhatAsync(int id, LinhKien linhKien, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();
            if (await _db.LinhKiens.AnyAsync(l => l.MaLinhKien != id && l.TenLinhKien == linhKien.TenLinhKien))
                loi.Add(new(nameof(linhKien.TenLinhKien), LoiTrungTen));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            // Lấy entity đang được theo dõi để tránh ghi đè
            // các thuộc tính điều hướng do form gửi lên.
            var entity = await _db.LinhKiens.FindAsync(id);
            if (entity == null) return KetQuaDichVu.KhongTimThay();

            entity.TenLinhKien = linhKien.TenLinhKien;
            entity.DonViTinh = linhKien.DonViTinh;
            entity.DonGia = linhKien.DonGia;
            entity.SoLuongTon = linhKien.SoLuongTon;
            entity.MoTa = linhKien.MoTa;
            entity.TrangThai = linhKien.TrangThai;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(string.Empty, "Không thể lưu thay đổi. Hãy kiểm tra dữ liệu.")
                });
            }

            return KetQuaDichVu.Ok("Cập nhật linh kiện thành công.");
        }

        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var linhKien = await _db.LinhKiens.FirstOrDefaultAsync(l => l.MaLinhKien == id);
            if (linhKien == null) return KetQuaDichVu.KhongTimThay();

            // Kiểm tra linh kiện đã được sử dụng chưa
            if (await _db.ChiTietSuaChuas.AnyAsync(c => c.MaLinhKien == id))
                return KetQuaDichVu.Loi("Không thể xóa linh kiện đã được sử dụng trong phiếu sửa chữa.");

            _db.LinhKiens.Remove(linhKien);
            try
            {
                await _db.SaveChangesAsync();
                return KetQuaDichVu.Ok("Xóa linh kiện thành công.");
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.Loi("Không thể xóa linh kiện do dữ liệu đang được tham chiếu.");
            }
        }
    }
}
