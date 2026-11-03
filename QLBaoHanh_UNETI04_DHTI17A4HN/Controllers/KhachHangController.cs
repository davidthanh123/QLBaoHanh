using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen]
    public class KhachHangController : Controller
    {
        private readonly IKhachHangService _service;

        public KhachHangController(IKhachHangService service) => _service = service;

        // Danh sách khách hàng chỉ dành cho Admin/Nhân viên.
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        {
            ViewBag.TuKhoa = string.IsNullOrWhiteSpace(tuKhoa) ? tuKhoa : tuKhoa.Trim();
            ViewBag.TrangThai = trangThai;
            return View(await _service.TimKiemAsync(tuKhoa, trangThai));
        }

        // Hồ sơ của chính tài khoản hiện tại; không nhận MaKhachHang từ URL.
        public async Task<IActionResult> HoSo()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var hoSo = await _service.LayHoSoAsync(maTaiKhoan.Value);
            if (hoSo == null)
            {
                TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng. Vui lòng liên hệ nhân viên.";
                return View("HoSo", new HoSoKhachHangViewModel());
            }

            return View(hoSo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSo(HoSoKhachHangViewModel model)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var ketQua = await _service.CapNhatHoSoAsync(maTaiKhoan.Value, model, ModelState.IsValid);

            switch (ketQua.Loai)
            {
                case LoaiKetQua.LoiNghiepVu: // chưa có hồ sơ khách hàng
                    this.DatThongBao(ketQua);
                    return RedirectToAction(nameof(HoSo));

                case LoaiKetQua.DuLieuKhongHopLe:
                    ketQua.GopLoiVaoModelState(ModelState);
                    return View(model);

                default:
                    // Đồng bộ lại tên hiển thị trong Session sau khi đổi họ tên.
                    HttpContext.Session.GhiDangNhap(maTaiKhoan.Value, ketQua.DuLieu!,
                        HttpContext.Session.LayVaiTro() ?? VaiTroEnum.KhachHang);
                    this.DatThongBao(ketQua);
                    return RedirectToAction(nameof(HoSo));
            }
        }
    }
}
