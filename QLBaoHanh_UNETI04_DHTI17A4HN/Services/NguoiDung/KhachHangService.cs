// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Quản lý khách hàng và hồ sơ cá nhân (tách từ KhachHangController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AppDbContext _db;
        public KhachHangService(AppDbContext db) => _db = db;

        public async Task<List<KhachHang>> TimKiemAsync(string? tuKhoa, bool? trangThai)
        {
            var query = _db.KhachHangs
                .AsNoTracking()
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaSach = tuKhoa.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoaSach) ||
                    k.SoDienThoai.Contains(tuKhoaSach) ||
                    (k.Email != null && k.Email.Contains(tuKhoaSach)) ||
                    (k.CCCD != null && k.CCCD.Contains(tuKhoaSach)));
            }

            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            return await query.OrderBy(k => k.HoTen).ToListAsync();
        }

        public async Task<HoSoKhachHangViewModel?> LayHoSoAsync(int maTaiKhoan)
        {
            var kh = await _db.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);
            if (kh == null) return null;

            return new HoSoKhachHangViewModel
            {
                MaKhachHang = kh.MaKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                CCCD = kh.CCCD,
                LoaiKhachHang = kh.LoaiKhachHang,
                MaSoThue = kh.MaSoThue
            };
        }

        public async Task<KetQuaDichVu<string>> CapNhatHoSoAsync(
            int maTaiKhoan, HoSoKhachHangViewModel model, bool modelHopLe)
        {
            var kh = await _db.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

            if (kh == null)
                return KetQuaDichVu.Loi("Không tìm thấy hồ sơ khách hàng gắn với tài khoản hiện tại.").Kem("");

            var loi = new List<LoiTruongNhap>();

            // Sửa trực tiếp model (như code cũ) để View hiển thị lại đúng khi form bị lỗi.
            if (model.LoaiKhachHang != LoaiKhachHangEnum.DoanhNghiep)
                model.MaSoThue = null;
            if (model.LoaiKhachHang == LoaiKhachHangEnum.DoanhNghiep &&
                string.IsNullOrWhiteSpace(model.MaSoThue))
                loi.Add(new(nameof(model.MaSoThue), "Khách hàng doanh nghiệp cần có mã số thuế."));

            var cccd = string.IsNullOrWhiteSpace(model.CCCD) ? null : model.CCCD.Trim();
            if (cccd != null && await _db.KhachHangs.AnyAsync(k => k.CCCD == cccd && k.MaKhachHang != kh.MaKhachHang))
                loi.Add(new(nameof(model.CCCD), "CCCD này đã được sử dụng."));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi).Kem("");

            kh.HoTen = model.HoTen.Trim();
            kh.SoDienThoai = model.SoDienThoai.Trim();
            kh.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            kh.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();
            kh.CCCD = cccd;
            kh.LoaiKhachHang = model.LoaiKhachHang;
            kh.MaSoThue = string.IsNullOrWhiteSpace(model.MaSoThue) ? null : model.MaSoThue.Trim();

            // Đồng bộ tên/email hiển thị của tài khoản đăng nhập.
            if (kh.TaiKhoan != null)
            {
                kh.TaiKhoan.HoTen = kh.HoTen;
                if (!string.IsNullOrWhiteSpace(kh.Email))
                    kh.TaiKhoan.Email = kh.Email;
            }

            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã cập nhật hồ sơ cá nhân.").Kem(kh.HoTen);
        }
    }
}
