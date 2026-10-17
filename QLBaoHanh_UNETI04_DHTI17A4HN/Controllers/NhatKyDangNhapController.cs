// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Xem nhật ký đăng nhập (Admin). Chỉ đọc: nhật ký không có chức năng sửa/xóa (THIETKE.md mục 2.7).

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen(VaiTroEnum.Admin)]
    public class NhatKyDangNhapController : Controller
    {
        private const int SoDongToiDa = 200;

        private readonly AppDbContext _db;

        public NhatKyDangNhapController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? tuKhoa, KetQuaDangNhapEnum? ketQua)
        {
            var query = _db.NhatKyDangNhaps.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(n => n.TenDangNhapNhap.Contains(k));
            }
            if (ketQua.HasValue)
                query = query.Where(n => n.KetQua == ketQua.Value);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.KetQua = ketQua;
            ViewBag.SoDongToiDa = SoDongToiDa;

            var danhSach = await query.OrderByDescending(n => n.ThoiGian).Take(SoDongToiDa).ToListAsync();
            return View(danhSach);
        }
    }
}
