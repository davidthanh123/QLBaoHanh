// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Đăng nhập/Đăng xuất (Session), CRUD Tài khoản (danh sách, thêm, sửa, khóa/mở khóa, xóa an toàn).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Cả Controller chỉ dành cho Admin; 3 action đăng nhập/đăng xuất/từ chối có [AllowAnonymous]
    [PhanQuyen(VaiTroEnum.Admin)]
    public class TaiKhoanController : Controller
    {
        // Sai liên tiếp 5 lần thì khóa tạm 15 phút (SoLanDangNhapSai + KhoaDenNgay, THIETKE.md mục 2.9)
        private const int SoLanSaiToiDa = 5;
        private static readonly TimeSpan ThoiGianKhoaTam = TimeSpan.FromMinutes(15);

        private readonly AppDbContext _db;

        public TaiKhoanController(AppDbContext db)
        {
            _db = db;
        }

        // =====================================================================
        // ĐĂNG NHẬP / ĐĂNG XUẤT
        // =====================================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (HttpContext.Session.DaDangNhap())
                return RedirectToAction("Index", "Home");

            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenNhap = model.TenDangNhap.Trim();
            var bayGio = DateTime.Now;
            var diaChiIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == tenNhap);

            // 1. Không tồn tại: báo chung chung để không lộ tên đăng nhập nào có thật
            if (taiKhoan == null)
            {
                GhiNhatKy(null, tenNhap, KetQuaDangNhapEnum.KhongTonTai, bayGio, diaChiIp);
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 2. Bị Admin khóa vĩnh viễn
            if (!taiKhoan.TrangThai)
            {
                GhiNhatKy(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.BiKhoa, bayGio, diaChiIp);
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // 3. Đang bị khóa tạm do nhập sai nhiều lần
            if (taiKhoan.KhoaDenNgay.HasValue && taiKhoan.KhoaDenNgay.Value > bayGio)
            {
                GhiNhatKy(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.BiKhoa, bayGio, diaChiIp);
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty,
                    $"Tài khoản tạm khóa do nhập sai nhiều lần. Thử lại sau {taiKhoan.KhoaDenNgay.Value:HH:mm dd/MM/yyyy}.");
                return View(model);
            }

            // 4. Sai mật khẩu
            if (!MatKhauHelper.KiemTra(taiKhoan, model.MatKhau, out var canBamLai))
            {
                taiKhoan.SoLanDangNhapSai++;
                string thongBao;
                if (taiKhoan.SoLanDangNhapSai >= SoLanSaiToiDa)
                {
                    taiKhoan.KhoaDenNgay = bayGio.Add(ThoiGianKhoaTam);
                    taiKhoan.SoLanDangNhapSai = 0;
                    thongBao = $"Bạn đã nhập sai {SoLanSaiToiDa} lần. Tài khoản tạm khóa {ThoiGianKhoaTam.TotalMinutes:0} phút.";
                }
                else
                {
                    thongBao = $"Tên đăng nhập hoặc mật khẩu không đúng. Còn {SoLanSaiToiDa - taiKhoan.SoLanDangNhapSai} lần thử.";
                }
                GhiNhatKy(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.SaiMatKhau, bayGio, diaChiIp);
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, thongBao);
                return View(model);
            }

            // 5. Thành công
            taiKhoan.SoLanDangNhapSai = 0;
            taiKhoan.KhoaDenNgay = null;
            taiKhoan.LanDangNhapCuoi = bayGio;
            if (canBamLai)
                taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, model.MatKhau);
            GhiNhatKy(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.ThanhCong, bayGio, diaChiIp);
            await _db.SaveChangesAsync();

            // Xóa Session cũ rồi ghi mới để tránh dùng lại phiên của người trước
            HttpContext.Session.Clear();
            HttpContext.Session.GhiDangNhap(taiKhoan.MaTaiKhoan, taiKhoan.HoTen, taiKhoan.VaiTro);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["ThanhCong"] = "Bạn đã đăng xuất.";
            return RedirectToAction(nameof(DangNhap));
        }

        // Trang 403: đã đăng nhập nhưng không đủ quyền
        [AllowAnonymous]
        public IActionResult TuChoi()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // =====================================================================
        // CRUD TÀI KHOẢN (Admin)
        // =====================================================================

        public async Task<IActionResult> Index(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai)
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

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;
            ViewBag.MaDangNhap = HttpContext.Session.LayMaTaiKhoan();

            var danhSach = await query.OrderBy(t => t.TenDangNhap).ToListAsync();
            return View(danhSach);
        }

        public async Task<IActionResult> Details(int id)
        {
            var taiKhoan = await _db.TaiKhoans.AsNoTracking()
                .Include(t => t.KhachHang)
                .Include(t => t.KyThuatVien)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            ViewBag.NhatKy = await _db.NhatKyDangNhaps.AsNoTracking()
                .Where(n => n.MaTaiKhoan == id)
                .OrderByDescending(n => n.ThoiGian)
                .Take(10)
                .ToListAsync();
            ViewBag.MaDangNhap = HttpContext.Session.LayMaTaiKhoan();
            return View(taiKhoan);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TaiKhoanFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanFormViewModel model)
        {
            model.TenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
            model.HoTen = (model.HoTen ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không được để trống");

            if (ModelState.IsValid && await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

            if (!ModelState.IsValid)
                return View(model);

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
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            TempData["ThanhCong"] = $"Đã thêm tài khoản {taiKhoan.TenDangNhap}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var taiKhoan = await _db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            ViewBag.LaChinhMinh = taiKhoan.MaTaiKhoan == HttpContext.Session.LayMaTaiKhoan();
            return View(new TaiKhoanFormViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel model)
        {
            if (id != model.MaTaiKhoan) return BadRequest();

            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            var laChinhMinh = taiKhoan.MaTaiKhoan == HttpContext.Session.LayMaTaiKhoan();
            ViewBag.LaChinhMinh = laChinhMinh;

            // Tên đăng nhập không đổi khi sửa: bỏ qua giá trị gửi lên
            model.TenDangNhap = taiKhoan.TenDangNhap;
            ModelState.Remove(nameof(model.TenDangNhap));
            model.HoTen = (model.HoTen ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty).Trim();

            if (ModelState.IsValid)
            {
                var doiVaiTroHoacKhoa = model.VaiTro != taiKhoan.VaiTro || model.TrangThai != taiKhoan.TrangThai;
                if (laChinhMinh && doiVaiTroHoacKhoa)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tự đổi vai trò hoặc tự khóa tài khoản đang đăng nhập.");
                }
                else if (taiKhoan.VaiTro == VaiTroEnum.Admin && taiKhoan.TrangThai
                         && (model.VaiTro != VaiTroEnum.Admin || !model.TrangThai)
                         && await LaAdminHoatDongCuoiCungAsync(id))
                {
                    ModelState.AddModelError(string.Empty, "Phải còn ít nhất một quản trị viên đang hoạt động.");
                }
            }

            if (!ModelState.IsValid)
                return View(model);

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
            TempData["ThanhCong"] = $"Đã cập nhật tài khoản {taiKhoan.TenDangNhap}.";
            return RedirectToAction(nameof(Index));
        }

        // Khóa/mở khóa (đảo TrangThai)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            if (taiKhoan.MaTaiKhoan == HttpContext.Session.LayMaTaiKhoan())
            {
                TempData["Loi"] = "Không thể tự khóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            if (taiKhoan.TrangThai)
            {
                if (taiKhoan.VaiTro == VaiTroEnum.Admin && await LaAdminHoatDongCuoiCungAsync(id))
                {
                    TempData["Loi"] = "Phải còn ít nhất một quản trị viên đang hoạt động.";
                    return RedirectToAction(nameof(Index));
                }
                taiKhoan.TrangThai = false;
                TempData["ThanhCong"] = $"Đã khóa tài khoản {taiKhoan.TenDangNhap}.";
            }
            else
            {
                taiKhoan.TrangThai = true;
                taiKhoan.SoLanDangNhapSai = 0;
                taiKhoan.KhoaDenNgay = null;
                TempData["ThanhCong"] = $"Đã mở khóa tài khoản {taiKhoan.TenDangNhap}.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Xóa an toàn: chỉ xóa tài khoản chưa phát sinh dữ liệu; còn lại hướng dẫn dùng "Khóa"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            if (taiKhoan.MaTaiKhoan == HttpContext.Session.LayMaTaiKhoan())
            {
                TempData["Loi"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }
            if (taiKhoan.VaiTro == VaiTroEnum.Admin && taiKhoan.TrangThai && await LaAdminHoatDongCuoiCungAsync(id))
            {
                TempData["Loi"] = "Phải còn ít nhất một quản trị viên đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var coDuLieuLienQuan =
                await _db.KhachHangs.AnyAsync(k => k.MaTaiKhoan == id)
                || await _db.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == id)
                || await _db.PhieuSuaChuas.AnyAsync(p => p.MaTaiKhoanTiepNhan == id)
                || await _db.LichSuTrangThaiPhieus.AnyAsync(l => l.MaTaiKhoanThucHien == id)
                || await _db.TepDinhKems.AnyAsync(t => t.MaTaiKhoanUpload == id)
                || await _db.ThongBaos.AnyAsync(t => t.MaTaiKhoanNhan == id)
                || await _db.NhatKyDangNhaps.AnyAsync(n => n.MaTaiKhoan == id);

            if (coDuLieuLienQuan)
            {
                TempData["Loi"] = $"Không thể xóa tài khoản {taiKhoan.TenDangNhap}: đã có dữ liệu liên quan (hồ sơ, phiếu hoặc nhật ký đăng nhập). Hãy dùng chức năng Khóa tài khoản.";
                return RedirectToAction(nameof(Index));
            }

            _db.TaiKhoans.Remove(taiKhoan);
            try
            {
                await _db.SaveChangesAsync();
                TempData["ThanhCong"] = $"Đã xóa tài khoản {taiKhoan.TenDangNhap}.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] = "Không thể xóa tài khoản này vì đã có dữ liệu liên quan. Hãy dùng chức năng Khóa tài khoản.";
            }
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================

        // true nếu ngoài tài khoản `id` không còn Admin nào đang hoạt động
        private async Task<bool> LaAdminHoatDongCuoiCungAsync(int id) =>
            !await _db.TaiKhoans.AnyAsync(t => t.MaTaiKhoan != id && t.VaiTro == VaiTroEnum.Admin && t.TrangThai);

        private void GhiNhatKy(int? maTaiKhoan, string tenNhap, KetQuaDangNhapEnum ketQua, DateTime thoiGian, string? diaChiIp)
        {
            _db.NhatKyDangNhaps.Add(new NhatKyDangNhap
            {
                MaTaiKhoan = maTaiKhoan,
                TenDangNhapNhap = tenNhap.Length > 50 ? tenNhap[..50] : tenNhap,
                ThoiGian = thoiGian,
                KetQua = ketQua,
                DiaChiIP = diaChiIp,
            });
        }
    }
}
