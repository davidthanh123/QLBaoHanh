// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - tạo phiếu, theo dõi phiếu của khách hàng, hủy phiếu (Create, TheoDoi, Huy).
//
// Họ và tên: [người phụ trách Module 4 điền]
// Mã sinh viên: [người phụ trách Module 4 điền]
// Nội dung thực hiện: Module 4 - tiếp nhận, xử lý và quản lý trạng thái phiếu sửa chữa (Index, Details, ThemChiTietSuaChua, CapNhatTrangThai).

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Cả Controller: chỉ cần đã đăng nhập (Khách hàng dùng Index/Create/Huy).
    // Các action của Admin/Nhân viên bên dưới có [PhanQuyen(Admin, NhanVien)] riêng.
    // Controller chỉ lo phần HTTP: đọc Session, ModelState, TempData, ViewBag, chọn View/Redirect.
    // Toàn bộ truy vấn và quy tắc nghiệp vụ nằm trong IPhieuSuaChuaService.
    [PhanQuyen]
    public class PhieuSuaChuaController : Controller
    {
        private readonly IPhieuSuaChuaService _service;

        public PhieuSuaChuaController(IPhieuSuaChuaService service) => _service = service;

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLoai,
            TrangThaiPhieuEnum? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string sapXep = "moi-nhat",
            int trang = 1)
        {
            var boLoc = new PhieuSuaChuaBoLoc(tuKhoa, maLoai, trangThai, tuNgay, denNgay, sapXep, trang);
            var ketQua = await _service.LayDanhSachAsync(
                boLoc, HttpContext.Session.LayMaTaiKhoan(), HttpContext.Session.LayVaiTro());

            if (ketQua.CanhBao != null) TempData["Loi"] = ketQua.CanhBao;
            ViewBag.LoaiThietBis = ketQua.LoaiThietBis;
            return View(ketQua.Model);
        }

        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Details(int id)
        {
            var ketQua = await _service.LayChiTietAsync(id);
            if (ketQua == null) return NotFound();

            ViewBag.ConBaoHanh = ketQua.ConBaoHanh;
            ViewBag.KyThuatViens = ketQua.KyThuatViens;
            ViewBag.LinhKiens = ketQua.LinhKiens;
            return View(ketQua.Phieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> ThemChiTietSuaChua(int id, ThemChiTietSuaChuaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Loi"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id });
            }

            var ketQua = await _service.ThemChiTietSuaChuaAsync(id, model);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiPhieuEnum trangThaiMoi, string? ghiChu)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (!maTaiKhoan.HasValue) return Forbid();

            var ketQua = await _service.CapNhatTrangThaiAsync(id, trangThaiMoi, ghiChu, maTaiKhoan.Value);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua);
            return RedirectToAction(nameof(Details), new { id });
        }

        // Khách hàng xem chi tiết/kết quả phiếu của chính mình (đề mục 7.5, 17.1).
        // Phiếu của người khác -> NotFound nên đổi mã trên URL không lộ dữ liệu.
        [PhanQuyen(VaiTroEnum.KhachHang)]
        public async Task<IActionResult> TheoDoi(int id)
        {
            var phieu = await _service.LayChiTietCuaToiAsync(id, HttpContext.Session.LayMaTaiKhoan());
            if (phieu == null) return NotFound();

            return View(phieu);
        }

        // maThietBi: nút "Tạo phiếu sửa chữa" ở danh sách thiết bị (Module 2) truyền sang để chọn sẵn thiết bị.
        [HttpGet]
        public async Task<IActionResult> Create(int? maThietBi)
        {
            var maKhachHang = await _service.LayMaKhachHangHienTaiAsync(HttpContext.Session.LayMaTaiKhoan());
            if (maKhachHang == null)
            {
                TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var model = new TaoPhieuSuaChuaViewModel
            {
                ThietBiCoTheSua = await _service.LayThietBiCoTheSuaAsync(maKhachHang.Value)
            };
            // Chỉ chọn sẵn khi thiết bị nằm trong danh sách được phép (thuộc khách này, đang khả dụng).
            if (maThietBi.HasValue && model.ThietBiCoTheSua.Any(t => t.Value == maThietBi.Value.ToString()))
                model.MaThietBi = maThietBi;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoPhieuSuaChuaViewModel model)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            var maKhachHang = await _service.LayMaKhachHangHienTaiAsync(maTaiKhoan);
            if (maTaiKhoan == null || maKhachHang == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var ketQua = await _service.TaoPhieuAsync(maKhachHang.Value, maTaiKhoan.Value, model, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);

                model.ThietBiCoTheSua = await _service.LayThietBiCoTheSuaAsync(maKhachHang.Value);
                return View(model);
            }

            TempData["ThanhCong"] = ketQua.ThongBao;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Huy(int id)
        {
            var ketQua = await _service.KiemTraCoTheHuyAsync(
                id, HttpContext.Session.LayMaTaiKhoan(), HttpContext.Session.LayVaiTro());

            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (!ketQua.ThanhCong)
            {
                this.DatThongBao(ketQua);
                return RedirectToAction(nameof(Index));
            }

            return View(new HuyPhieuViewModel { MaPhieu = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(HuyPhieuViewModel model)
        {
            var ketQua = await _service.HuyPhieuAsync(
                model, HttpContext.Session.LayMaTaiKhoan(), HttpContext.Session.LayVaiTro(), ModelState.IsValid);

            switch (ketQua.Loai)
            {
                case LoaiKetQua.KhongTimThay:
                    return NotFound();
                case LoaiKetQua.DuLieuKhongHopLe:
                    return View(model);
                default:
                    this.DatThongBao(ketQua);
                    return RedirectToAction(nameof(Index));
            }
        }
    }
}
