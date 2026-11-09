// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - quản lý khách hàng (Admin/NV: danh sách, chi tiết, thêm, sửa, đổi trạng thái, xóa) và hồ sơ cá nhân của khách hàng.

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Cả Controller: chỉ cần đăng nhập (khách hàng dùng HoSo). Action quản trị có [PhanQuyen(Admin, NhanVien)] riêng.
    [PhanQuyen]
    public class KhachHangController : Controller
    {
        private readonly IKhachHangService _service;

        public KhachHangController(IKhachHangService service) => _service = service;

        // ===================== ADMIN / NHÂN VIÊN =====================

        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, LoaiKhachHangEnum? loai, int trang = 1)
            => View(await _service.LayDanhSachAsync(tuKhoa, trangThai, loai, trang));

        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Details(int id)
        {
            var kh = await _service.LayChiTietAsync(id);
            return kh == null ? NotFound() : View(kh);
        }

        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create() => View(await _service.TaoFormMoiAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create(KhachHangFormViewModel model)
        {
            var ketQua = await _service.TaoAsync(model, ModelState.IsValid);
            if (ketQua.ThanhCong)
            {
                this.DatThongBao(ketQua);
                return RedirectToAction(nameof(Index));
            }

            ketQua.GopLoiVaoModelState(ModelState);
            await _service.NapLaiTaiKhoanAsync(model);
            return View(model);
        }

        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.LayFormSuaAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id, KhachHangFormViewModel model)
        {
            var ketQua = await _service.CapNhatAsync(id, model, ModelState.IsValid);
            switch (ketQua.Loai)
            {
                case LoaiKetQua.KhongTimThay:
                    return NotFound();
                case LoaiKetQua.ThanhCong:
                    this.DatThongBao(ketQua);
                    return RedirectToAction(nameof(Details), new { id });
                default:
                    ketQua.GopLoiVaoModelState(ModelState);
                    model.MaKhachHang = id;
                    await _service.NapLaiTaiKhoanAsync(model);
                    return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> DoiTrangThai(int id, string? quayVe)
        {
            var ketQua = await _service.DoiTrangThaiAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return quayVe == nameof(Details)
                ? RedirectToAction(nameof(Details), new { id })
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Xoa(int id)
        {
            var ketQua = await _service.XoaAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // ===================== KHÁCH HÀNG: HỒ SƠ CÁ NHÂN =====================

        // Hồ sơ của chính tài khoản hiện tại; không nhận MaKhachHang từ URL.
        public async Task<IActionResult> HoSo()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var hoSo = await _service.LayHoSoAsync(maTaiKhoan.Value);
            if (hoSo == null)
            {
                // View tự hiện cảnh báo khi MaKhachHang = 0 (không dùng TempData để thông báo không lặp lại ở request sau).
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
