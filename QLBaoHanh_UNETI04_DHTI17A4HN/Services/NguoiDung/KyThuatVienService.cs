// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - quản lý hồ sơ kỹ thuật viên (tách từ KyThuatVienController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class KyThuatVienService : IKyThuatVienService
    {
        private readonly AppDbContext _db;
        public KyThuatVienService(AppDbContext db) => _db = db;

        public async Task<List<KyThuatVien>> TimKiemAsync(string? tuKhoa, bool? trangThai)
        {
            var query = _db.KyThuatViens.AsNoTracking().Include(k => k.TaiKhoan).AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaSach = tuKhoa.Trim();
                query = query.Where(k => k.HoTen.Contains(tuKhoaSach) ||
                    (k.ChuyenMon != null && k.ChuyenMon.Contains(tuKhoaSach)) ||
                    (k.TaiKhoan != null && k.TaiKhoan.TenDangNhap.Contains(tuKhoaSach)));
            }
            if (trangThai.HasValue) query = query.Where(k => k.TrangThai == trangThai.Value);
            return await query.OrderBy(k => k.HoTen).ToListAsync();
        }

        public async Task<List<TaiKhoan>> LayTaiKhoanNhanVienAsync(int? taiKhoanHienTai = null)
        {
            return await _db.TaiKhoans.AsNoTracking()
                .Where(t => t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai &&
                    (t.KyThuatVien == null || t.MaTaiKhoan == taiKhoanHienTai))
                .OrderBy(t => t.HoTen).ToListAsync();
        }

        public async Task<KyThuatVienViewModel?> LayDeSuaAsync(int id)
        {
            var ktv = await _db.KyThuatViens.AsNoTracking().FirstOrDefaultAsync(k => k.MaKyThuatVien == id);
            if (ktv == null) return null;

            return new KyThuatVienViewModel
            {
                MaKyThuatVien = ktv.MaKyThuatVien,
                MaTaiKhoan = ktv.MaTaiKhoan,
                HoTen = ktv.HoTen,
                ChuyenMon = ktv.ChuyenMon,
                SoDienThoai = ktv.SoDienThoai,
                Email = ktv.Email,
                TrangThai = ktv.TrangThai
            };
        }

        public async Task<KetQuaDichVu> ThemAsync(KyThuatVienViewModel model, bool modelHopLe)
        {
            if (!modelHopLe)
                return KetQuaDichVu.DuLieuKhongHopLe();

            var taiKhoanHopLe = await _db.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan == model.MaTaiKhoan && t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai);
            var daCoHoSo = await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == model.MaTaiKhoan);
            if (!taiKhoanHopLe || daCoHoSo)
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(nameof(model.MaTaiKhoan),
                        "Tài khoản phải là nhân viên hoạt động và chưa có hồ sơ kỹ thuật viên.")
                });

            _db.KyThuatViens.Add(new KyThuatVien
            {
                MaTaiKhoan = model.MaTaiKhoan,
                HoTen = model.HoTen.Trim(),
                ChuyenMon = model.ChuyenMon?.Trim(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                Email = model.Email?.Trim(),
                TrangThai = true
            });
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã tạo hồ sơ kỹ thuật viên.");
        }

        public async Task<KetQuaDichVu<int>> CapNhatAsync(int id, KyThuatVienViewModel model, bool modelHopLe)
        {
            var ktv = await _db.KyThuatViens.FindAsync(id);
            if (ktv == null) return KetQuaDichVu.KhongTimThay().Kem(0);
            var taiKhoanCu = ktv.MaTaiKhoan;

            if (!modelHopLe)
                return KetQuaDichVu.DuLieuKhongHopLe().Kem(taiKhoanCu);

            var taiKhoanHopLe = await _db.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan == model.MaTaiKhoan && t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai);
            var taiKhoanDaGan = await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == model.MaTaiKhoan && k.MaKyThuatVien != id);
            if (!taiKhoanHopLe || taiKhoanDaGan)
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(nameof(model.MaTaiKhoan),
                        "Tài khoản phải là nhân viên hoạt động và chưa được gán hồ sơ khác.")
                }).Kem(taiKhoanCu);

            ktv.MaTaiKhoan = model.MaTaiKhoan;
            ktv.HoTen = model.HoTen.Trim();
            ktv.ChuyenMon = model.ChuyenMon?.Trim();
            ktv.SoDienThoai = model.SoDienThoai?.Trim();
            ktv.Email = model.Email?.Trim();
            ktv.TrangThai = model.TrangThai;
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã cập nhật hồ sơ kỹ thuật viên.").Kem(taiKhoanCu);
        }
    }
}
