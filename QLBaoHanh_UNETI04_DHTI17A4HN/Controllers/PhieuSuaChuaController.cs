using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen]
    public class PhieuSuaChuaController : Controller
    {
        private readonly AppDbContext _db;

        public PhieuSuaChuaController(AppDbContext db) => _db = db;

        // Khách chỉ xem phiếu của mình. Nhân viên/Admin xem danh sách toàn hệ thống.
        public async Task<IActionResult> Index()
        {
            var vaiTro = HttpContext.Session.LayVaiTro();
            var query = _db.PhieuSuaChuas
                .AsNoTracking()
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .AsQueryable();

            if (vaiTro == VaiTroEnum.KhachHang)
            {
                var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
                if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");
                var maKhachHang = await _db.KhachHangs
                    .Where(k => k.MaTaiKhoan == maTaiKhoan.Value)
                    .Select(k => (int?)k.MaKhachHang)
                    .FirstOrDefaultAsync();
                if (maKhachHang == null)
                {
                    TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng.";
                    return View(new List<PhieuSuaChua>());
                }
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }

            return View(await query.OrderByDescending(p => p.NgayTiepNhan).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var kh = await LayKhachHangHienTaiAsync();
            if (kh == null)
            {
                TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var model = new TaoPhieuSuaChuaViewModel();
            await NapThietBiAsync(model, kh.MaKhachHang);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoPhieuSuaChuaViewModel model)
        {
            var kh = await LayKhachHangHienTaiAsync();
            if (kh == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var homNay = DateTime.Today;
            if (model.HanDuKien.HasValue && model.HanDuKien.Value.Date < homNay)
                ModelState.AddModelError(nameof(model.HanDuKien), "Ngày hẹn dự kiến không được ở trong quá khứ.");

            ThietBi? thietBi = null;
            if (model.MaThietBi.HasValue)
            {
                thietBi = await _db.ThietBis.FirstOrDefaultAsync(t =>
                    t.MaThietBi == model.MaThietBi.Value &&
                    t.MaKhachHang == kh.MaKhachHang);
                if (thietBi == null)
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
                else if (thietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị hiện không ở trạng thái khả dụng để tiếp nhận sửa chữa.");
                else if (await _db.PhieuSuaChuas.AnyAsync(p =>
                    p.MaThietBi == thietBi.MaThietBi &&
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)))
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị đang có phiếu chờ xử lý/đang sửa chữa. Không thể tạo phiếu trùng.");
            }

            if (!ModelState.IsValid)
            {
                await NapThietBiAsync(model, kh.MaKhachHang);
                return View(model);
            }

            // Các trường hệ thống không lấy từ form: mã khách, thời điểm nhận và trạng thái.
            var phieu = new PhieuSuaChua
            {
                MaThietBi = thietBi!.MaThietBi,
                MaKhachHang = kh.MaKhachHang,
                NgayTiepNhan = DateTime.Now,
                NoiDungLoi = model.NoiDungLoi.Trim(),
                MucDo = model.MucDo,
                HanDuKien = model.HanDuKien?.Date,
                TrangThai = TrangThaiPhieuEnum.ChoXuLy
            };

            _db.PhieuSuaChuas.Add(phieu);
            await _db.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã tạo phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}. Phiếu đang chờ tiếp nhận.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Huy(int id)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy)
            {
                TempData["Loi"] = "Chỉ được hủy phiếu ở trạng thái Chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            return View(new HuyPhieuViewModel { MaPhieu = phieu.MaPhieu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(HuyPhieuViewModel model)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(model.MaPhieu);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy)
            {
                TempData["Loi"] = "Phiếu không còn ở trạng thái Chờ xử lý nên không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) return View(model);

            phieu.TrangThai = TrangThaiPhieuEnum.Huy;
            phieu.LyDoHuyTuChoi = model.LyDo.Trim();
            await _db.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã hủy phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<KhachHang?> LayKhachHangHienTaiAsync()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return null;
            return await _db.KhachHangs.FirstOrDefaultAsync(k =>
                k.MaTaiKhoan == maTaiKhoan.Value && k.TrangThai);
        }

        private async Task<PhieuSuaChua?> LayPhieuCuaNguoiDungAsync(int id)
        {
            var vaiTro = HttpContext.Session.LayVaiTro();
            var query = _db.PhieuSuaChuas.Where(p => p.MaPhieu == id);
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
                if (maTaiKhoan == null) return null;
                var maKhachHang = await _db.KhachHangs
                    .Where(k => k.MaTaiKhoan == maTaiKhoan.Value)
                    .Select(k => (int?)k.MaKhachHang)
                    .FirstOrDefaultAsync();
                if (maKhachHang == null) return null;
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }
            return await query.FirstOrDefaultAsync();
        }

        private async Task NapThietBiAsync(TaoPhieuSuaChuaViewModel model, int maKhachHang)
        {
            model.ThietBiCoTheSua = await _db.ThietBis
                .AsNoTracking()
                .Where(t => t.MaKhachHang == maKhachHang && t.TrangThai == TrangThaiThietBiEnum.KhaDung)
                .OrderBy(t => t.TenThietBi)
                .Select(t => new SelectListItem
                {
                    Value = t.MaThietBi.ToString(),
                    Text = t.TenThietBi + " — SN: " + t.SerialNumber
                }).ToListAsync();
        }
    }
}
