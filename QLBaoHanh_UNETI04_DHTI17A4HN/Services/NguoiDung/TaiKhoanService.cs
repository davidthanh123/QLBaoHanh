// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - nghiệp vụ CRUD Tài khoản (Admin): thêm, sửa, khóa/mở khóa, xóa an toàn (tách từ TaiKhoanController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private const string LoiTrungTenDangNhap = "Tên đăng nhập đã tồn tại";
        private const string LoiAdminCuoiCung = "Phải còn ít nhất một quản trị viên đang hoạt động.";

        private readonly AppDbContext _db;
        public TaiKhoanService(AppDbContext db) => _db = db;

        public async Task<List<TaiKhoan>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai)
        {
            var query = _db.TaiKhoans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(k) || t.HoTen.Contains(k) || t.Email.Contains(k));
            }
            if (vaiTro.HasValue)
                query = query.Where(t => t.VaiTro == vaiTro.Value);
            if (trangThai.HasValue)
                query = query.Where(t => t.TrangThai == trangThai.Value);

            return await query.OrderBy(t => t.TenDangNhap).ToListAsync();
        }

        public async Task<ChiTietTaiKhoan?> LayChiTietAsync(int id)
        {
            var taiKhoan = await _db.TaiKhoans.AsNoTracking()
                .Include(t => t.KhachHang)
                .Include(t => t.KyThuatVien)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return null;

            var nhatKy = await _db.NhatKyDangNhaps.AsNoTracking()
                .Where(n => n.MaTaiKhoan == id)
                .OrderByDescending(n => n.ThoiGian)
                .Take(10)
                .ToListAsync();
            return new ChiTietTaiKhoan(taiKhoan, nhatKy);
        }

        public async Task<TaiKhoanFormViewModel?> LayDeSuaAsync(int id)
        {
            var taiKhoan = await _db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return null;

            return new TaiKhoanFormViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai,
            };
        }

        public async Task<KetQuaDichVu> ThemAsync(TaiKhoanFormViewModel model, bool modelHopLe)
        {
            model.TenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
            model.HoTen = (model.HoTen ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty).Trim();

            var loi = new List<LoiTruongNhap>();
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                loi.Add(new(nameof(model.MatKhau), "Mật khẩu không được để trống"));

            // Chỉ kiểm tra trùng tên khi các lỗi nhập liệu khác đã sạch (giữ đúng hành vi cũ).
            if (modelHopLe && loi.Count == 0 && await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                loi.Add(new(nameof(model.TenDangNhap), LoiTrungTenDangNhap));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = model.VaiTro,
                TrangThai = model.TrangThai,
                NgayTaoTaiKhoan = DateTime.Now,
            };
            taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, model.MatKhau!);

            _db.TaiKhoans.Add(taiKhoan);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Hai Admin cùng tạo một tên đăng nhập: unique index chặn lại
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.TenDangNhap), LoiTrungTenDangNhap) });
            }

            return KetQuaDichVu.Ok($"Đã thêm tài khoản {taiKhoan.TenDangNhap}.");
        }

        public async Task<KetQuaDichVu> CapNhatAsync(int id, TaiKhoanFormViewModel model, int? maDangNhap, bool modelHopLe)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return KetQuaDichVu.KhongTimThay();

            var laChinhMinh = taiKhoan.MaTaiKhoan == maDangNhap;

            // Tên đăng nhập không đổi khi sửa: bỏ qua giá trị gửi lên
            model.TenDangNhap = taiKhoan.TenDangNhap;
            model.HoTen = (model.HoTen ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty).Trim();

            var loi = new List<LoiTruongNhap>();
            if (modelHopLe)
            {
                var doiVaiTroHoacKhoa = model.VaiTro != taiKhoan.VaiTro || model.TrangThai != taiKhoan.TrangThai;
                if (laChinhMinh && doiVaiTroHoacKhoa)
                {
                    loi.Add(new(string.Empty, "Không thể tự đổi vai trò hoặc tự khóa tài khoản đang đăng nhập."));
                }
                else if (taiKhoan.VaiTro == VaiTroEnum.Admin && taiKhoan.TrangThai
                         && (model.VaiTro != VaiTroEnum.Admin || !model.TrangThai)
                         && await LaAdminHoatDongCuoiCungAsync(id))
                {
                    loi.Add(new(string.Empty, LoiAdminCuoiCung));
                }
            }

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            taiKhoan.HoTen = model.HoTen;
            taiKhoan.Email = model.Email;
            taiKhoan.VaiTro = model.VaiTro;
            if (model.TrangThai && !taiKhoan.TrangThai)
            {
                // Mở khóa: xóa luôn dấu vết khóa tạm
                taiKhoan.SoLanDangNhapSai = 0;
                taiKhoan.KhoaDenNgay = null;
            }
            taiKhoan.TrangThai = model.TrangThai;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, model.MatKhau);

            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã cập nhật tài khoản {taiKhoan.TenDangNhap}.");
        }

        // Khóa/mở khóa (đảo TrangThai)
        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id, int? maDangNhap)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return KetQuaDichVu.KhongTimThay();

            if (taiKhoan.MaTaiKhoan == maDangNhap)
                return KetQuaDichVu.Loi("Không thể tự khóa tài khoản đang đăng nhập.");

            string thongBao;
            if (taiKhoan.TrangThai)
            {
                if (taiKhoan.VaiTro == VaiTroEnum.Admin && await LaAdminHoatDongCuoiCungAsync(id))
                    return KetQuaDichVu.Loi(LoiAdminCuoiCung);

                taiKhoan.TrangThai = false;
                thongBao = $"Đã khóa tài khoản {taiKhoan.TenDangNhap}.";
            }
            else
            {
                taiKhoan.TrangThai = true;
                taiKhoan.SoLanDangNhapSai = 0;
                taiKhoan.KhoaDenNgay = null;
                thongBao = $"Đã mở khóa tài khoản {taiKhoan.TenDangNhap}.";
            }

            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok(thongBao);
        }

        // Xóa an toàn: chỉ xóa tài khoản chưa phát sinh dữ liệu; còn lại hướng dẫn dùng "Khóa"
        public async Task<KetQuaDichVu> XoaAsync(int id, int? maDangNhap)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return KetQuaDichVu.KhongTimThay();

            if (taiKhoan.MaTaiKhoan == maDangNhap)
                return KetQuaDichVu.Loi("Không thể xóa tài khoản đang đăng nhập.");
            if (taiKhoan.VaiTro == VaiTroEnum.Admin && taiKhoan.TrangThai && await LaAdminHoatDongCuoiCungAsync(id))
                return KetQuaDichVu.Loi(LoiAdminCuoiCung);

            var coDuLieuLienQuan =
                await _db.KhachHangs.AnyAsync(k => k.MaTaiKhoan == id)
                || await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == id)
                || await _db.PhieuSuaChuas.AnyAsync(p => p.MaTaiKhoanTiepNhan == id)
                || await _db.LichSuTrangThaiPhieus.AnyAsync(l => l.MaTaiKhoanThucHien == id)
                || await _db.TepDinhKems.AnyAsync(t => t.MaTaiKhoanUpload == id)
                || await _db.ThongBaos.AnyAsync(t => t.MaTaiKhoanNhan == id)
                || await _db.NhatKyDangNhaps.AnyAsync(n => n.MaTaiKhoan == id);

            if (coDuLieuLienQuan)
                return KetQuaDichVu.Loi($"Không thể xóa tài khoản {taiKhoan.TenDangNhap}: đã có dữ liệu liên quan (hồ sơ, phiếu hoặc nhật ký đăng nhập). Hãy dùng chức năng Khóa tài khoản.");

            _db.TaiKhoans.Remove(taiKhoan);
            try
            {
                await _db.SaveChangesAsync();
                return KetQuaDichVu.Ok($"Đã xóa tài khoản {taiKhoan.TenDangNhap}.");
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.Loi("Không thể xóa tài khoản này vì đã có dữ liệu liên quan. Hãy dùng chức năng Khóa tài khoản.");
            }
        }

        // true nếu ngoài tài khoản `id` không còn Admin nào đang hoạt động
        private async Task<bool> LaAdminHoatDongCuoiCungAsync(int id) =>
            !await _db.TaiKhoans.AnyAsync(t => t.MaTaiKhoan != id && t.VaiTro == VaiTroEnum.Admin && t.TrangThai);
    }
}
