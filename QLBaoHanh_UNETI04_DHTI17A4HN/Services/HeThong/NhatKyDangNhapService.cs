// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 1 - Xem nhật ký đăng nhập (chỉ đọc), tách từ NhatKyDangNhapController.

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class NhatKyDangNhapService : INhatKyDangNhapService
    {
        private const int SoDongToiDa = 200;
        private readonly AppDbContext _db;
        public NhatKyDangNhapService(AppDbContext db) => _db = db;

        public async Task<DanhSachNhatKy> LayDanhSachAsync(string? tuKhoa, KetQuaDangNhapEnum? ketQua)
        {
            var query = _db.NhatKyDangNhaps.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(n => n.TenDangNhapNhap.Contains(k));
            }
            if (ketQua.HasValue)
                query = query.Where(n => n.KetQua == ketQua.Value);

            var danhSach = await query.OrderByDescending(n => n.ThoiGian).Take(SoDongToiDa).ToListAsync();
            return new DanhSachNhatKy(danhSach, SoDongToiDa);
        }
    }
}
