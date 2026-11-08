// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Trang giới thiệu công khai (chưa đăng nhập) + Trang chủ sau đăng nhập theo vai trò (đề mục 17.1, 17.2).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using System.Diagnostics;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Controller chỉ đọc Session và chọn View/Redirect. Truy vấn nằm trong IHomeService.
    public class HomeController : Controller
    {
        private readonly IHomeService _service;

        public HomeController(IHomeService service) => _service = service;

        // Địa chỉ gốc "/". Là nơi duy nhất (cùng Error) mở cho người chưa đăng nhập.
        //  - Chưa đăng nhập: hiện trang giới thiệu (có nút Đăng nhập).
        //  - Đã đăng nhập:   chuyển sang TrangChu. Việc chuyển này giúp TrangChu đi qua DangNhapFilter
        //    (kiểm tra tài khoản bị khóa/đổi vai trò), vì [AllowAnonymous] làm filter bỏ qua action này.
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.DaDangNhap())
                return RedirectToAction(nameof(TrangChu));

            return View("GioiThieu", await _service.LayGioiThieuAsync());
        }

        // Trang chủ sau đăng nhập. Không có [AllowAnonymous] nên DangNhapFilter (global) bắt buộc đăng nhập.
        public async Task<IActionResult> TrangChu()
        {
            var session = HttpContext.Session;
            var vm = await _service.LayTrangChuAsync(session.LayMaTaiKhoan(), session.LayVaiTro(), session.LayHoTen());
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
