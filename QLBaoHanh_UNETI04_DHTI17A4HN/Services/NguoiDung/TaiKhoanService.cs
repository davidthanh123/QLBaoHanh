// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - nghiệp vụ CRUD Tài khoản (Admin): thêm, sửa, khóa/mở khóa, xóa an toàn (tách từ TaiKhoanController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private const string LoiTrungTenDangNhap = "Tên đăng nhập đã tồn tại";
        private const string LoiAdminCuoiCung = "Phải còn ít nhất một quản trị viên đang hoạt động.";

        private readonly IUnitOfWork _uow;
        public TaiKhoanService(IUnitOfWork uow) => _uow = uow;

        public Task<List<TaiKhoan>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai) =>
            _uow.TaiKhoans.TimKiemAsync(tuKhoa, vaiTro, trangThai);

        public async Task<ChiTietTaiKhoan?> LayChiTietAsync(int id)
        {
            var taiKhoan = await _uow.TaiKhoans.LayChiTietKemHoSoAsync(id);
            if (taiKhoan == null) return null;

            var nhatKy = await _uow.NhatKyDangNhaps.LayTopTheoTaiKhoanAsync(id, 10);
            return new ChiTietTaiKhoan(taiKhoan, nhatKy);
        }

        public async Task<TaiKhoanFormViewModel?> LayDeSuaAsync(int id)
        {
            var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(id);
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
            if (modelHopLe && loi.Count == 0 && await _uow.TaiKhoans.KiemTraTrungTenDangNhapAsync(model.TenDangNhap))
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

            await _uow.TaiKhoans.AddAsync(taiKhoan);
            try
            {
                await _uow.SaveChangesAsync();
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
            var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(id);
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
                         && await _uow.TaiKhoans.LaAdminHoatDongCuoiCungAsync(id))
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

            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã cập nhật tài khoản {taiKhoan.TenDangNhap}.");
        }

        // Khóa/mở khóa (đảo TrangThai)
        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id, int? maDangNhap)
        {
            var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(id);
            if (taiKhoan == null) return KetQuaDichVu.KhongTimThay();

            if (taiKhoan.MaTaiKhoan == maDangNhap)
                return KetQuaDichVu.Loi("Không thể tự khóa tài khoản đang đăng nhập.");

            string thongBao;
            if (taiKhoan.TrangThai)
            {
                if (taiKhoan.VaiTro == VaiTroEnum.Admin && await _uow.TaiKhoans.LaAdminHoatDongCuoiCungAsync(id))
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

            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok(thongBao);
        }

        // Xóa an toàn: chỉ xóa tài khoản chưa phát sinh dữ liệu; còn lại hướng dẫn dùng "Khóa"
        public async Task<KetQuaDichVu> XoaAsync(int id, int? maDangNhap)
        {
            var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(id);
            if (taiKhoan == null) return KetQuaDichVu.KhongTimThay();

            if (taiKhoan.MaTaiKhoan == maDangNhap)
                return KetQuaDichVu.Loi("Không thể xóa tài khoản đang đăng nhập.");
            if (taiKhoan.VaiTro == VaiTroEnum.Admin && taiKhoan.TrangThai && await _uow.TaiKhoans.LaAdminHoatDongCuoiCungAsync(id))
                return KetQuaDichVu.Loi(LoiAdminCuoiCung);

            var coDuLieuLienQuan = await _uow.TaiKhoans.CoDuLieuLienQuanAsync(id);

            if (coDuLieuLienQuan)
                return KetQuaDichVu.Loi($"Không thể xóa tài khoản {taiKhoan.TenDangNhap}: đã có dữ liệu liên quan (hồ sơ, phiếu hoặc nhật ký đăng nhập). Hãy dùng chức năng Khóa tài khoản.");

            _uow.TaiKhoans.Remove(taiKhoan);
            try
            {
                await _uow.SaveChangesAsync();
                return KetQuaDichVu.Ok($"Đã xóa tài khoản {taiKhoan.TenDangNhap}.");
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.Loi("Không thể xóa tài khoản này vì đã có dữ liệu liên quan. Hãy dùng chức năng Khóa tài khoản.");
            }
        }
    }
}
