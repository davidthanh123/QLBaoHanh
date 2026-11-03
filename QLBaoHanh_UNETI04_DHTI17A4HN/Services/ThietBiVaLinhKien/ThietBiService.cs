// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 2 - nghiệp vụ Thiết bị: danh sách (tìm kiếm, lọc, sắp xếp, phân trang), thêm, sửa, chi tiết, đổi trạng thái, xóa có kiểm soát.

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class ThietBiService : IThietBiService
    {
        private const int KichThuocTrang = 10;
        private const string LoiTrungSerial = "Serial number đã tồn tại";
        private readonly IUnitOfWork _uow;

        public ThietBiService(IUnitOfWork uow) => _uow = uow;

        // ---------- Hàm dùng chung ----------

        private Task<int?> LayMaKhachHangAsync(int? maTaiKhoan) =>
            maTaiKhoan.HasValue
                ? _uow.KhachHangs.LayMaKhachHangTheoTaiKhoanAsync(maTaiKhoan.Value)
                : Task.FromResult<int?>(null);

        private async Task<List<LoiTruongNhap>> KiemTraNghiepVuAsync(ThietBi tb, ThietBi? goc)
        {
            var loi = new List<LoiTruongNhap>();

            if (await _uow.ThietBis.KiemTraTrungSerialAsync(tb.SerialNumber, tb.MaThietBi))
                loi.Add(new(nameof(tb.SerialNumber), LoiTrungSerial));

            if ((goc == null || tb.MaLoai != goc.MaLoai)
                && !await _uow.LoaiThietBis.KiemTraLoaiThietBiHoatDongAsync(tb.MaLoai))
                loi.Add(new(nameof(tb.MaLoai), "Vui lòng chọn loại thiết bị đang sử dụng"));

            if ((goc == null || tb.MaKhachHang != goc.MaKhachHang)
                && !await _uow.KhachHangs.KiemTraKhachHangHoatDongAsync(tb.MaKhachHang))
                loi.Add(new(nameof(tb.MaKhachHang), "Vui lòng chọn khách hàng đang hoạt động"));

            return loi;
        }

        public async Task<DanhSachChonThietBi> LayDanhSachChonAsync(int? maLoaiHienTai = null, int? maKhachHangHienTai = null)
        {
            var dsLoai = await _uow.LoaiThietBis.LayDanhSachHoatDongHoacHienTaiAsync(maLoaiHienTai);
            var dsKhachHang = await _uow.KhachHangs.LayDanhSachHoatDongHoacHienTaiAsync(maKhachHangHienTai);
            return new DanhSachChonThietBi(dsLoai, dsKhachHang);
        }

        // ---------- Danh sách: tìm kiếm + lọc + sắp xếp + phân trang ----------
        public async Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiFilter f, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            int? maKh = null;
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                maKh = await LayMaKhachHangAsync(maTaiKhoan);
            }

            var phanTrang = await _uow.ThietBis.LayDanhSachPhanTrangAsync(f, maKh, KichThuocTrang);

            return new ThietBiIndexViewModel
            {
                Filter = f,
                TotalItems = phanTrang.TongSoLuong,
                TotalPages = phanTrang.SoTrang,
                PageSize = KichThuocTrang,
                Items = phanTrang.DanhSach,
                DsLoai = await _uow.LoaiThietBis.LayDanhSachHoatDongHoacHienTaiAsync(),
                DsHang = await _uow.LoaiThietBis.LayDanhSachHangSanXuatAsync()
            };
        }

        // ---------- Chi tiết ----------
        public async Task<ThietBi?> LayChiTietAsync(int id, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var tb = await _uow.ThietBis.LayChiTietKemThongTinAsync(id);
            if (tb == null) return null;

            if (vaiTro == VaiTroEnum.KhachHang && tb.MaKhachHang != await LayMaKhachHangAsync(maTaiKhoan))
                return null;

            return tb;
        }

        public Task<ThietBi?> LayDeSuaAsync(int id) =>
            _uow.ThietBis.GetByIdAsync(id);

        // ---------- Thêm ----------
        public async Task<KetQuaDichVu> ThemAsync(ThietBi model, bool modelHopLe)
        {
            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            var loi = await KiemTraNghiepVuAsync(model, null);

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            model.TrangThai = TrangThaiThietBiEnum.KhaDung;
            await _uow.ThietBis.AddAsync(model);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.SerialNumber), LoiTrungSerial) });
            }

            return KetQuaDichVu.Ok($"Đã thêm thiết bị {model.TenThietBi}.");
        }

        // ---------- Sửa ----------
        public async Task<KetQuaDichVu<(int MaLoai, int MaKhachHang)>> CapNhatAsync(int id, ThietBi model, bool modelHopLe)
        {
            var tb = await _uow.ThietBis.GetByIdAsync(id);
            if (tb == null) return KetQuaDichVu.KhongTimThay().Kem((0, 0));

            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            var loi = await KiemTraNghiepVuAsync(model, tb);

            if (model.MaKhachHang != tb.MaKhachHang && await _uow.ThietBis.KiemTraCoPhieuSuaChuaAsync(id))
                loi.Add(new(nameof(model.MaKhachHang), "Thiết bị đã có phiếu sửa chữa, không thể đổi khách hàng"));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi).Kem((tb.MaLoai, tb.MaKhachHang));

            tb.MaLoai = model.MaLoai;
            tb.MaKhachHang = model.MaKhachHang;
            tb.SerialNumber = model.SerialNumber;
            tb.TenThietBi = model.TenThietBi;
            tb.NgayMua = model.NgayMua;
            tb.HanBaoHanh = model.HanBaoHanh;
            tb.MoTa = model.MoTa;
            tb.GhiChu = model.GhiChu;
            tb.GiaTri = model.GiaTri;

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.SerialNumber), LoiTrungSerial) })
                    .Kem((tb.MaLoai, tb.MaKhachHang));
            }

            return KetQuaDichVu.Ok($"Đã cập nhật thiết bị {tb.TenThietBi}.").Kem((tb.MaLoai, tb.MaKhachHang));
        }

        // ---------- Đổi trạng thái ----------
        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id, TrangThaiThietBiEnum trangThaiMoi)
        {
            var tb = await _uow.ThietBis.GetByIdAsync(id);
            if (tb == null) return KetQuaDichVu.KhongTimThay();

            if (trangThaiMoi == TrangThaiThietBiEnum.DangSuaChua)
                return KetQuaDichVu.Loi("Trạng thái \"Đang sửa chữa\" do hệ thống đặt khi nhân viên tiếp nhận phiếu.");

            if (tb.TrangThai == TrangThaiThietBiEnum.DangSuaChua)
                return KetQuaDichVu.Loi("Thiết bị đang được sửa chữa, không thể đổi trạng thái.");

            if (trangThaiMoi == TrangThaiThietBiEnum.KhongKhaDung
                && await _uow.ThietBis.KiemTraCoPhieuChuaDongAsync(id))
                return KetQuaDichVu.Loi("Thiết bị còn phiếu sửa chữa chưa đóng, không thể chuyển sang \"Không khả dụng\".");

            tb.TrangThai = trangThaiMoi;
            await _uow.SaveChangesAsync();

            return KetQuaDichVu.Ok(trangThaiMoi == TrangThaiThietBiEnum.KhaDung
                ? $"Thiết bị {tb.TenThietBi} đã khả dụng trở lại."
                : $"Đã ngừng nhận sửa thiết bị {tb.TenThietBi}.");
        }

        // ---------- Xóa an toàn ----------
        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var tb = await _uow.ThietBis.GetByIdAsync(id);
            if (tb == null) return KetQuaDichVu.KhongTimThay();

            if (await _uow.ThietBis.KiemTraCoPhieuSuaChuaAsync(id))
                return KetQuaDichVu.Loi("Không thể xóa: thiết bị đã có phiếu sửa chữa. Hãy chuyển sang \"Không khả dụng\" để ngừng sử dụng.");

            _uow.ThietBis.Remove(tb);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã xóa thiết bị {tb.TenThietBi}.");
        }
    }
}
