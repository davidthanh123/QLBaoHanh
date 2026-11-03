// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Nghiệp vụ chi tiết sửa chữa và tồn kho linh kiện (tách từ ChiTietSuaChuaController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class ChiTietSuaChuaService : IChiTietSuaChuaService
    {
        private readonly IUnitOfWork _uow;

        public ChiTietSuaChuaService(IUnitOfWork uow) => _uow = uow;

        // ===================== TRUY VẤN =====================

        public Task<List<ChiTietSuaChua>> LayDanhSachAsync() =>
            _uow.ChiTietSuaChuas.LayDanhSachKemThongTinAsync();

        // Dùng chung cho trang Details và Delete (cùng tập Include).
        public Task<ChiTietSuaChua?> LayChiTietAsync(int id) =>
            _uow.ChiTietSuaChuas.LayChiTietKemThongTinAsync(id);

        public Task<ChiTietSuaChua?> LayDeSuaAsync(int id) =>
            _uow.ChiTietSuaChuas.GetByIdAsync(id);

        public Task<List<PhieuSuaChua>> LayDsPhieuAsync() =>
            _uow.PhieuSuaChuas.GetAllAsync();

        public Task<List<KyThuatVien>> LayDsKyThuatVienAsync() =>
            _uow.KyThuatViens.GetAllAsync();

        public Task<List<LinhKien>> LayDsLinhKienAsync(LocLinhKien loc, int? maLinhKienDangChon = null) =>
            _uow.LinhKiens.LayDanhSachTheoLocAsync(loc, maLinhKienDangChon);

        public Task<List<GiaLinhKien>> LayGiaLinhKienAsync() =>
            _uow.LinhKiens.LayDanhSachGiaLinhKienAsync();

        // ===================== THÊM =====================

        public async Task<KetQuaDichVu> ThemAsync(ChiTietSuaChua model, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();

            // Kiểm tra linh kiện nếu có sử dụng
            LinhKien? linhKien = null;
            if (model.MaLinhKien.HasValue)
            {
                linhKien = await _uow.LinhKiens.GetByIdAsync(model.MaLinhKien.Value);

                if (linhKien == null)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện không tồn tại."));
                }
                else
                {
                    if (!linhKien.TrangThai)
                        loi.Add(new(nameof(model.MaLinhKien), "Linh kiện đã ngừng hoạt động."));

                    if (model.SoLuong > linhKien.SoLuongTon)
                        loi.Add(new(nameof(model.SoLuong), "Số lượng sử dụng vượt quá số lượng tồn kho."));
                }
            }

            // Lấy đơn giá từ dữ liệu linh kiện, không tin giá gửi từ form
            if (linhKien != null)
                model.DonGia = linhKien.DonGia;

            // Tự tính thành tiền
            model.ThanhTien = model.SoLuong * model.DonGia;

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            if (linhKien != null)
                linhKien.SoLuongTon -= model.SoLuong;

            await _uow.ChiTietSuaChuas.AddAsync(model);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Thêm chi tiết sửa chữa thành công.");
        }

        // ===================== SỬA =====================

        public async Task<KetQuaDichVu> CapNhatAsync(int id, ChiTietSuaChua model, bool modelHopLe)
        {
            // Lấy chi tiết cũ từ database
            var chiTietCu = await _uow.ChiTietSuaChuas.GetByIdAsync(id);
            if (chiTietCu == null) return KetQuaDichVu.KhongTimThay();

            var loi = new List<LoiTruongNhap>();

            // Lấy linh kiện mới nếu có sử dụng
            LinhKien? linhKienMoi = null;
            if (model.MaLinhKien.HasValue)
            {
                linhKienMoi = await _uow.LinhKiens.GetByIdAsync(model.MaLinhKien.Value);

                if (linhKienMoi == null)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện không tồn tại."));
                }
                else if (!linhKienMoi.TrangThai)
                {
                    loi.Add(new(nameof(model.MaLinhKien), "Linh kiện đã ngừng hoạt động."));
                }
                else
                {
                    // Nếu vẫn dùng linh kiện cũ, cộng lại số lượng cũ để tính lượng tồn có thể sử dụng.
                    int tonCoTheDung = linhKienMoi.SoLuongTon;
                    if (chiTietCu.MaLinhKien == linhKienMoi.MaLinhKien)
                        tonCoTheDung += chiTietCu.SoLuong;

                    if (model.SoLuong > tonCoTheDung)
                        loi.Add(new(nameof(model.SoLuong), "Số lượng sử dụng vượt quá tồn kho."));
                }
            }

            // Lấy đơn giá chính thức từ linh kiện
            if (model.MaLinhKien.HasValue)
            {
                if (linhKienMoi != null)
                    model.DonGia = linhKienMoi.DonGia;
            }
            else
            {
                model.DonGia = 0;
            }

            // Tự động tính thành tiền
            model.ThanhTien = model.SoLuong * model.DonGia;

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            // Hoàn lại số lượng của linh kiện cũ
            if (chiTietCu.MaLinhKien.HasValue)
            {
                var linhKienCu = await _uow.LinhKiens.GetByIdAsync(chiTietCu.MaLinhKien.Value);
                if (linhKienCu != null)
                    linhKienCu.SoLuongTon += chiTietCu.SoLuong;
            }

            // Trừ số lượng linh kiện mới
            if (linhKienMoi != null)
                linhKienMoi.SoLuongTon -= model.SoLuong;

            // Cập nhật bản ghi đang được theo dõi
            chiTietCu.MaPhieu = model.MaPhieu;
            chiTietCu.MaKyThuatVien = model.MaKyThuatVien;
            chiTietCu.MaLinhKien = model.MaLinhKien;
            chiTietCu.SoLuong = model.SoLuong;
            chiTietCu.DonGia = model.DonGia;
            chiTietCu.ThanhTien = model.ThanhTien;
            chiTietCu.KetQuaChanDoan = model.KetQuaChanDoan;
            chiTietCu.GhiChu = model.GhiChu;

            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Cập nhật chi tiết sửa chữa thành công.");
        }

        // ===================== XÓA =====================

        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var chiTiet = await _uow.ChiTietSuaChuas.GetByIdAsync(id);
            if (chiTiet == null) return KetQuaDichVu.KhongTimThay();

            // Hoàn lại số lượng linh kiện vào kho
            if (chiTiet.MaLinhKien.HasValue)
            {
                var linhKien = await _uow.LinhKiens.GetByIdAsync(chiTiet.MaLinhKien.Value);
                if (linhKien != null)
                    linhKien.SoLuongTon += chiTiet.SoLuong;
            }

            _uow.ChiTietSuaChuas.Remove(chiTiet);

            // Lưu cả thay đổi tồn kho và thao tác xóa
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok("Xóa chi tiết sửa chữa và hoàn lại linh kiện thành công.");
        }
    }
}
