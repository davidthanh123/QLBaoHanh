// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - quản lý hồ sơ kỹ thuật viên và tài khoản được phân công.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTroEnum.Admin)]
    public class KyThuatVienController : Controller
    {
        private readonly AppDbContext _db;
        public KyThuatVienController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
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
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            return View(await query.OrderBy(k => k.HoTen).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await NapTaiKhoanNhanVien();
            return View(new KyThuatVienViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KyThuatVienViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await NapTaiKhoanNhanVien();
                return View(model);
            }
            var taiKhoanHopLe = await _db.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan == model.MaTaiKhoan && t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai);
            var daCoHoSo = await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == model.MaTaiKhoan);
            if (!taiKhoanHopLe || daCoHoSo)
            {
                ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản phải là nhân viên hoạt động và chưa có hồ sơ kỹ thuật viên.");
                await NapTaiKhoanNhanVien();
                return View(model);
            }

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
            TempData["ThanhCong"] = "Đã tạo hồ sơ kỹ thuật viên.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ktv = await _db.KyThuatViens.FindAsync(id);
            if (ktv == null) return NotFound();
            await NapTaiKhoanNhanVien(ktv.MaTaiKhoan);
            return View(new KyThuatVienViewModel
            {
                MaKyThuatVien = ktv.MaKyThuatVien,
                MaTaiKhoan = ktv.MaTaiKhoan,
                HoTen = ktv.HoTen,
                ChuyenMon = ktv.ChuyenMon,
                SoDienThoai = ktv.SoDienThoai,
                Email = ktv.Email,
                TrangThai = ktv.TrangThai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KyThuatVienViewModel model)
        {
            if (model.MaKyThuatVien != id) return BadRequest();
            var ktv = await _db.KyThuatViens.FindAsync(id);
            if (ktv == null) return NotFound();
            if (!ModelState.IsValid)
            {
                await NapTaiKhoanNhanVien(ktv.MaTaiKhoan);
                return View(model);
            }
            var taiKhoanHopLe = await _db.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan == model.MaTaiKhoan && t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai);
            var taiKhoanDaGan = await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == model.MaTaiKhoan && k.MaKyThuatVien != id);
            if (!taiKhoanHopLe || taiKhoanDaGan)
            {
                ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản phải là nhân viên hoạt động và chưa được gán hồ sơ khác.");
                await NapTaiKhoanNhanVien(ktv.MaTaiKhoan);
                return View(model);
            }

            ktv.MaTaiKhoan = model.MaTaiKhoan;
            ktv.HoTen = model.HoTen.Trim();
            ktv.ChuyenMon = model.ChuyenMon?.Trim();
            ktv.SoDienThoai = model.SoDienThoai?.Trim();
            ktv.Email = model.Email?.Trim();
            ktv.TrangThai = model.TrangThai;
            await _db.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã cập nhật hồ sơ kỹ thuật viên.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapTaiKhoanNhanVien(int? taiKhoanHienTai = null)
        {
            ViewBag.TaiKhoanNhanViens = await _db.TaiKhoans.AsNoTracking()
                .Where(t => t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai &&
                    (t.KyThuatVien == null || t.MaTaiKhoan == taiKhoanHienTai))
                .OrderBy(t => t.HoTen).ToListAsync();
        }
    }
}
