// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - nghiệp vụ quản lý khách hàng (Admin/NV thêm, sửa, xóa, đổi trạng thái) và hồ sơ cá nhân của khách hàng.

using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class KhachHangService : IKhachHangService
    {
        private const int KichThuocTrang = 10;
        private readonly IUnitOfWork _uow;
        public KhachHangService(IUnitOfWork uow) => _uow = uow;

        // ===================== DANH SÁCH / CHI TIẾT =====================

        public async Task<KhachHangDanhSachViewModel> LayDanhSachAsync(
            string? tuKhoa, bool? trangThai, LoaiKhachHangEnum? loai, int trang)
        {
            var kq = await _uow.KhachHangs.TimKiemPhanTrangAsync(tuKhoa, trangThai, loai, trang, KichThuocTrang);
            return new KhachHangDanhSachViewModel
            {
                TuKhoa = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim(),
                TrangThai = trangThai,
                Loai = loai,
                Trang = kq.TrangHienTai,
                SoTrang = kq.SoTrang,
                TongSo = kq.TongSo,
                KhachHangs = kq.DanhSach
            };
        }

        public Task<KhachHang?> LayChiTietAsync(int maKhachHang) =>
            _uow.KhachHangs.LayChiTietAsync(maKhachHang);

        // ===================== FORM THÊM / SỬA =====================

        public async Task<KhachHangFormViewModel> TaoFormMoiAsync()
        {
            var model = new KhachHangFormViewModel();
            await NapLaiTaiKhoanAsync(model);
            return model;
        }

        public async Task<KhachHangFormViewModel?> LayFormSuaAsync(int maKhachHang)
        {
            var kh = await _uow.KhachHangs.GetByIdAsync(maKhachHang);
            if (kh == null) return null;

            var model = new KhachHangFormViewModel
            {
                MaKhachHang = kh.MaKhachHang,
                MaTaiKhoan = kh.MaTaiKhoan,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                CCCD = kh.CCCD,
                LoaiKhachHang = kh.LoaiKhachHang,
                MaSoThue = kh.MaSoThue,
                GhiChu = kh.GhiChu
            };
            await NapLaiTaiKhoanAsync(model);
            return model;
        }

        public async Task NapLaiTaiKhoanAsync(KhachHangFormViewModel model)
        {
            var dsTaiKhoan = await _uow.KhachHangs.LayTaiKhoanKhachChuaLienKetAsync(model.MaTaiKhoan);
            model.TaiKhoanKhaDung = dsTaiKhoan
                .Select(t => new SelectListItem
                {
                    Value = t.MaTaiKhoan.ToString(),
                    Text = $"{t.TenDangNhap} — {t.HoTen}",
                    Selected = t.MaTaiKhoan == model.MaTaiKhoan
                }).ToList();
        }

        // ===================== THÊM / SỬA / ĐỔI TRẠNG THÁI / XÓA =====================

        public async Task<KetQuaDichVu> TaoAsync(KhachHangFormViewModel model, bool modelHopLe)
        {
            var loi = await KiemTraNghiepVuAsync(model, maKhachHangHienTai: 0);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            var kh = new KhachHang
            {
                MaTaiKhoan = model.MaTaiKhoan,
                NgayTao = DateTime.Now,   // thời điểm tạo do hệ thống xác định
                TrangThai = true
            };
            GanDuLieuTuForm(kh, model);

            await _uow.KhachHangs.AddAsync(kh);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã thêm khách hàng {kh.HoTen}.");
        }

        public async Task<KetQuaDichVu> CapNhatAsync(int maKhachHang, KhachHangFormViewModel model, bool modelHopLe)
        {
            var kh = await _uow.KhachHangs.LayTheoMaKemTaiKhoanAsync(maKhachHang);
            if (kh == null) return KetQuaDichVu.KhongTimThay();

            var loi = await KiemTraNghiepVuAsync(model, maKhachHang);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            kh.MaTaiKhoan = model.MaTaiKhoan;
            GanDuLieuTuForm(kh, model);

            // Đồng bộ tên/email hiển thị của tài khoản đăng nhập (giống chức năng Hồ sơ cá nhân).
            if (kh.MaTaiKhoan != null)
            {
                var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(kh.MaTaiKhoan.Value);
                if (taiKhoan != null)
                {
                    taiKhoan.HoTen = kh.HoTen;
                    if (!string.IsNullOrWhiteSpace(kh.Email)) taiKhoan.Email = kh.Email;
                }
            }

            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã cập nhật khách hàng {kh.HoTen}.");
        }

        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int maKhachHang)
        {
            var kh = await _uow.KhachHangs.GetByIdAsync(maKhachHang);
            if (kh == null) return KetQuaDichVu.KhongTimThay();

            if (kh.TrangThai && await _uow.KhachHangs.KiemTraCoPhieuChuaDongAsync(maKhachHang))
                return KetQuaDichVu.Loi("Khách hàng đang có phiếu chờ xử lý/đang xử lý, chưa thể ngừng hoạt động.");

            kh.TrangThai = !kh.TrangThai;
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok(kh.TrangThai
                ? $"Đã kích hoạt lại khách hàng {kh.HoTen}."
                : $"Đã chuyển khách hàng {kh.HoTen} sang ngừng hoạt động.");
        }

        public async Task<KetQuaDichVu> XoaAsync(int maKhachHang)
        {
            var kh = await _uow.KhachHangs.GetByIdAsync(maKhachHang);
            if (kh == null) return KetQuaDichVu.KhongTimThay();

            if (await _uow.KhachHangs.KiemTraCoDuLieuPhatSinhAsync(maKhachHang))
                return KetQuaDichVu.Loi("Khách hàng đã có thiết bị hoặc phiếu sửa chữa nên không thể xóa. Hãy chuyển sang Ngừng hoạt động.");

            _uow.KhachHangs.Remove(kh);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã xóa khách hàng {kh.HoTen}.");
        }

        // Các quy tắc không biểu diễn được bằng Data Annotation (đề mục 13).
        private async Task<List<LoiTruongNhap>> KiemTraNghiepVuAsync(KhachHangFormViewModel model, int maKhachHangHienTai)
        {
            var loi = new List<LoiTruongNhap>();

            if (model.LoaiKhachHang != LoaiKhachHangEnum.DoanhNghiep)
                model.MaSoThue = null;
            else if (string.IsNullOrWhiteSpace(model.MaSoThue))
                loi.Add(new(nameof(model.MaSoThue), "Khách hàng doanh nghiệp cần có mã số thuế."));

            var cccd = string.IsNullOrWhiteSpace(model.CCCD) ? null : model.CCCD.Trim();
            if (cccd != null && await _uow.KhachHangs.KiemTraTrungCccdAsync(cccd, maKhachHangHienTai))
                loi.Add(new(nameof(model.CCCD), "CCCD này đã được sử dụng."));

            if (model.MaTaiKhoan.HasValue)
            {
                var taiKhoan = await _uow.TaiKhoans.GetByIdAsync(model.MaTaiKhoan.Value);
                if (taiKhoan == null || taiKhoan.VaiTro != VaiTroEnum.KhachHang)
                    loi.Add(new(nameof(model.MaTaiKhoan), "Tài khoản không tồn tại hoặc không phải tài khoản khách hàng."));
                else if (!taiKhoan.TrangThai)
                    loi.Add(new(nameof(model.MaTaiKhoan), "Tài khoản này đang bị khóa."));
                else if (await _uow.KhachHangs.KiemTraTaiKhoanDaLienKetAsync(taiKhoan.MaTaiKhoan, maKhachHangHienTai))
                    loi.Add(new(nameof(model.MaTaiKhoan), "Tài khoản này đã gắn với một hồ sơ khách hàng khác."));
            }

            return loi;
        }

        private static void GanDuLieuTuForm(KhachHang kh, KhachHangFormViewModel model)
        {
            kh.HoTen = model.HoTen.Trim();
            kh.SoDienThoai = model.SoDienThoai.Trim();
            kh.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            kh.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();
            kh.CCCD = string.IsNullOrWhiteSpace(model.CCCD) ? null : model.CCCD.Trim();
            kh.LoaiKhachHang = model.LoaiKhachHang;
            kh.MaSoThue = string.IsNullOrWhiteSpace(model.MaSoThue) ? null : model.MaSoThue.Trim();
            kh.GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim();
        }

        // ===================== HỒ SƠ CÁ NHÂN (khách hàng) =====================

        public async Task<HoSoKhachHangViewModel?> LayHoSoAsync(int maTaiKhoan)
        {
            var kh = await _uow.KhachHangs.LayTheoMaTaiKhoanAsync(maTaiKhoan, tracking: false);
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
            var kh = await _uow.KhachHangs.LayTheoMaTaiKhoanAsync(maTaiKhoan, tracking: true);

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
            if (cccd != null && await _uow.KhachHangs.KiemTraTrungCccdAsync(cccd, kh.MaKhachHang))
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

            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã cập nhật hồ sơ cá nhân.").Kem(kh.HoTen);
        }
    }
}
