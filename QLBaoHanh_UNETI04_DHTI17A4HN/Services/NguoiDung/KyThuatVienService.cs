// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - quản lý hồ sơ kỹ thuật viên (tách từ KyThuatVienController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class KyThuatVienService : IKyThuatVienService
    {
        private readonly IUnitOfWork _uow;
        public KyThuatVienService(IUnitOfWork uow) => _uow = uow;

        public Task<List<KyThuatVien>> TimKiemAsync(string? tuKhoa, bool? trangThai) =>
            _uow.KyThuatViens.TimKiemAsync(tuKhoa, trangThai);

        public Task<List<TaiKhoan>> LayTaiKhoanNhanVienAsync(int? taiKhoanHienTai = null) =>
            _uow.TaiKhoans.LayDanhSachNhanVienChuaCoHoSoKtvAsync(taiKhoanHienTai);

        public async Task<KyThuatVienViewModel?> LayDeSuaAsync(int id)
        {
            var ktv = await _uow.KyThuatViens.GetByIdAsync(id);
            if (ktv == null) return null;

            return new KyThuatVienViewModel
            {
                MaKyThuatVien = ktv.MaKyThuatVien,
                MaTaiKhoan = ktv.MaTaiKhoan,
                HoTen = ktv.HoTen,
                ChuyenMon = ktv.ChuyenMon,
                SoDienThoai = ktv.SoDienThoai,
                Email = ktv.Email,
                TrangThai = ktv.TrangThai
            };
        }

        public async Task<KetQuaDichVu> ThemAsync(KyThuatVienViewModel model, bool modelHopLe)
        {
            if (!modelHopLe)
                return KetQuaDichVu.DuLieuKhongHopLe();

            var taiKhoanHopLe = await _uow.TaiKhoans.KiemTraNhanVienHoatDongHopLeAsync(model.MaTaiKhoan);
            var daCoHoSo = await _uow.KyThuatViens.KiemTraTaiKhoanDaCoHoSoAsync(model.MaTaiKhoan);
            if (!taiKhoanHopLe || daCoHoSo)
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(nameof(model.MaTaiKhoan),
                        "Tài khoản phải là nhân viên hoạt động và chưa có hồ sơ kỹ thuật viên.")
                });

            await _uow.KyThuatViens.AddAsync(new KyThuatVien
            {
                MaTaiKhoan = model.MaTaiKhoan,
                HoTen = model.HoTen.Trim(),
                ChuyenMon = model.ChuyenMon?.Trim(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                Email = model.Email?.Trim(),
                TrangThai = true
            });
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã tạo hồ sơ kỹ thuật viên.");
        }

        public async Task<KetQuaDichVu<int>> CapNhatAsync(int id, KyThuatVienViewModel model, bool modelHopLe)
        {
            var ktv = await _uow.KyThuatViens.GetByIdAsync(id);
            if (ktv == null) return KetQuaDichVu.KhongTimThay().Kem(0);
            var taiKhoanCu = ktv.MaTaiKhoan;

            if (!modelHopLe)
                return KetQuaDichVu.DuLieuKhongHopLe().Kem(taiKhoanCu);

            var taiKhoanHopLe = await _uow.TaiKhoans.KiemTraNhanVienHoatDongHopLeAsync(model.MaTaiKhoan);
            var taiKhoanDaGan = await _uow.KyThuatViens.KiemTraTaiKhoanDaCoHoSoAsync(model.MaTaiKhoan, id);
            if (!taiKhoanHopLe || taiKhoanDaGan)
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(nameof(model.MaTaiKhoan),
                        "Tài khoản phải là nhân viên hoạt động và chưa được gán hồ sơ khác.")
                }).Kem(taiKhoanCu);

            ktv.MaTaiKhoan = model.MaTaiKhoan;
            ktv.HoTen = model.HoTen.Trim();
            ktv.ChuyenMon = model.ChuyenMon?.Trim();
            ktv.SoDienThoai = model.SoDienThoai?.Trim();
            ktv.Email = model.Email?.Trim();
            ktv.TrangThai = model.TrangThai;
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Đã cập nhật hồ sơ kỹ thuật viên.").Kem(taiKhoanCu);
        }
    }
}
