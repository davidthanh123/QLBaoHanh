// Họ và tên: Đào Hoàng Thành
// Mã sinh viên: 23103100223
// Nội dung thực hiện: Module 2 - Quản lý thiết bị: danh sách (tìm kiếm, lọc, sắp xếp, phân trang), thêm, sửa, chi tiết, đổi trạng thái, xóa có kiểm soát.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    [PhanQuyen] // đã đăng nhập; từng action quản lý có [PhanQuyen] riêng
    public class ThietBiController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly AppDbContext _db;

        public ThietBiController(AppDbContext db)
        {
            _db = db;
        }

        // ---------- Hàm dùng chung ----------
        private bool LaKhachHang => HttpContext.Session.LayVaiTro() == VaiTroEnum.KhachHang;

        // Khách hàng chỉ thấy thiết bị của mình: tìm hồ sơ KhachHang theo tài khoản đang đăng nhập
        private async Task<int?> LayMaKhachHangHienTaiAsync()
        {
            var maTk = HttpContext.Session.LayMaTaiKhoan();
            if (maTk == null) return null;
            return await _db.KhachHangs.AsNoTracking()
                .Where(k => k.MaTaiKhoan == maTk)
                .Select(k => (int?)k.MaKhachHang)
                .FirstOrDefaultAsync();
        }

        private async Task NapDanhSachChonAsync(int? maLoaiHienTai = null, int? maKhachHangHienTai = null)
        {
            // Chỉ cho chọn loại/khách đang hoạt động; riêng giá trị hiện tại của bản ghi đang sửa vẫn giữ lại
            ViewBag.DsLoai = await _db.LoaiThietBis.AsNoTracking()
                .Where(l => l.TrangThai || l.MaLoai == maLoaiHienTai)
                .OrderBy(l => l.TenLoai).ToListAsync();
            ViewBag.DsKhachHang = await _db.KhachHangs.AsNoTracking()
                .Where(k => k.TrangThai || k.MaKhachHang == maKhachHangHienTai)
                .OrderBy(k => k.HoTen).ToListAsync();
        }

        // Quy tắc nghiệp vụ không biểu diễn được bằng Annotation (HanBaoHanh >= NgayMua đã có trong Entity)
        private async Task KiemTraNghiepVuAsync(ThietBi tb, ThietBi? goc)
        {
            if (await _db.ThietBis.AnyAsync(t => t.SerialNumber == tb.SerialNumber && t.MaThietBi != tb.MaThietBi))
                ModelState.AddModelError(nameof(tb.SerialNumber), "Serial number đã tồn tại");

            if ((goc == null || tb.MaLoai != goc.MaLoai)
                && !await _db.LoaiThietBis.AnyAsync(l => l.MaLoai == tb.MaLoai && l.TrangThai))
                ModelState.AddModelError(nameof(tb.MaLoai), "Vui lòng chọn loại thiết bị đang sử dụng");

            if ((goc == null || tb.MaKhachHang != goc.MaKhachHang)
                && !await _db.KhachHangs.AnyAsync(k => k.MaKhachHang == tb.MaKhachHang && k.TrangThai))
                ModelState.AddModelError(nameof(tb.MaKhachHang), "Vui lòng chọn khách hàng đang hoạt động");
        }

        // ---------- Danh sách: tìm kiếm + lọc + sắp xếp + phân trang ----------
        public async Task<IActionResult> Index(ThietBiFilter f)
        {
            var q = _db.ThietBis.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .AsQueryable();

            if (LaKhachHang)
            {
                var maKh = await LayMaKhachHangHienTaiAsync();
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

            var vm = new ThietBiIndexViewModel
            {
                Filter = f,
                TotalItems = tong,
                TotalPages = soTrang,
                PageSize = KichThuocTrang,
                Items = await q.Skip((f.Page - 1) * KichThuocTrang).Take(KichThuocTrang).ToListAsync(),
                DsLoai = await _db.LoaiThietBis.AsNoTracking().OrderBy(l => l.TenLoai).ToListAsync(),
                DsHang = await _db.LoaiThietBis.AsNoTracking().Select(l => l.HangSanXuat).Distinct().OrderBy(h => h).ToListAsync()
            };
            return View(vm);
        }

        // ---------- Chi tiết ----------
        public async Task<IActionResult> Details(int id)
        {
            var tb = await _db.ThietBis.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .Include(t => t.PhieuSuaChuas)
                .FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();

            // Khách hàng đổi mã trên URL để xem thiết bị người khác: không lộ dữ liệu
            if (LaKhachHang && tb.MaKhachHang != await LayMaKhachHangHienTaiAsync())
                return NotFound();

            return View(tb);
        }

        // ---------- Thêm ----------
        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachChonAsync();
            return View(new ThietBi
            {
                NgayMua = DateTime.Today,
                HanBaoHanh = DateTime.Today.AddMonths(12)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Create(
            [Bind("MaLoai,MaKhachHang,SerialNumber,TenThietBi,NgayMua,HanBaoHanh,MoTa,GhiChu,GiaTri")] ThietBi model)
        {
            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            await KiemTraNghiepVuAsync(model, null);

            if (!ModelState.IsValid)
            {
                await NapDanhSachChonAsync();
                return View(model);
            }

            model.TrangThai = TrangThaiThietBiEnum.KhaDung;
            _db.ThietBis.Add(model);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(model.SerialNumber), "Serial number đã tồn tại");
                await NapDanhSachChonAsync();
                return View(model);
            }

            TempData["ThanhCong"] = $"Đã thêm thiết bị {model.TenThietBi}.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Sửa ----------
        [HttpGet]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id)
        {
            var tb = await _db.ThietBis.AsNoTracking().FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();

            await NapDanhSachChonAsync(tb.MaLoai, tb.MaKhachHang);
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaThietBi,MaLoai,MaKhachHang,SerialNumber,TenThietBi,NgayMua,HanBaoHanh,MoTa,GhiChu,GiaTri")] ThietBi model)
        {
            if (id != model.MaThietBi) return BadRequest();

            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();

            model.SerialNumber = (model.SerialNumber ?? "").Trim();
            model.TenThietBi = (model.TenThietBi ?? "").Trim();
            await KiemTraNghiepVuAsync(model, tb);

            // Đã có phiếu sửa chữa thì không đổi chủ sở hữu (bảo toàn lịch sử)
            if (model.MaKhachHang != tb.MaKhachHang && await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id))
                ModelState.AddModelError(nameof(model.MaKhachHang), "Thiết bị đã có phiếu sửa chữa, không thể đổi khách hàng");

            if (!ModelState.IsValid)
            {
                await NapDanhSachChonAsync(tb.MaLoai, tb.MaKhachHang);
                return View(model);
            }

            // Chỉ cập nhật các trường cho phép sửa (TrangThai đổi ở action riêng)
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
                ModelState.AddModelError(nameof(model.SerialNumber), "Serial number đã tồn tại");
                await NapDanhSachChonAsync(tb.MaLoai, tb.MaKhachHang);
                return View(model);
            }

            TempData["ThanhCong"] = $"Đã cập nhật thiết bị {tb.TenThietBi}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------- Đổi trạng thái: chỉ KhaDung <-> KhongKhaDung ----------
        // DangSuaChua do luồng phiếu (Module 4) tự đặt/gỡ, không đổi tay ở đây.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> DoiTrangThai(int id, TrangThaiThietBiEnum trangThaiMoi)
        {
            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();

            if (trangThaiMoi == TrangThaiThietBiEnum.DangSuaChua)
            {
                TempData["Loi"] = "Trạng thái \"Đang sửa chữa\" do hệ thống đặt khi nhân viên tiếp nhận phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (tb.TrangThai == TrangThaiThietBiEnum.DangSuaChua)
            {
                TempData["Loi"] = "Thiết bị đang được sửa chữa, không thể đổi trạng thái.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (trangThaiMoi == TrangThaiThietBiEnum.KhongKhaDung
                && await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id
                    && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)))
            {
                TempData["Loi"] = "Thiết bị còn phiếu sửa chữa chưa đóng, không thể chuyển sang \"Không khả dụng\".";
                return RedirectToAction(nameof(Details), new { id });
            }

            tb.TrangThai = trangThaiMoi;
            await _db.SaveChangesAsync();

            TempData["ThanhCong"] = trangThaiMoi == TrangThaiThietBiEnum.KhaDung
                ? $"Thiết bị {tb.TenThietBi} đã khả dụng trở lại."
                : $"Đã ngừng nhận sửa thiết bị {tb.TenThietBi}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------- Xóa an toàn (Admin): đã có phiếu sửa chữa thì không xóa ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin)]
        public async Task<IActionResult> Xoa(int id)
        {
            var tb = await _db.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();

            if (await _db.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == id))
            {
                TempData["Loi"] = "Không thể xóa: thiết bị đã có phiếu sửa chữa. Hãy chuyển sang \"Không khả dụng\" để ngừng sử dụng.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.ThietBis.Remove(tb);
            await _db.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xóa thiết bị {tb.TenThietBi}.";
            return RedirectToAction(nameof(Index));
        }
    }
}