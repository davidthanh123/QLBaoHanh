// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - CRUD Loại thiết bị (Admin): tên bắt buộc và không trùng, kiểm soát trước khi xóa khi đã có thiết bị.

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
    public class LoaiThietBiController : Controller
    {
        private readonly AppDbContext _db;

        public LoaiThietBiController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        {
            var query = _db.LoaiThietBis.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(l => l.TenLoai.Contains(k) || l.HangSanXuat.Contains(k));
            }
            if (trangThai.HasValue)
                query = query.Where(l => l.TrangThai == trangThai.Value);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;

            var danhSach = await query
                .OrderBy(l => l.TenLoai)
                .Select(l => new LoaiThietBiDongViewModel { Loai = l, SoThietBi = l.ThietBis.Count })
                .ToListAsync();
            return View(danhSach);
        }

        public async Task<IActionResult> Details(int id)
        {
            var loai = await _db.LoaiThietBis.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return NotFound();

            ViewBag.SoThietBi = await _db.ThietBis.CountAsync(t => t.MaLoai == id);
            return View(loai);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LoaiThietBi());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenLoai,HangSanXuat,MoTa,ThoiHanBaoHanhMacDinh,TrangThai")] LoaiThietBi model)
        {
            ChuanHoa(model);
            await KiemTraTrungTenAsync(model, 0);

            if (!ModelState.IsValid)
                return View(model);

            _db.LoaiThietBis.Add(model);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(model.TenLoai), "Tên loại thiết bị đã tồn tại");
                return View(model);
            }

            TempData["ThanhCong"] = $"Đã thêm loại thiết bị {model.TenLoai}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var loai = await _db.LoaiThietBis.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaLoai,TenLoai,HangSanXuat,MoTa,ThoiHanBaoHanhMacDinh,TrangThai")] LoaiThietBi model)
        {
            if (id != model.MaLoai) return BadRequest();

            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return NotFound();

            ChuanHoa(model);
            await KiemTraTrungTenAsync(model, id);

            if (!ModelState.IsValid)
                return View(model);

            // Chỉ cập nhật các trường cho phép sửa trên bản ghi gốc
            loai.TenLoai = model.TenLoai;
            loai.HangSanXuat = model.HangSanXuat;
            loai.MoTa = model.MoTa;
            loai.ThoiHanBaoHanhMacDinh = model.ThoiHanBaoHanhMacDinh;
            loai.TrangThai = model.TrangThai;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(model.TenLoai), "Tên loại thiết bị đã tồn tại");
                return View(model);
            }

            TempData["ThanhCong"] = $"Đã cập nhật loại thiết bị {loai.TenLoai}.";
            return RedirectToAction(nameof(Index));
        }

        // Ngừng sử dụng / sử dụng lại (đảo TrangThai). Loại ngừng sử dụng không nên được chọn khi thêm thiết bị mới.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return NotFound();

            loai.TrangThai = !loai.TrangThai;
            await _db.SaveChangesAsync();

            TempData["ThanhCong"] = loai.TrangThai
                ? $"Đã cho phép sử dụng lại loại thiết bị {loai.TenLoai}."
                : $"Đã ngừng sử dụng loại thiết bị {loai.TenLoai}.";
            return RedirectToAction(nameof(Index));
        }

        // Xóa an toàn: đã có thiết bị tham chiếu thì không xóa, báo lỗi và gợi ý "Ngừng sử dụng"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return NotFound();

            var soThietBi = await _db.ThietBis.CountAsync(t => t.MaLoai == id);
            if (soThietBi > 0)
            {
                TempData["Loi"] = $"Không thể xóa: loại thiết bị {loai.TenLoai} đang có {soThietBi} thiết bị sử dụng. Hãy chọn Ngừng sử dụng thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            _db.LoaiThietBis.Remove(loai);
            try
            {
                await _db.SaveChangesAsync();
                TempData["ThanhCong"] = $"Đã xóa loại thiết bị {loai.TenLoai}.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] = "Không thể xóa loại thiết bị này vì đã có dữ liệu liên quan.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------------

        private static void ChuanHoa(LoaiThietBi model)
        {
            model.TenLoai = (model.TenLoai ?? string.Empty).Trim();
            model.HangSanXuat = (model.HangSanXuat ?? string.Empty).Trim();
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
        }

        // Báo lỗi thân thiện trước khi chạm unique index. id = 0 khi thêm mới.
        private async Task KiemTraTrungTenAsync(LoaiThietBi model, int id)
        {
            if (string.IsNullOrEmpty(model.TenLoai)) return;
            if (await _db.LoaiThietBis.AnyAsync(l => l.TenLoai == model.TenLoai && l.MaLoai != id))
                ModelState.AddModelError(nameof(model.TenLoai), "Tên loại thiết bị đã tồn tại");
        }
    }
}
