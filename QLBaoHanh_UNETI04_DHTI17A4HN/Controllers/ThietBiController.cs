// Họ và tên: Đào Hoàng Thành
// Mã sinh viên: 23103100223
// Nội dung thực hiện: Module 2 - Quản lý thiết bị: danh sách (tìm kiếm, lọc, sắp xếp, phân trang), thêm, sửa, chi tiết, đổi trạng thái, xóa có kiểm soát.

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen] // đã đăng nhập; từng action quản lý có [PhanQuyen] riêng
    public class ThietBiController : Controller
    {
        private readonly IThietBiService _service;

        public ThietBiController(IThietBiService service) => _service = service;

        // Nạp 2 dropdown của form thêm/sửa vào ViewBag (View _Form đọc ViewBag.DsLoai và ViewBag.DsKhachHang).
        private async Task NapDanhSachChonAsync(int? maLoaiHienTai = null, int? maKhachHangHienTai = null)
        {
            var ds = await _service.LayDanhSachChonAsync(maLoaiHienTai, maKhachHangHienTai);
            ViewBag.DsLoai = ds.DsLoai;
            ViewBag.DsKhachHang = ds.DsKhachHang;
        }

        // ---------- Danh sách ----------
        public async Task<IActionResult> Index(ThietBiFilter f)
        {
            var vm = await _service.LayDanhSachAsync(
                f, HttpContext.Session.LayMaTaiKhoan(), HttpContext.Session.LayVaiTro());
            return View(vm);
        }

        // ---------- Chi tiết ----------
        public async Task<IActionResult> Details(int id)
        {
            var tb = await _service.LayChiTietAsync(
                id, HttpContext.Session.LayMaTaiKhoan(), HttpContext.Session.LayVaiTro());
            if (tb == null) return NotFound();
            return View(tb);
        }

        // ---------- Thêm ----------
        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachChonAsync();
            return View(new ThietBi
            {
                NgayMua = DateTime.Today,
                HanBaoHanh = DateTime.Today.AddMonths(12)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create(
            [Bind("MaLoai,MaKhachHang,SerialNumber,TenThietBi,NgayMua,HanBaoHanh,MoTa,GhiChu,GiaTri")] ThietBi model)
        {
            var ketQua = await _service.ThemAsync(model, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapDanhSachChonAsync();
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Index));
        }

        // ---------- Sửa ----------
        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id)
        {
            var tb = await _service.LayDeSuaAsync(id);
            if (tb == null) return NotFound();

            await NapDanhSachChonAsync(tb.MaLoai, tb.MaKhachHang);
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaThietBi,MaLoai,MaKhachHang,SerialNumber,TenThietBi,NgayMua,HanBaoHanh,MoTa,GhiChu,GiaTri")] ThietBi model)
        {
            if (id != model.MaThietBi) return BadRequest();

            var ketQua = await _service.CapNhatAsync(id, model, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapDanhSachChonAsync(ketQua.DuLieu.MaLoai, ketQua.DuLieu.MaKhachHang);
                return View(model);
            }

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------- Đổi trạng thái: chỉ KhaDung <-> KhongKhaDung ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> DoiTrangThai(int id, TrangThaiThietBiEnum trangThaiMoi)
        {
            var ketQua = await _service.DoiTrangThaiAsync(id, trangThaiMoi);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------- Xóa an toàn (Admin): đã có phiếu sửa chữa thì không xóa ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin)]
        public async Task<IActionResult> Xoa(int id)
        {
            var ketQua = await _service.XoaAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            // Xóa được -> về danh sách; bị chặn -> ở lại trang chi tiết để đọc lý do.
            return ketQua.ThanhCong
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(Details), new { id });
        }
    }
}
