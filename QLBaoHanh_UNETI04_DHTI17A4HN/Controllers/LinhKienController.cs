// Họ và tên: Nguyễn Văn Vũ
// Mã sinh viên: Điền mã sinh viên của bạn
// Nội dung thực hiện: Module 5 - Quản lý linh kiện.

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class LinhKienController : Controller
    {
        // Controller này dùng khóa TempData "Success"/"Error" (khác "ThanhCong"/"Loi" của các module khác).
        private const string KhoaThanhCong = "Success";
        private const string KhoaLoi = "Error";

        private readonly ILinhKienService _service;

        public LinhKienController(ILinhKienService service) => _service = service;

        // GET: LinhKien
        public async Task<IActionResult> Index(string? searchString, string? trangThai, string? sapXep)
        {
            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;
            return View(await _service.TimKiemAsync(searchString, trangThai, sapXep));
        }

        // GET: LinhKien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var linhKien = await _service.LayChiTietAsync(id.Value);
            if (linhKien == null) return NotFound();
            return View(linhKien);
        }

        // GET: LinhKien/Create
        public IActionResult Create() => View();

        // POST: LinhKien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LinhKien linhKien)
        {
            var ketQua = await _service.ThemAsync(linhKien, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                return View(linhKien);
            }

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }

        // GET: LinhKien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var linhKien = await _service.LayChiTietAsync(id.Value);
            if (linhKien == null) return NotFound();
            return View(linhKien);
        }

        // POST: LinhKien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LinhKien linhKien)
        {
            if (id != linhKien.MaLinhKien) return NotFound();

            var ketQua = await _service.CapNhatAsync(id, linhKien, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                return View(linhKien);
            }

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }

        // GET: LinhKien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var linhKien = await _service.LayChiTietAsync(id.Value);
            if (linhKien == null) return NotFound();
            return View(linhKien);
        }

        // POST: LinhKien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketQua = await _service.XoaAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }
    }
}
