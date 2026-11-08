// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Xem nhật ký đăng nhập (Admin). Chỉ đọc: nhật ký không có chức năng sửa/xóa (THIETKE.md mục 2.7).

using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTroEnum.Admin)]
    public class NhatKyDangNhapController : Controller
    {
        private readonly INhatKyDangNhapService _service;

        public NhatKyDangNhapController(INhatKyDangNhapService service) => _service = service;

        public async Task<IActionResult> Index(string? tuKhoa, KetQuaDangNhapEnum? ketQua)
        {
            var kq = await _service.LayDanhSachAsync(tuKhoa, ketQua);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.KetQua = ketQua;
            ViewBag.SoDongToiDa = kq.SoDongToiDa;
            return View(kq.DanhSach);
        }
    }
}
