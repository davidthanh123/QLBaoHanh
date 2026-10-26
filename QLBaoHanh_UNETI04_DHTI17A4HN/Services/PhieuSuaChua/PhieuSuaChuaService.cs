// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - nghiệp vụ tiếp nhận, xử lý và quản lý trạng thái phiếu sửa chữa (tách từ PhieuSuaChuaController).

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class PhieuSuaChuaService : IPhieuSuaChuaService
    {
        private const int KichThuocTrang = 10;
        private readonly AppDbContext _db;

        public PhieuSuaChuaService(AppDbContext db) => _db = db;

        // ===================== DANH SÁCH =====================

        // Tìm khách hàng/thiết bị, lọc loại, trạng thái, ngày, sắp xếp và phân trang trực tiếp trên truy vấn EF Core.
        public async Task<DanhSachPhieuKetQua> LayDanhSachAsync(
            PhieuSuaChuaBoLoc boLoc, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var trang = boLoc.Trang < 1 ? 1 : boLoc.Trang;
            var tuNgay = boLoc.TuNgay;
            var denNgay = boLoc.DenNgay;
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
                (tuNgay, denNgay) = (denNgay, tuNgay);

            var query = _db.PhieuSuaChuas.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .AsQueryable();

            string? canhBao = null;

            // Khách hàng chỉ xem phiếu của chính mình; Admin/Nhân viên xem toàn hệ thống.
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                int? maKhachHang = null;
                if (maTaiKhoan != null)
                {
                    maKhachHang = await _db.KhachHangs
                        .Where(k => k.MaTaiKhoan == maTaiKhoan.Value)
                        .Select(k => (int?)k.MaKhachHang)
                        .FirstOrDefaultAsync();
                }

                if (maKhachHang == null)
                {
                    canhBao = "Tài khoản chưa có hồ sơ khách hàng.";
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
            {
                var tuKhoaSach = boLoc.TuKhoa.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(tuKhoaSach) ||
                    p.KhachHang.SoDienThoai.Contains(tuKhoaSach) ||
                    p.ThietBi!.TenThietBi.Contains(tuKhoaSach) ||
                    p.ThietBi.SerialNumber.Contains(tuKhoaSach));
            }

            if (boLoc.MaLoai.HasValue)
                query = query.Where(p => p.ThietBi != null && p.ThietBi.MaLoai == boLoc.MaLoai.Value);
            if (boLoc.TrangThai.HasValue)
                query = query.Where(p => p.TrangThai == boLoc.TrangThai.Value);
            if (tuNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan >= tuNgay.Value.Date);
            if (denNgay.HasValue)
            {
                var mocKetThuc = denNgay.Value.Date.AddDays(1);
                query = query.Where(p => p.NgayTiepNhan < mocKetThuc);
            }

            query = boLoc.SapXep switch
            {
                "cu-nhat" => query.OrderBy(p => p.NgayTiepNhan),
                "ten-khach" => query.OrderBy(p => p.KhachHang!.HoTen).ThenByDescending(p => p.NgayTiepNhan),
                "ten-thiet-bi" => query.OrderBy(p => p.ThietBi!.TenThietBi).ThenByDescending(p => p.NgayTiepNhan),
                _ => query.OrderByDescending(p => p.NgayTiepNhan)
            };

            var tongSoPhieu = await query.CountAsync();
            var soTrang = Math.Max(1, (int)Math.Ceiling(tongSoPhieu / (double)KichThuocTrang));
            if (trang > soTrang) trang = soTrang;

            var model = new PhieuSuaChuaDanhSachViewModel
            {
                TuKhoa = boLoc.TuKhoa,
                MaLoai = boLoc.MaLoai,
                TrangThai = boLoc.TrangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SapXep = boLoc.SapXep,
                Trang = trang,
                SoTrang = soTrang,
                TongSoPhieu = tongSoPhieu,
                PhieuSuaChuas = await query.Skip((trang - 1) * KichThuocTrang)
                    .Take(KichThuocTrang).ToListAsync()
            };

            return new DanhSachPhieuKetQua
            {
                Model = model,
                CanhBao = canhBao,
                LoaiThietBis = await _db.LoaiThietBis.AsNoTracking()
                    .OrderBy(l => l.TenLoai).ToListAsync()
            };
        }

        // ===================== CHI TIẾT =====================

        public async Task<ChiTietPhieuKetQua?> LayChiTietAsync(int maPhieu)
        {
            var phieu = await _db.PhieuSuaChuas.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.TaiKhoanTiepNhan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.KyThuatVien)
                    .ThenInclude(k => k!.TaiKhoan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .Include(p => p.LichSus).ThenInclude(l => l.TaiKhoanThucHien)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

            if (phieu == null) return null;

            var conBaoHanh = phieu.ThietBi != null &&
                phieu.ThietBi.NgayMua.Date <= phieu.NgayTiepNhan.Date &&
                phieu.ThietBi.HanBaoHanh.Date >= phieu.NgayTiepNhan.Date;

            List<KyThuatVien>? kyThuatViens = null;
            List<LinhKien>? linhKiens = null;
            if (phieu.TrangThai == TrangThaiPhieuEnum.DangXuLy)
            {
                kyThuatViens = await _db.KyThuatViens.AsNoTracking()
                    .Include(k => k.TaiKhoan)
                    .Where(k => k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai)
                    .OrderBy(k => k.HoTen).ToListAsync();
                linhKiens = await _db.LinhKiens.AsNoTracking()
                    .Where(l => l.TrangThai && l.SoLuongTon > 0)
                    .OrderBy(l => l.TenLinhKien).ToListAsync();
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
            // Mọi return sớm bên dưới đều không Commit -> transaction tự rollback khi Dispose.
            await using var transaction = await _db.Database.BeginTransactionAsync();

            var phieu = await _db.PhieuSuaChuas.FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);
            if (phieu == null) return KetQuaDichVu.KhongTimThay();
            if (phieu.TrangThai != TrangThaiPhieuEnum.DangXuLy)
                return KetQuaDichVu.Loi("Chỉ được ghi nhận chẩn đoán và chi phí khi phiếu đang xử lý.");

            var kyThuatVien = await _db.KyThuatViens
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKyThuatVien == model.MaKyThuatVien && k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai);
            if (kyThuatVien == null)
                return KetQuaDichVu.Loi("Kỹ thuật viên không tồn tại hoặc đã ngừng hoạt động.");

            LinhKien? linhKien = null;
            var soLuong = 1;
            decimal donGia;
            if (model.MaLinhKien.HasValue)
            {
                linhKien = await _db.LinhKiens.FirstOrDefaultAsync(l => l.MaLinhKien == model.MaLinhKien.Value && l.TrangThai);
                if (linhKien == null || linhKien.SoLuongTon < model.SoLuong)
                    return KetQuaDichVu.Loi("Linh kiện không khả dụng hoặc tồn kho không đủ số lượng yêu cầu.");

                soLuong = model.SoLuong;
                donGia = linhKien.DonGia; // Giá linh kiện luôn lấy từ DB, không tin giá do trình duyệt gửi.
                linhKien.SoLuongTon -= soLuong;
            }
            else
            {
                donGia = model.DonGiaCong;
            }

            _db.ChiTietSuaChuas.Add(new ChiTietSuaChua
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

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return KetQuaDichVu.Ok("Đã ghi nhận chẩn đoán/chi phí sửa chữa.");
        }

        // ===================== CẬP NHẬT TRẠNG THÁI =====================

        public async Task<KetQuaDichVu> CapNhatTrangThaiAsync(
            int maPhieu, TrangThaiPhieuEnum trangThaiMoi, string? ghiChu, int maTaiKhoan)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            var phieu = await _db.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);
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

            var khachHangHoatDong = await _db.KhachHangs.AnyAsync(k =>
                k.MaKhachHang == phieu.MaKhachHang && k.TrangThai);
            if (!khachHangHoatDong)
                return KetQuaDichVu.Loi("Không thể tiếp nhận phiếu của khách hàng không còn hoạt động.");

            if (trangThaiMoi == TrangThaiPhieuEnum.DangXuLy)
            {
                if (phieu.ThietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    return KetQuaDichVu.Loi("Thiết bị hiện không khả dụng để tiếp nhận sửa chữa.");

                // Mỗi thiết bị chỉ có tối đa một phiếu đang mở.
                var daCoPhieuDangMo = await _db.PhieuSuaChuas.AnyAsync(p =>
                    p.MaThietBi == phieu.MaThietBi && p.MaPhieu != phieu.MaPhieu &&
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));
                if (daCoPhieuDangMo)
                    return KetQuaDichVu.Loi("Thiết bị đã có phiếu khác đang chờ hoặc đang xử lý.");

                phieu.MaTaiKhoanTiepNhan = maTaiKhoan;
                phieu.ThietBi.TrangThai = TrangThaiThietBiEnum.DangSuaChua;
            }

            if (trangThaiMoi == TrangThaiPhieuEnum.HoanThanh)
            {
                var coChiTiet = await _db.ChiTietSuaChuas.AnyAsync(c => c.MaPhieu == maPhieu);
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

            _db.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
            {
                MaPhieu = phieu.MaPhieu,
                MaTaiKhoanThucHien = maTaiKhoan,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = trangThaiMoi,
                ThoiGian = DateTime.Now,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim()
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return KetQuaDichVu.Ok("Đã cập nhật trạng thái phiếu và lưu lịch sử xử lý.");
        }

        // ===================== KHÁCH HÀNG TẠO / HỦY PHIẾU =====================

        public async Task<int?> LayMaKhachHangHienTaiAsync(int? maTaiKhoan)
        {
            if (maTaiKhoan == null) return null;
            return await _db.KhachHangs
                .Where(k => k.MaTaiKhoan == maTaiKhoan.Value && k.TrangThai)
                .Select(k => (int?)k.MaKhachHang)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SelectListItem>> LayThietBiCoTheSuaAsync(int maKhachHang)
        {
            return await _db.ThietBis
                .AsNoTracking()
                .Where(t => t.MaKhachHang == maKhachHang && t.TrangThai == TrangThaiThietBiEnum.KhaDung)
                .OrderBy(t => t.TenThietBi)
                .Select(t => new SelectListItem
                {
                    Value = t.MaThietBi.ToString(),
                    Text = t.TenThietBi + " — SN: " + t.SerialNumber
                }).ToListAsync();
        }

        // modelHopLe = ModelState.IsValid của Controller (lỗi DataAnnotations).
        // Service vẫn chạy kiểm tra nghiệp vụ để gom đủ lỗi hiển thị một lượt, nhưng chỉ tạo phiếu khi không có lỗi nào.
        public async Task<KetQuaDichVu> TaoPhieuAsync(int maKhachHang, TaoPhieuSuaChuaViewModel model, bool modelHopLe)
        {
            var loi = new List<LoiTruongNhap>();

            if (model.HanDuKien.HasValue && model.HanDuKien.Value.Date < DateTime.Today)
                loi.Add(new(nameof(model.HanDuKien), "Ngày hẹn dự kiến không được ở trong quá khứ."));

            ThietBi? thietBi = null;
            if (model.MaThietBi.HasValue)
            {
                thietBi = await _db.ThietBis.FirstOrDefaultAsync(t =>
                    t.MaThietBi == model.MaThietBi.Value &&
                    t.MaKhachHang == maKhachHang);
                if (thietBi == null)
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu của bạn."));
                else if (thietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị hiện không ở trạng thái khả dụng để tiếp nhận sửa chữa."));
                else if (await _db.PhieuSuaChuas.AnyAsync(p =>
                    p.MaThietBi == thietBi.MaThietBi &&
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)))
                    loi.Add(new(nameof(model.MaThietBi), "Thiết bị đang có phiếu chờ xử lý/đang sửa chữa. Không thể tạo phiếu trùng."));
            }

            if (!modelHopLe || loi.Count > 0 || thietBi == null)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            // Các trường hệ thống không lấy từ form: mã khách, thời điểm nhận và trạng thái.
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

            _db.PhieuSuaChuas.Add(phieu);
            await _db.SaveChangesAsync();
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
            await _db.SaveChangesAsync();

            return KetQuaDichVu.Ok($"Đã hủy phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}.");
        }

        // ===================== HÀM NỘI BỘ =====================

        private async Task<PhieuSuaChua?> LayPhieuCuaNguoiDungAsync(int maPhieu, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var query = _db.PhieuSuaChuas.Where(p => p.MaPhieu == maPhieu);
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                if (maTaiKhoan == null) return null;
                var maKhachHang = await _db.KhachHangs
                    .Where(k => k.MaTaiKhoan == maTaiKhoan.Value)
                    .Select(k => (int?)k.MaKhachHang)
                    .FirstOrDefaultAsync();
                if (maKhachHang == null) return null;
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }
            return await query.FirstOrDefaultAsync();
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
