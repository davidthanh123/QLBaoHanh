// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - nghiệp vụ tiếp nhận, xử lý và quản lý trạng thái phiếu sửa chữa (tách từ PhieuSuaChuaController).

using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class PhieuSuaChuaService : IPhieuSuaChuaService
    {
        private const int KichThuocTrang = 10;
        private readonly IUnitOfWork _uow;

        public PhieuSuaChuaService(IUnitOfWork uow) => _uow = uow;

        // ===================== DANH SÁCH =====================

        public async Task<DanhSachPhieuKetQua> LayDanhSachAsync(
            PhieuSuaChuaBoLoc boLoc, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            string? canhBao = null;
            int? maKhachHang = null;

            if (vaiTro == VaiTroEnum.KhachHang)
            {
                if (maTaiKhoan != null)
                {
                    maKhachHang = await _uow.KhachHangs.LayMaKhachHangTheoTaiKhoanAsync(maTaiKhoan.Value);
                }

                if (maKhachHang == null)
                {
                    canhBao = "Tài khoản chưa có hồ sơ khách hàng.";
                    // Mã không tồn tại để không load phiếu nào
                    maKhachHang = -1;
                }
            }

            var phanTrang = await _uow.PhieuSuaChuas.LayDanhSachPhanTrangAsync(boLoc, maKhachHang, KichThuocTrang);

            var model = new PhieuSuaChuaDanhSachViewModel
            {
                TuKhoa = boLoc.TuKhoa,
                MaLoai = boLoc.MaLoai,
                TrangThai = boLoc.TrangThai,
                TuNgay = phanTrang.TuNgay,
                DenNgay = phanTrang.DenNgay,
                SapXep = boLoc.SapXep,
                Trang = phanTrang.TrangHienTai,
                SoTrang = phanTrang.SoTrang,
                TongSoPhieu = phanTrang.TongSoPhieu,
                PhieuSuaChuas = phanTrang.DanhSach
            };

            return new DanhSachPhieuKetQua
            {
                Model = model,
                CanhBao = canhBao,
                LoaiThietBis = await _uow.LoaiThietBis.LayDanhSachHoatDongHoacHienTaiAsync()
            };
        }

        // ===================== CHI TIẾT =====================

        public async Task<ChiTietPhieuKetQua?> LayChiTietAsync(int maPhieu)
        {
            var phieu = await _uow.PhieuSuaChuas.LayChiTietKemTatCaThongTinAsync(maPhieu);
            if (phieu == null) return null;

            var conBaoHanh = phieu.ThietBi != null &&
                phieu.ThietBi.NgayMua.Date <= phieu.NgayTiepNhan.Date &&
                phieu.ThietBi.HanBaoHanh.Date >= phieu.NgayTiepNhan.Date;

            List<KyThuatVien>? kyThuatViens = null;
            List<LinhKien>? linhKiens = null;
            if (phieu.TrangThai == TrangThaiPhieuEnum.DangXuLy)
            {
                kyThuatViens = await _uow.KyThuatViens.LayDanhSachHoatDongAsync();
                linhKiens = await _uow.LinhKiens.LayDanhSachKhaDungAsync();
            }

            return new ChiTietPhieuKetQua
            {
                Phieu = phieu,
                ConBaoHanh = conBaoHanh,
                KyThuatViens = kyThuatViens,
                LinhKiens = linhKiens
            };
        }

        // ===================== GHI NHẬN CHI TIẾT SỬA CHỮA =====================

        public async Task<KetQuaDichVu> ThemChiTietSuaChuaAsync(int maPhieu, ThemChiTietSuaChuaViewModel model)
        {
            await using var transaction = await _uow.BeginTransactionAsync();

            var phieu = await _uow.PhieuSuaChuas.GetByIdAsync(maPhieu);
            if (phieu == null) return KetQuaDichVu.KhongTimThay();
            if (phieu.TrangThai != TrangThaiPhieuEnum.DangXuLy)
                return KetQuaDichVu.Loi("Chỉ được ghi nhận chẩn đoán và chi phí khi phiếu đang xử lý.");

            var kyThuatVien = await _uow.KyThuatViens.LayChiTietKemTaiKhoanAsync(model.MaKyThuatVien);
            if (kyThuatVien == null)
                return KetQuaDichVu.Loi("Kỹ thuật viên không tồn tại hoặc đã ngừng hoạt động.");

            LinhKien? linhKien = null;
            var soLuong = 1;
            decimal donGia;
            if (model.MaLinhKien.HasValue)
            {
                linhKien = await _uow.LinhKiens.LayTheoMaKemTrangThaiAsync(model.MaLinhKien.Value, chiKhaDung: true);
                if (linhKien == null || linhKien.SoLuongTon < model.SoLuong)
                    return KetQuaDichVu.Loi("Linh kiện không khả dụng hoặc tồn kho không đủ số lượng yêu cầu.");

                soLuong = model.SoLuong;
                donGia = linhKien.DonGia;
                linhKien.SoLuongTon -= soLuong;
            }
            else
            {
                donGia = model.DonGiaCong;
            }

            await _uow.ChiTietSuaChuas.AddAsync(new ChiTietSuaChua
            {
                MaPhieu = maPhieu,
                MaKyThuatVien = kyThuatVien.MaKyThuatVien,
                MaLinhKien = linhKien?.MaLinhKien,
                SoLuong = soLuong,
                DonGia = donGia,
                ThanhTien = soLuong * donGia,
                KetQuaChanDoan = model.KetQuaChanDoan.Trim(),
                GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim()
            });

            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return KetQuaDichVu.Ok("Đã ghi nhận chẩn đoán/chi phí sửa chữa.");
        }

        // ===================== CẬP NHẬT TRẠNG THÁI =====================

        public async Task<KetQuaDichVu> CapNhatTrangThaiAsync(
            int maPhieu, TrangThaiPhieuEnum trangThaiMoi, string? ghiChu, int maTaiKhoan)
        {
            await using var transaction = await _uow.BeginTransactionAsync();
            var phieu = await _uow.PhieuSuaChuas.LayPhieuKemThietBiAsync(maPhieu);
            if (phieu == null) return KetQuaDichVu.KhongTimThay();

            var trangThaiCu = phieu.TrangThai;
            if (!ChuyenTrangThaiHopLe(trangThaiCu, trangThaiMoi))
                return KetQuaDichVu.Loi("Luồng chuyển trạng thái không hợp lệ hoặc phiếu đã kết thúc.");

            var laTuChoiHoacHuy = trangThaiMoi is TrangThaiPhieuEnum.Huy or TrangThaiPhieuEnum.TuChoi;
            if (laTuChoiHoacHuy && string.IsNullOrWhiteSpace(ghiChu))
                return KetQuaDichVu.Loi("Vui lòng nhập lý do hủy hoặc từ chối phiếu.");
            if (ghiChu?.Length > 500)
                return KetQuaDichVu.Loi("Ghi chú không được vượt quá 500 ký tự.");
            if (phieu.ThietBi == null || phieu.ThietBi.MaKhachHang != phieu.MaKhachHang)
                return KetQuaDichVu.Loi("Không thể xử lý: thiết bị không tồn tại hoặc không thuộc khách hàng trên phiếu.");

            var khachHangHoatDong = await _uow.KhachHangs.KiemTraKhachHangHoatDongAsync(phieu.MaKhachHang);
            if (!khachHangHoatDong)
                return KetQuaDichVu.Loi("Không thể tiếp nhận phiếu của khách hàng không còn hoạt động.");

            if (trangThaiMoi == TrangThaiPhieuEnum.DangXuLy)
            {
                if (phieu.ThietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    return KetQuaDichVu.Loi("Thiết bị hiện không khả dụng để tiếp nhận sửa chữa.");

                // Mỗi thiết bị chỉ có tối đa một phiếu đang mở.
                var daCoPhieuDangMo = await _uow.PhieuSuaChuas.KiemTraThietBiCoPhieuDangMoAsync(phieu.MaThietBi, phieu.MaPhieu);
                if (daCoPhieuDangMo)
                    return KetQuaDichVu.Loi("Thiết bị đã có phiếu khác đang chờ hoặc đang xử lý.");

                phieu.MaTaiKhoanTiepNhan = maTaiKhoan;
                phieu.ThietBi.TrangThai = TrangThaiThietBiEnum.DangSuaChua;
            }

            if (trangThaiMoi == TrangThaiPhieuEnum.HoanThanh)
            {
                var coChiTiet = await _uow.ChiTietSuaChuas.KiemTraPhieuCoChiTietAsync(maPhieu);
                if (!coChiTiet)
                    return KetQuaDichVu.Loi("Chưa thể hoàn thành: phiếu cần có ít nhất một chi tiết sửa chữa/kết quả.");

                phieu.NgayHoanThanh = DateTime.Now;
                phieu.ThietBi.TrangThai = TrangThaiThietBiEnum.KhaDung;
            }

            if (trangThaiMoi == TrangThaiPhieuEnum.Huy && trangThaiCu == TrangThaiPhieuEnum.DangXuLy)
                phieu.ThietBi.TrangThai = TrangThaiThietBiEnum.KhaDung;

            phieu.TrangThai = trangThaiMoi;
            if (laTuChoiHoacHuy)
                phieu.LyDoHuyTuChoi = ghiChu!.Trim();

            await _uow.LichSuTrangThaiPhieus.AddAsync(new LichSuTrangThaiPhieu
            {
                MaPhieu = phieu.MaPhieu,
                MaTaiKhoanThucHien = maTaiKhoan,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = trangThaiMoi,
                ThoiGian = DateTime.Now,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim()
            });

            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return KetQuaDichVu.Ok("Đã cập nhật trạng thái phiếu và lưu lịch sử xử lý.");
        }

        // ===================== KHÁCH HÀNG TẠO / HỦY PHIẾU =====================

        public Task<int?> LayMaKhachHangHienTaiAsync(int? maTaiKhoan) =>
            maTaiKhoan.HasValue
                ? _uow.KhachHangs.LayMaKhachHangTheoTaiKhoanAsync(maTaiKhoan.Value)
                : Task.FromResult<int?>(null);

        public async Task<List<SelectListItem>> LayThietBiCoTheSuaAsync(int maKhachHang)
        {
            var ds = await _uow.ThietBis.LayDanhSachKhaDungTheoKhachHangAsync(maKhachHang);
            return ds.Select(t => new SelectListItem
            {
                Value = t.MaThietBi.ToString(),
                Text = t.TenThietBi + " — SN: " + t.SerialNumber
            }).ToList();
        }

        public async Task<KetQuaDichVu> TaoPhieuAsync(int maKhachHang, TaoPhieuSuaChuaViewModel model, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();

            if (model.HanDuKien.HasValue && model.HanDuKien.Value.Date < DateTime.Today)
                loi.Add(new(nameof(model.HanDuKien), "Ngày hẹn dự kiến không được ở trong quá khứ."));

            ThietBi? thietBi = null;
            if (model.MaThietBi.HasValue)
            {
                thietBi = await _uow.ThietBis.GetByIdAsync(model.MaThietBi.Value);
                if (thietBi == null || thietBi.MaKhachHang != maKhachHang)
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu của bạn."));
                else if (thietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị hiện không ở trạng thái khả dụng để tiếp nhận sửa chữa."));
                else if (await _uow.PhieuSuaChuas.KiemTraThietBiCoPhieuMoHoacDangSuaAsync(thietBi.MaThietBi))
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị đang có phiếu chờ xử lý/đang sửa chữa. Không thể tạo phiếu trùng."));
            }

            if (!modelHopLe || loi.Count > 0 || thietBi == null)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            var phieu = new PhieuSuaChua
            {
                MaThietBi = thietBi.MaThietBi,
                MaKhachHang = maKhachHang,
                NgayTiepNhan = DateTime.Now,
                NoiDungLoi = model.NoiDungLoi.Trim(),
                MucDo = model.MucDo,
                HanDuKien = model.HanDuKien?.Date,
                TrangThai = TrangThaiPhieuEnum.ChoXuLy
            };

            await _uow.PhieuSuaChuas.AddAsync(phieu);
            await _uow.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã tạo phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}. Phiếu đang chờ tiếp nhận.");
        }

        public async Task<KetQuaDichVu> KiemTraCoTheHuyAsync(int maPhieu, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(maPhieu, maTaiKhoan, vaiTro);
            if (phieu == null) return KetQuaDichVu.KhongTimThay();

            return phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy
                ? KetQuaDichVu.Loi("Chỉ được hủy phiếu ở trạng thái Chờ xử lý.")
                : KetQuaDichVu.Ok();
        }

        public async Task<KetQuaDichVu> HuyPhieuAsync(
            HuyPhieuViewModel model, int? maTaiKhoan, VaiTroEnum? vaiTro, bool modelHopLe)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(model.MaPhieu, maTaiKhoan, vaiTro);
            if (phieu == null) return KetQuaDichVu.KhongTimThay();

            if (phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy)
                return KetQuaDichVu.Loi("Phiếu không còn ở trạng thái Chờ xử lý nên không thể hủy.");

            if (!modelHopLe) return KetQuaDichVu.DuLieuKhongHopLe();

            phieu.TrangThai = TrangThaiPhieuEnum.Huy;
            phieu.LyDoHuyTuChoi = model.LyDo.Trim();
            await _uow.SaveChangesAsync();

            return KetQuaDichVu.Ok($"Đã hủy phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}.");
        }

        // ===================== HÀM NỘI BỘ =====================

        private async Task<PhieuSuaChua?> LayPhieuCuaNguoiDungAsync(int maPhieu, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            int? maKhachHang = null;
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                if (maTaiKhoan == null) return null;
                maKhachHang = await _uow.KhachHangs.LayMaKhachHangTheoTaiKhoanAsync(maTaiKhoan.Value);
                if (maKhachHang == null) return null;
            }
            return await _uow.PhieuSuaChuas.LayPhieuTheoNguoiDungAsync(maPhieu, maKhachHang);
        }

        private static bool ChuyenTrangThaiHopLe(TrangThaiPhieuEnum hienTai, TrangThaiPhieuEnum moi) =>
            (hienTai, moi) switch
            {
                (TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.DangXuLy) => true,
                (TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.Huy) => true,
                (TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.TuChoi) => true,
                (TrangThaiPhieuEnum.DangXuLy, TrangThaiPhieuEnum.HoanThanh) => true,
                (TrangThaiPhieuEnum.DangXuLy, TrangThaiPhieuEnum.Huy) => true,
                _ => false
            };
    }
}
