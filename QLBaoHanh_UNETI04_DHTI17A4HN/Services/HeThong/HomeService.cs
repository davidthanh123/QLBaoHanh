// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Dữ liệu cho trang giới thiệu công khai và trang chủ sau đăng nhập (tách khỏi HomeController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _uow;

        public HomeService(IUnitOfWork uow) => _uow = uow;

        public async Task<GioiThieuViewModel> LayGioiThieuAsync()
        {
            return new GioiThieuViewModel
            {
                LoaiThietBis = await _uow.LoaiThietBis.AsNoTracking()
                    .Where(l => l.TrangThai)
                    .OrderBy(l => l.TenLoai)
                    .ToListAsync(),
            };
        }

        public async Task<HomeViewModel> LayTrangChuAsync(int? maTaiKhoan, VaiTroEnum? vaiTro, string? hoTen)
        {
            var vm = new HomeViewModel
            {
                VaiTro = vaiTro ?? VaiTroEnum.KhachHang,
                HoTen = string.IsNullOrWhiteSpace(hoTen) ? "bạn" : hoTen,
            };

            var homNay = DateTime.Today;

            if (vm.VaiTro == VaiTroEnum.KhachHang)
            {
                // Khách chỉ thấy dữ liệu của mình: lọc theo hồ sơ KhachHang gắn với tài khoản đang đăng nhập
                int? maKhachHang = maTaiKhoan == null
                    ? null
                    : await _uow.KhachHangs.LayMaKhachHangTheoTaiKhoanAsync(maTaiKhoan.Value);

                if (maKhachHang == null)
                {
                    vm.ThieuHoSoKhachHang = true;
                    return vm;
                }

                var ma = maKhachHang.Value;
                var hanCuoi = homNay.AddDays(UiHelper.NgayCanhBaoBaoHanh);

                vm.So1 = await _uow.ThietBis.CountAsync(t => t.MaKhachHang == ma);
                vm.So2 = await _uow.PhieuSuaChuas.CountAsync(p => p.MaKhachHang == ma
                    && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));
                vm.So4 = await _uow.PhieuSuaChuas.CountAsync(p => p.MaKhachHang == ma
                    && p.TrangThai == TrangThaiPhieuEnum.HoanThanh);

                vm.ThietBiSapHetBaoHanh = await _uow.ThietBis.AsNoTracking()
                    .Where(t => t.MaKhachHang == ma && t.HanBaoHanh >= homNay && t.HanBaoHanh <= hanCuoi)
                    .OrderBy(t => t.HanBaoHanh)
                    .Take(5)
                    .ToListAsync();
                vm.So3 = vm.ThietBiSapHetBaoHanh.Count;

                vm.PhieuGanDay = await _uow.PhieuSuaChuas.AsNoTracking()
                    .Where(p => p.MaKhachHang == ma)
                    .Include(p => p.ThietBi)
                    .OrderByDescending(p => p.NgayTiepNhan)
                    .Take(5)
                    .ToListAsync();
            }
            else
            {
                // Admin / Nhân viên: nhìn toàn hệ thống
                vm.So1 = await _uow.PhieuSuaChuas.CountAsync(p => p.TrangThai == TrangThaiPhieuEnum.ChoXuLy);
                vm.So2 = await _uow.PhieuSuaChuas.CountAsync(p => p.TrangThai == TrangThaiPhieuEnum.DangXuLy);
                // Quá hạn = chưa đóng (Chờ xử lý / Đang xử lý) và HanDuKien đã qua. Khớp UiHelper.HanDuKien
                vm.So3 = await _uow.PhieuSuaChuas.CountAsync(p =>
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)
                    && p.HanDuKien != null && p.HanDuKien < homNay);
                vm.So4 = await _uow.KhachHangs.CountAsync(k => k.TrangThai);

                vm.PhieuGanDay = await _uow.PhieuSuaChuas.AsNoTracking()
                    .Include(p => p.ThietBi)
                    .Include(p => p.KhachHang)
                    .OrderByDescending(p => p.NgayTiepNhan)
                    .Take(5)
                    .ToListAsync();
            }

            return vm;
        }
    }
}
