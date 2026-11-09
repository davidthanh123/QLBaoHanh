// Họ và tên: Bùi Hồng Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Service NhatKyDangNhap - tra cứu lịch sử đăng nhập hệ thống cho Admin.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class NhatKyDangNhapService : INhatKyDangNhapService
    {
        private const int SoDongToiDa = 200;
        private readonly INhatKyDangNhapRepository _nhatKyRepo;
        public NhatKyDangNhapService(INhatKyDangNhapRepository nhatKyRepo) => _nhatKyRepo = nhatKyRepo;

        public async Task<DanhSachNhatKy> LayDanhSachAsync(string? tuKhoa, KetQuaDangNhapEnum? ketQua)
        {
            var danhSach = await _nhatKyRepo.LayDanhSachAsync(tuKhoa, ketQua, SoDongToiDa);
            return new DanhSachNhatKy(danhSach, SoDongToiDa);
        }
    }
}
