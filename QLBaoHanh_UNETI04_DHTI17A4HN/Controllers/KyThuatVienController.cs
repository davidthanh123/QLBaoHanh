// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - quản lý hồ sơ kỹ thuật viên và tài khoản được phân công.

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTroEnum.Admin)]
    public class KyThuatVienController : Controller
    {
        private readonly IKyThuatVienService _service;

        public KyThuatVienController(IKyThuatVienService service) => _service = service;

        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        {
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            return View(await _service.TimKiemAsync(tuKhoa, trangThai));
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
            var ketQua = await _service.ThemAsync(model, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapTaiKhoanNhanVien();
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.LayDeSuaAsync(id);
            if (model == null) return NotFound();

            await NapTaiKhoanNhanVien(model.MaTaiKhoan);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KyThuatVienViewModel model)
        {
            if (model.MaKyThuatVien != id) return BadRequest();

            var ketQua = await _service.CapNhatAsync(id, model, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapTaiKhoanNhanVien(ketQua.DuLieu); // DuLieu = tài khoản đang gắn với hồ sơ
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        private async Task NapTaiKhoanNhanVien(int? taiKhoanHienTai = null)
        {
            ViewBag.TaiKhoanNhanViens = await _service.LayTaiKhoanNhanVienAsync(taiKhoanHienTai);
        }
    }
}
