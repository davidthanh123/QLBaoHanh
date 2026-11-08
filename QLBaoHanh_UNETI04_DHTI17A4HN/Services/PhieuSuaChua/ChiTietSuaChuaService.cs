// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Nghiệp vụ chi tiết sửa chữa và tồn kho linh kiện (tách từ ChiTietSuaChuaController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class ChiTietSuaChuaService : IChiTietSuaChuaService
    {
        private readonly AppDbContext _db;

        public ChiTietSuaChuaService(AppDbContext db) => _db = db;

        // ===================== TRUY VẤN =====================

        public Task<List<ChiTietSuaChua>> LayDanhSachAsync() =>
            _db.ChiTietSuaChuas.AsNoTracking()
                .Include(c => c.KyThuatVien)
                .Include(c => c.LinhKien)
                .Include(c => c.PhieuSuaChua)
                .ToListAsync();

        // Dùng chung cho trang Details và Delete (cùng tập Include).
        public Task<ChiTietSuaChua?> LayChiTietAsync(int id) =>
            _db.ChiTietSuaChuas.AsNoTracking()
                .Include(c => c.PhieuSuaChua)
                .Include(c => c.KyThuatVien)
                .Include(c => c.LinhKien)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

        public Task<ChiTietSuaChua?> LayDeSuaAsync(int id) =>
            _db.ChiTietSuaChuas.AsNoTracking().FirstOrDefaultAsync(c => c.MaChiTiet == id);

        public Task<List<PhieuSuaChua>> LayDsPhieuAsync() =>
            _db.PhieuSuaChuas.AsNoTracking().ToListAsync();

        public Task<List<KyThuatVien>> LayDsKyThuatVienAsync() =>
            _db.KyThuatViens.AsNoTracking().ToListAsync();

        public Task<List<LinhKien>> LayDsLinhKienAsync(LocLinhKien loc, int? maLinhKienDangChon = null)
        {
            var query = _db.LinhKiens.AsNoTracking().AsQueryable();
            query = loc switch
            {
                LocLinhKien.DangHoatDong => query.Where(l => l.TrangThai),
                LocLinhKien.DangHoatDongVaConHang => query.Where(l => l.TrangThai && l.SoLuongTon > 0),
                LocLinhKien.DangHoatDongConHangHoacDangChon => query.Where(l =>
                    l.TrangThai && (l.SoLuongTon > 0 || l.MaLinhKien == maLinhKienDangChon)),
                _ => query
            };
            return query.ToListAsync();
        }

        public Task<List<GiaLinhKien>> LayGiaLinhKienAsync() =>
            _db.LinhKiens.AsNoTracking()
                .Select(l => new GiaLinhKien(l.MaLinhKien, l.DonGia))
                .ToListAsync();

        // ===================== THÊM =====================

        public async Task<KetQuaDichVu> ThemAsync(ChiTietSuaChua model, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();

            // Kiểm tra linh kiện nếu có sử dụng
            LinhKien? linhKien = null;
            if (model.MaLinhKien.HasValue)
            {
                linhKien = await _db.LinhKiens
                    .FirstOrDefaultAsync(l => l.MaLinhKien == model.MaLinhKien.Value);

                if (linhKien == null)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện không tồn tại."));
                }
                else
                {
                    if (!linhKien.TrangThai)
                        loi.Add(new(nameof(model.MaLinhKien), "Linh kiện đã ngừng hoạt động."));

                    if (model.SoLuong > linhKien.SoLuongTon)
                        loi.Add(new(nameof(model.SoLuong), "Số lượng sử dụng vượt quá số lượng tồn kho."));
                }
            }

            // Lấy đơn giá từ dữ liệu linh kiện, không tin giá gửi từ form
            if (linhKien != null)
                model.DonGia = linhKien.DonGia;

            // Tự tính thành tiền
            model.ThanhTien = model.SoLuong * model.DonGia;

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            if (linhKien != null)
                linhKien.SoLuongTon -= model.SoLuong;

            _db.ChiTietSuaChuas.Add(model);
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Thêm chi tiết sửa chữa thành công.");
        }

        // ===================== SỬA =====================

        public async Task<KetQuaDichVu> CapNhatAsync(int id, ChiTietSuaChua model, bool modelHopLe)
        {
            // Lấy chi tiết cũ từ database
            var chiTietCu = await _db.ChiTietSuaChuas.FirstOrDefaultAsync(c => c.MaChiTiet == id);
            if (chiTietCu == null) return KetQuaDichVu.KhongTimThay();

            var loi = new List<LoiTruongNhap>();

            // Lấy linh kiện mới nếu có sử dụng
            LinhKien? linhKienMoi = null;
            if (model.MaLinhKien.HasValue)
            {
                linhKienMoi = await _db.LinhKiens
                    .FirstOrDefaultAsync(l => l.MaLinhKien == model.MaLinhKien.Value);

                if (linhKienMoi == null)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện không tồn tại."));
                }
                else if (!linhKienMoi.TrangThai)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện đã ngừng hoạt động."));
                }
                else
                {
                    // Nếu vẫn dùng linh kiện cũ, cộng lại số lượng cũ để tính lượng tồn có thể sử dụng.
                    int tonCoTheDung = linhKienMoi.SoLuongTon;
                    if (chiTietCu.MaLinhKien == linhKienMoi.MaLinhKien)
                        tonCoTheDung += chiTietCu.SoLuong;

                    if (model.SoLuong > tonCoTheDung)
                        loi.Add(new(nameof(model.SoLuong), "Số lượng sử dụng vượt quá tồn kho."));
                }
            }

            // Lấy đơn giá chính thức từ linh kiện
            if (model.MaLinhKien.HasValue)
            {
                if (linhKienMoi != null)
                    model.DonGia = linhKienMoi.DonGia;
            }
            else
            {
                model.DonGia = 0;
            }

            // Tự động tính thành tiền
            model.ThanhTien = model.SoLuong * model.DonGia;

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            // Hoàn lại số lượng của linh kiện cũ
            if (chiTietCu.MaLinhKien.HasValue)
            {
                var linhKienCu = await _db.LinhKiens
                    .FirstOrDefaultAsync(l => l.MaLinhKien == chiTietCu.MaLinhKien.Value);
                if (linhKienCu != null)
                    linhKienCu.SoLuongTon += chiTietCu.SoLuong;
            }

            // Trừ số lượng linh kiện mới
            if (linhKienMoi != null)
                linhKienMoi.SoLuongTon -= model.SoLuong;

            // Cập nhật bản ghi đang được EF Core theo dõi
            chiTietCu.MaPhieu = model.MaPhieu;
            chiTietCu.MaKyThuatVien = model.MaKyThuatVien;
            chiTietCu.MaLinhKien = model.MaLinhKien;
            chiTietCu.SoLuong = model.SoLuong;
            chiTietCu.DonGia = model.DonGia;
            chiTietCu.ThanhTien = model.ThanhTien;
            chiTietCu.KetQuaChanDoan = model.KetQuaChanDoan;
            chiTietCu.GhiChu = model.GhiChu;

            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Cập nhật chi tiết sửa chữa thành công.");
        }

        // ===================== XÓA =====================

        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var chiTiet = await _db.ChiTietSuaChuas.FirstOrDefaultAsync(c => c.MaChiTiet == id);
            if (chiTiet == null) return KetQuaDichVu.KhongTimThay();

            // Hoàn lại số lượng linh kiện vào kho
            if (chiTiet.MaLinhKien.HasValue)
            {
                var linhKien = await _db.LinhKiens
                    .FirstOrDefaultAsync(l => l.MaLinhKien == chiTiet.MaLinhKien.Value);
                if (linhKien != null)
                    linhKien.SoLuongTon += chiTiet.SoLuong;
            }

            _db.ChiTietSuaChuas.Remove(chiTiet);

            // Lưu cả thay đổi tồn kho và thao tác xóa
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Xóa chi tiết sửa chữa và hoàn lại linh kiện thành công.");
        }
    }
}
