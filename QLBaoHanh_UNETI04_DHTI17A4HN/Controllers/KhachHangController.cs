using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen]
    public class KhachHangController : Controller
    {
        private readonly AppDbContext _db;

        public KhachHangController(AppDbContext db) => _db = db;

        // Danh sách khách hàng chỉ dành cho Admin/Nhân viên.
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        {
            var query = _db.KhachHangs
                .AsNoTracking()
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoa) ||
                    k.SoDienThoai.Contains(tuKhoa) ||
                    (k.Email != null && k.Email.Contains(tuKhoa)) ||
                    (k.CCCD != null && k.CCCD.Contains(tuKhoa)));
            }

            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            return View(await query.OrderBy(k => k.HoTen).ToListAsync());
        }

        // Hồ sơ của chính tài khoản hiện tại; không nhận MaKhachHang từ URL.
        public async Task<IActionResult> HoSo()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _db.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);
            if (kh == null)
            {
                TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng. Vui lòng liên hệ nhân viên.";
                return View("HoSo", new HoSoKhachHangViewModel());
            }

            return View(new HoSoKhachHangViewModel
            {
                MaKhachHang = kh.MaKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                CCCD = kh.CCCD,
                LoaiKhachHang = kh.LoaiKhachHang,
                MaSoThue = kh.MaSoThue
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSo(HoSoKhachHangViewModel model)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = await _db.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            if (kh == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ khách hàng gắn với tài khoản hiện tại.";
                return RedirectToAction(nameof(HoSo));
            }

            if (model.LoaiKhachHang != LoaiKhachHangEnum.DoanhNghiep)
                model.MaSoThue = null;
            if (model.LoaiKhachHang == LoaiKhachHangEnum.DoanhNghiep &&
                string.IsNullOrWhiteSpace(model.MaSoThue))
                ModelState.AddModelError(nameof(model.MaSoThue), "Khách hàng doanh nghiệp cần có mã số thuế.");

            var cccd = string.IsNullOrWhiteSpace(model.CCCD) ? null : model.CCCD.Trim();
            if (cccd != null && await _db.KhachHangs.AnyAsync(k => k.CCCD == cccd && k.MaKhachHang != kh.MaKhachHang))
                ModelState.AddModelError(nameof(model.CCCD), "CCCD này đã được sử dụng.");

            if (!ModelState.IsValid)
                return View(model);

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
            HttpContext.Session.GhiDangNhap(maTaiKhoan.Value, kh.HoTen, HttpContext.Session.LayVaiTro() ?? VaiTroEnum.KhachHang);
            TempData["ThanhCong"] = "Đã cập nhật hồ sơ cá nhân.";
            return RedirectToAction(nameof(HoSo));
        }
    }
}
