// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 2 - nghiệp vụ Thiết bị: danh sách (tìm kiếm, lọc, sắp xếp, phân trang), thêm, sửa, chi tiết, đổi trạng thái, xóa có kiểm soát.

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class ThietBiService : IThietBiService
    {
        private const int KichThuocTrang = 10;
        private const string LoiTrungSerial = "Serial number đã tồn tại";
        private readonly AppDbContext _db;

        public ThietBiService(AppDbContext db) => _db = db;

        // ---------- Hàm dùng chung ----------

        // Khách hàng chỉ thấy thiết bị của mình: tìm hồ sơ KhachHang theo tài khoản đang đăng nhập
        private async Task<int?> LayMaKhachHangAsync(int? maTaiKhoan)
        {
            if (maTaiKhoan == null) return null;
            return await _db.KhachHangs.AsNoTracking()
                .Where(k => k.MaTaiKhoan == maTaiKhoan)
                .Select(k => (int?)k.MaKhachHang)
                .FirstOrDefaultAsync();
        }

        // Quy tắc nghiệp vụ không biểu diễn được bằng Annotation (HanBaoHanh >= NgayMua đã có trong Entity)
        private async Task<List<LoiTruongNhap>> KiemTraNghiepVuAsync(ThietBi tb, ThietBi? goc)
        {
            var loi = new List<LoiTruongNhap>();

            if (await _db.ThietBis.AnyAsync(t => t.SerialNumber == tb.SerialNumber && t.MaThietBi != tb.MaThietBi))
                loi.Add(new(nameof(tb.SerialNumber), LoiTrungSerial));

            if ((goc == null || tb.MaLoai != goc.MaLoai)
                && !await _db.LoaiThietBis.AnyAsync(l => l.MaLoai == tb.MaLoai && l.TrangThai))
                loi.Add(new(nameof(tb.MaLoai), "Vui lòng chọn loại thiết bị đang sử dụng"));

            if ((goc == null || tb.MaKhachHang != goc.MaKhachHang)
                && !await _db.KhachHangs.AnyAsync(k => k.MaKhachHang == tb.MaKhachHang && k.TrangThai))
                loi.Add(new(nameof(tb.MaKhachHang), "Vui lòng chọn khách hàng đang hoạt động"));

            return loi;
        }

        public async Task<DanhSachChonThietBi> LayDanhSachChonAsync(int? maLoaiHienTai = null, int? maKhachHangHienTai = null)
        {
            // Chỉ cho chọn loại/khách đang hoạt động; riêng giá trị hiện tại của bản ghi đang sửa vẫn giữ lại
            var dsLoai = await _db.LoaiThietBis.AsNoTracking()
                .Where(l => l.TrangThai || l.MaLoai == maLoaiHienTai)
                .OrderBy(l => l.TenLoai).ToListAsync();
            var dsKhachHang = await _db.KhachHangs.AsNoTracking()
                .Where(k => k.TrangThai || k.MaKhachHang == maKhachHangHienTai)
                .OrderBy(k => k.HoTen).ToListAsync();
            return new DanhSachChonThietBi(dsLoai, dsKhachHang);
        }

        // ---------- Danh sách: tìm kiếm + lọc + sắp xếp + phân trang ----------
        public async Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiFilter f, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var q = _db.ThietBis.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .AsQueryable();

            if (vaiTro == VaiTroEnum.KhachHang)
            {
                var maKh = await LayMaKhachHangAsync(maTaiKhoan);
                q = q.Where(t => t.MaKhachHang == maKh);
            }

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(f.Keyword))
            {
                var kw = f.Keyword.Trim();
                q = q.Where(t => t.TenThietBi.Contains(kw)
                              || t.SerialNumber.Contains(kw)
                              || t.KhachHang!.HoTen.Contains(kw));
            }

            // Lọc (kết hợp được nhiều điều kiện)
            if (f.MaLoai != null) q = q.Where(t => t.MaLoai == f.MaLoai);
            if (!string.IsNullOrEmpty(f.Hang)) q = q.Where(t => t.LoaiThietBi!.HangSanXuat == f.Hang);
            if (f.TrangThai != null) q = q.Where(t => t.TrangThai == f.TrangThai);
            if (f.GiaTu != null) q = q.Where(t => t.GiaTri >= f.GiaTu);
            if (f.GiaDen != null) q = q.Where(t => t.GiaTri <= f.GiaDen);

            var homNay = DateTime.Today;
            var sapHet = homNay.AddDays(30); // cùng ngưỡng "Sắp hết hạn" của UiHelper.BaoHanh
            q = f.BaoHanh switch
            {
                "con" => q.Where(t => t.HanBaoHanh >= homNay),
                "sap" => q.Where(t => t.HanBaoHanh >= homNay && t.HanBaoHanh <= sapHet),
                "het" => q.Where(t => t.HanBaoHanh < homNay),
                _ => q
            };

            // Sắp xếp
            q = f.Sort switch
            {
                "ten" => q.OrderBy(t => t.TenThietBi),
                "ten_desc" => q.OrderByDescending(t => t.TenThietBi),
                "ngaymua" => q.OrderBy(t => t.NgayMua),
                "ngaymua_desc" => q.OrderByDescending(t => t.NgayMua),
                "hanbh" => q.OrderBy(t => t.HanBaoHanh),
                "hanbh_desc" => q.OrderByDescending(t => t.HanBaoHanh),
                "giatri" => q.OrderBy(t => t.GiaTri),
                "giatri_desc" => q.OrderByDescending(t => t.GiaTri),
                _ => q.OrderByDescending(t => t.MaThietBi)
            };

            // Phân trang trên truy vấn (Skip/Take), không tải toàn bộ dữ liệu
            var tong = await q.CountAsync();
            var soTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)KichThuocTrang));
            f.Page = Math.Clamp(f.Page, 1, soTrang);

            return new ThietBiIndexViewModel
            {
                Filter = f,
                TotalItems = tong,
                TotalPages = soTrang,
                PageSize = KichThuocTrang,
                Items = await q.Skip((f.Page - 1) * KichThuocTrang).Take(KichThuocTrang).ToListAsync(),
                DsLoai = await _db.LoaiThietBis.AsNoTracking().OrderBy(l => l.TenLoai).ToListAsync(),
                DsHang = await _db.LoaiThietBis.AsNoTracking().Select(l => l.HangSanXuat).Distinct().OrderBy(h => h).ToListAsync()
            };
        }

        // ---------- Chi tiết ----------
        public async Task<ThietBi?> LayChiTietAsync(int id, int? maTaiKhoan, VaiTroEnum? vaiTro)
        {
            var tb = await _db.ThietBis.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .Include(t => t.PhieuSuaChuas)
                .FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return null;

            // Khách hàng đổi mã trên URL để xem thiết bị người khác: không lộ dữ liệu
            if (vaiTro == VaiTroEnum.KhachHang && tb.MaKhachHang != await LayMaKhachHangAsync(maTaiKhoan))
                return null;

            return tb;
        }

        public Task<ThietBi?> LayDeSuaAsync(int id) =>
            _db.ThietBis.AsNoTracking().FirstOrDefaultAsync(t => t.MaThietBi == id);

        // ---------- Thêm ----------
        public async Task<KetQuaDichVu> ThemAsync(ThietBi model, bool modelHopLe)
        {
            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            var loi = await KiemTraNghiepVuAsync(model, null);

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            model.TrangThai = TrangThaiThietBiEnum.KhaDung;
            _db.ThietBis.Add(model);
            try
            {
                await _db.SaveChangesAsync();
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
            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return KetQuaDichVu.KhongTimThay().Kem((0, 0));

            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            var loi = await KiemTraNghiepVuAsync(model, tb);

            // Đã có phiếu sửa chữa thì không đổi chủ sở hữu (bảo toàn lịch sử)
            if (model.MaKhachHang != tb.MaKhachHang && await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id))
                loi.Add(new(nameof(model.MaKhachHang), "Thiết bị đã có phiếu sửa chữa, không thể đổi khách hàng"));

            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi).Kem((tb.MaLoai, tb.MaKhachHang));

            // Chỉ cập nhật các trường cho phép sửa (TrangThai đổi ở hàm riêng)
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
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.SerialNumber), LoiTrungSerial) })
                    .Kem((tb.MaLoai, tb.MaKhachHang));
            }

            return KetQuaDichVu.Ok($"Đã cập nhật thiết bị {tb.TenThietBi}.").Kem((tb.MaLoai, tb.MaKhachHang));
        }

        // ---------- Đổi trạng thái: chỉ KhaDung <-> KhongKhaDung ----------
        // DangSuaChua do luồng phiếu (Module 4) tự đặt/gỡ, không đổi tay ở đây.
        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id, TrangThaiThietBiEnum trangThaiMoi)
        {
            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return KetQuaDichVu.KhongTimThay();

            if (trangThaiMoi == TrangThaiThietBiEnum.DangSuaChua)
                return KetQuaDichVu.Loi("Trạng thái \"Đang sửa chữa\" do hệ thống đặt khi nhân viên tiếp nhận phiếu.");

            if (tb.TrangThai == TrangThaiThietBiEnum.DangSuaChua)
                return KetQuaDichVu.Loi("Thiết bị đang được sửa chữa, không thể đổi trạng thái.");

            if (trangThaiMoi == TrangThaiThietBiEnum.KhongKhaDung
                && await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id
                    && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)))
                return KetQuaDichVu.Loi("Thiết bị còn phiếu sửa chữa chưa đóng, không thể chuyển sang \"Không khả dụng\".");

            tb.TrangThai = trangThaiMoi;
            await _db.SaveChangesAsync();

            return KetQuaDichVu.Ok(trangThaiMoi == TrangThaiThietBiEnum.KhaDung
                ? $"Thiết bị {tb.TenThietBi} đã khả dụng trở lại."
                : $"Đã ngừng nhận sửa thiết bị {tb.TenThietBi}.");
        }

        // ---------- Xóa an toàn: đã có phiếu sửa chữa thì không xóa ----------
        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return KetQuaDichVu.KhongTimThay();

            if (await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id))
                return KetQuaDichVu.Loi("Không thể xóa: thiết bị đã có phiếu sửa chữa. Hãy chuyển sang \"Không khả dụng\" để ngừng sử dụng.");

            _db.ThietBis.Remove(tb);
            await _db.SaveChangesAsync();
            return KetQuaDichVu.Ok($"Đã xóa thiết bị {tb.TenThietBi}.");
        }
    }
}
