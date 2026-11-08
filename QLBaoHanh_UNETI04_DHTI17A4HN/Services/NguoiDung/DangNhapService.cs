// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - nghiệp vụ đăng nhập: kiểm tra mật khẩu, khóa tạm khi sai nhiều lần, ghi nhật ký (tách từ TaiKhoanController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class DangNhapService : IDangNhapService
    {
        // Sai liên tiếp 5 lần thì khóa tạm 15 phút (SoLanDangNhapSai + KhoaDenNgay, THIETKE.md mục 2.9)
        private const int SoLanSaiToiDa = 5;
        private static readonly TimeSpan ThoiGianKhoaTam = TimeSpan.FromMinutes(15);

        private readonly IUnitOfWork _uow;
        public DangNhapService(IUnitOfWork uow) => _uow = uow;

        public async Task<KetQuaDichVu<ThongTinDangNhap?>> DangNhapAsync(DangNhapViewModel model, string? diaChiIp)
        {
            var tenNhap = model.TenDangNhap.Trim();
            var bayGio = DateTime.Now;

            var taiKhoan = await _uow.TaiKhoans.LayTheoTenDangNhapAsync(tenNhap);

            // 1. Không tồn tại: báo chung chung để không lộ tên đăng nhập nào có thật
            if (taiKhoan == null)
            {
                await GhiNhatKyAsync(null, tenNhap, KetQuaDangNhapEnum.KhongTonTai, bayGio, diaChiIp);
                await _uow.SaveChangesAsync();
                return ThatBai("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            // 2. Bị Admin khóa vĩnh viễn
            if (!taiKhoan.TrangThai)
            {
                await GhiNhatKyAsync(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.BiKhoa, bayGio, diaChiIp);
                await _uow.SaveChangesAsync();
                return ThatBai("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
            }

            // 3. Đang bị khóa tạm do nhập sai nhiều lần
            if (taiKhoan.KhoaDenNgay.HasValue && taiKhoan.KhoaDenNgay.Value > bayGio)
            {
                await GhiNhatKyAsync(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.BiKhoa, bayGio, diaChiIp);
                await _uow.SaveChangesAsync();
                return ThatBai($"Tài khoản tạm khóa do nhập sai nhiều lần. Thử lại sau {taiKhoan.KhoaDenNgay.Value:HH:mm dd/MM/yyyy}.");
            }

            // 4. Sai mật khẩu
            if (!MatKhauHelper.KiemTra(taiKhoan, model.MatKhau, out var canBamLai))
            {
                taiKhoan.SoLanDangNhapSai++;
                string thongBao;
                if (taiKhoan.SoLanDangNhapSai >= SoLanSaiToiDa)
                {
                    taiKhoan.KhoaDenNgay = bayGio.Add(ThoiGianKhoaTam);
                    taiKhoan.SoLanDangNhapSai = 0;
                    thongBao = $"Bạn đã nhập sai {SoLanSaiToiDa} lần. Tài khoản tạm khóa {ThoiGianKhoaTam.TotalMinutes:0} phút.";
                }
                else
                {
                    thongBao = $"Tên đăng nhập hoặc mật khẩu không đúng. Còn {SoLanSaiToiDa - taiKhoan.SoLanDangNhapSai} lần thử.";
                }
                await GhiNhatKyAsync(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.SaiMatKhau, bayGio, diaChiIp);
                await _uow.SaveChangesAsync();
                return ThatBai(thongBao);
            }

            // 5. Thành công
            taiKhoan.SoLanDangNhapSai = 0;
            taiKhoan.KhoaDenNgay = null;
            taiKhoan.LanDangNhapCuoi = bayGio;
            if (canBamLai)
                taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, model.MatKhau);
            await GhiNhatKyAsync(taiKhoan.MaTaiKhoan, tenNhap, KetQuaDangNhapEnum.ThanhCong, bayGio, diaChiIp);
            await _uow.SaveChangesAsync();

            return KetQuaDichVu.Ok().Kem<ThongTinDangNhap?>(
                new ThongTinDangNhap(taiKhoan.MaTaiKhoan, taiKhoan.HoTen, taiKhoan.VaiTro));
        }

        private static KetQuaDichVu<ThongTinDangNhap?> ThatBai(string thongBao) =>
            KetQuaDichVu.Loi(thongBao).Kem<ThongTinDangNhap?>(null);

        private async Task GhiNhatKyAsync(int? maTaiKhoan, string tenNhap, KetQuaDangNhapEnum ketQua, DateTime thoiGian, string? diaChiIp)
        {
            await _uow.NhatKyDangNhaps.AddAsync(new NhatKyDangNhap
            {
                MaTaiKhoan = maTaiKhoan,
                TenDangNhapNhap = tenNhap.Length > 50 ? tenNhap[..50] : tenNhap,
                ThoiGian = thoiGian,
                KetQua = ketQua,
                DiaChiIP = diaChiIp,
            });
        }
    }
}
