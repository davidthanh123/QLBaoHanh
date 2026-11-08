// Họ và tên: Nguyễn Văn Vũ
// Mã sinh viên: Điền mã sinh viên của bạn
// Nội dung thực hiện: Module 5 - nghiệp vụ Quản lý linh kiện (tách từ LinhKienController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class LinhKienService : ILinhKienService
    {
        private const string LoiTrungTen = "Tên linh kiện đã tồn tại.";
        private readonly IUnitOfWork _uow;

        public LinhKienService(IUnitOfWork uow) => _uow = uow;

        public Task<List<LinhKien>> TimKiemAsync(string? tuKhoa, string? trangThai, string? sapXep) =>
            _uow.LinhKiens.TimKiemAsync(tuKhoa, trangThai, sapXep);

        public Task<LinhKien?> LayChiTietAsync(int id) =>
            _uow.LinhKiens.GetByIdAsync(id);

        public async Task<KetQuaDichVu> ThemAsync(LinhKien linhKien, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();
            if (await _uow.LinhKiens.KiemTraTrungTenAsync(linhKien.TenLinhKien))
                loi.Add(new(nameof(linhKien.TenLinhKien), LoiTrungTen));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            await _uow.LinhKiens.AddAsync(linhKien);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Thêm linh kiện thành công.");
        }

        public async Task<KetQuaDichVu> CapNhatAsync(int id, LinhKien linhKien, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();
            if (await _uow.LinhKiens.KiemTraTrungTenAsync(linhKien.TenLinhKien, id))
                loi.Add(new(nameof(linhKien.TenLinhKien), LoiTrungTen));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            var entity = await _uow.LinhKiens.GetByIdAsync(id);
            if (entity == null) return KetQuaDichVu.KhongTimThay();

            entity.TenLinhKien = linhKien.TenLinhKien;
            entity.DonViTinh = linhKien.DonViTinh;
            entity.DonGia = linhKien.DonGia;
            entity.SoLuongTon = linhKien.SoLuongTon;
            entity.MoTa = linhKien.MoTa;
            entity.TrangThai = linhKien.TrangThai;

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[]
                {
                    new LoiTruongNhap(string.Empty, "Không thể lưu thay đổi. Hãy kiểm tra dữ liệu.")
                });
            }

            return KetQuaDichVu.Ok("Cập nhật linh kiện thành công.");
        }

        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var linhKien = await _uow.LinhKiens.GetByIdAsync(id);
            if (linhKien == null) return KetQuaDichVu.KhongTimThay();

            _uow.LinhKiens.Remove(linhKien);
            try
            {
                await _uow.SaveChangesAsync();
                return KetQuaDichVu.Ok("Xóa linh kiện thành công.");
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.Loi("Không thể xóa linh kiện đã có chi tiết sửa chữa sử dụng.");
            }
        }

        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id)
        {
            var linhKien = await _uow.LinhKiens.GetByIdAsync(id);
            if (linhKien == null) return KetQuaDichVu.KhongTimThay();

            linhKien.TrangThai = !linhKien.TrangThai;
            await _uow.SaveChangesAsync();
            var thongBao = linhKien.TrangThai ? "Đã kích hoạt linh kiện." : "Đã tạm dừng linh kiện.";
            return KetQuaDichVu.Ok(thongBao);
        }
    }
}
