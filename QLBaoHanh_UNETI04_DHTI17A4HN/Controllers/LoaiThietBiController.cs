// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - CRUD Loại thiết bị (Admin): tên bắt buộc và không trùng, kiểm soát trước khi xóa khi đã có thiết bị.

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTroEnum.Admin)]
    public class LoaiThietBiController : Controller
    {
        private readonly ILoaiThietBiService _service;

        public LoaiThietBiController(ILoaiThietBiService service) => _service = service;

        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        {
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            return View(await _service.LayDanhSachAsync(tuKhoa, trangThai));
        }

        public async Task<IActionResult> Details(int id)
        {
            var chiTiet = await _service.LayChiTietAsync(id);
            if (chiTiet == null) return NotFound();

            ViewBag.SoThietBi = chiTiet.SoThietBi;
            return View(chiTiet.Loai);
        }

        [HttpGet]
        public IActionResult Create() => View(new LoaiThietBi());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenLoai,HangSanXuat,MoTa,ThoiHanBaoHanhMacDinh,TrangThai")] LoaiThietBi model)
        {
            var ketQua = await _service.ThemAsync(model, ModelState.IsValid);
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
            var loai = await _service.LayDeSuaAsync(id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaLoai,TenLoai,HangSanXuat,MoTa,ThoiHanBaoHanhMacDinh,TrangThai")] LoaiThietBi model)
        {
            if (id != model.MaLoai) return BadRequest();

            var ketQua = await _service.SuaAsync(id, model, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // Ngừng sử dụng / sử dụng lại (đảo TrangThai). Loại ngừng sử dụng không nên được chọn khi thêm thiết bị mới.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var ketQua = await _service.DoiTrangThaiAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // Xóa an toàn: đã có thiết bị tham chiếu thì không xóa, báo lỗi và gợi ý "Ngừng sử dụng"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var ketQua = await _service.XoaAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }
    }
}
