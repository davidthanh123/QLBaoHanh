// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Đăng nhập/Đăng xuất (Session), CRUD Tài khoản (danh sách, thêm, sửa, khóa/mở khóa, xóa an toàn).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Cả Controller chỉ dành cho Admin; 3 action đăng nhập/đăng xuất/từ chối có [AllowAnonymous]
    [PhanQuyen(VaiTroEnum.Admin)]
    public class TaiKhoanController : Controller
    {
        private readonly IDangNhapService _dangNhapService;
        private readonly ITaiKhoanService _taiKhoanService;

        public TaiKhoanController(IDangNhapService dangNhapService, ITaiKhoanService taiKhoanService)
        {
            _dangNhapService = dangNhapService;
            _taiKhoanService = taiKhoanService;
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

            var diaChiIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ketQua = await _dangNhapService.DangNhapAsync(model, diaChiIp);

            if (!ketQua.ThanhCong)
            {
                ModelState.AddModelError(string.Empty, ketQua.ThongBao!);
                return View(model);
            }

            // Xóa Session cũ rồi ghi mới để tránh dùng lại phiên của người trước
            var tt = ketQua.DuLieu!;
            HttpContext.Session.Clear();
            HttpContext.Session.GhiDangNhap(tt.MaTaiKhoan, tt.HoTen, tt.VaiTro);

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
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;
            ViewBag.MaDangNhap = HttpContext.Session.LayMaTaiKhoan();

            return View(await _taiKhoanService.TimKiemAsync(tuKhoa, vaiTro, trangThai));
        }

        public async Task<IActionResult> Details(int id)
        {
            var chiTiet = await _taiKhoanService.LayChiTietAsync(id);
            if (chiTiet == null) return NotFound();

            ViewBag.NhatKy = chiTiet.NhatKy;
            ViewBag.MaDangNhap = HttpContext.Session.LayMaTaiKhoan();
            return View(chiTiet.TaiKhoan);
        }

        [HttpGet]
        public IActionResult Create() => View(new TaiKhoanFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanFormViewModel model)
        {
            var ketQua = await _taiKhoanService.ThemAsync(model, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _taiKhoanService.LayDeSuaAsync(id);
            if (model == null) return NotFound();

            ViewBag.LaChinhMinh = model.MaTaiKhoan == HttpContext.Session.LayMaTaiKhoan();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel model)
        {
            if (id != model.MaTaiKhoan) return BadRequest();

            var maDangNhap = HttpContext.Session.LayMaTaiKhoan();
            ViewBag.LaChinhMinh = id == maDangNhap;

            // Tên đăng nhập không đổi khi sửa: bỏ qua lỗi validate của ô này (Service ghi đè giá trị từ DB).
            ModelState.Remove(nameof(model.TenDangNhap));

            var ketQua = await _taiKhoanService.CapNhatAsync(id, model, maDangNhap, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // Khóa/mở khóa (đảo TrangThai)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var ketQua = await _taiKhoanService.DoiTrangThaiAsync(id, HttpContext.Session.LayMaTaiKhoan());
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // Xóa an toàn: chỉ xóa tài khoản chưa phát sinh dữ liệu; còn lại hướng dẫn dùng "Khóa"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var ketQua = await _taiKhoanService.XoaAsync(id, HttpContext.Session.LayMaTaiKhoan());
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }
    }
}
