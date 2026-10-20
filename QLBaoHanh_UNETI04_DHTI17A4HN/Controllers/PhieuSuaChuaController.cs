// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - tiếp nhận, xử lý và quản lý trạng thái phiếu sửa chữa.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Cả Controller: chỉ cần đã đăng nhập (Khách hàng dùng Index/Create/Huy).
    // Các action của Admin/Nhân viên bên dưới có [PhanQuyen(Admin, NhanVien)] riêng.
    [PhanQuyen]
    public class PhieuSuaChuaController : Controller
    {
        private const int KichThuocTrang = 10;
        private readonly AppDbContext _db;

        public PhieuSuaChuaController(AppDbContext db) => _db = db;

        // Danh sách xử lý: tìm khách hàng/thiết bị, lọc loại, trạng thái, ngày,
        // sắp xếp và phân trang trực tiếp trên truy vấn EF Core.
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maLoai,
            TrangThaiPhieuEnum? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string sapXep = "moi-nhat",
            int trang = 1)
        {
            if (trang < 1) trang = 1;
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
                (tuNgay, denNgay) = (denNgay, tuNgay);

            var query = _db.PhieuSuaChuas.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .AsQueryable();

            // Khách hàng chỉ xem phiếu của chính mình; Admin/Nhân viên xem toàn hệ thống.
            if (HttpContext.Session.LayVaiTro() == VaiTroEnum.KhachHang)
            {
                var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
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
                    TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng.";
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaSach = tuKhoa.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(tuKhoaSach) ||
                    p.KhachHang.SoDienThoai.Contains(tuKhoaSach) ||
                    p.ThietBi!.TenThietBi.Contains(tuKhoaSach) ||
                    p.ThietBi.SerialNumber.Contains(tuKhoaSach));
            }

            if (maLoai.HasValue)
                query = query.Where(p => p.ThietBi != null && p.ThietBi.MaLoai == maLoai.Value);
            if (trangThai.HasValue)
                query = query.Where(p => p.TrangThai == trangThai.Value);
            if (tuNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan >= tuNgay.Value.Date);
            if (denNgay.HasValue)
            {
                var mocKetThuc = denNgay.Value.Date.AddDays(1);
                query = query.Where(p => p.NgayTiepNhan < mocKetThuc);
            }

            query = sapXep switch
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
                TuKhoa = tuKhoa,
                MaLoai = maLoai,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SapXep = sapXep,
                Trang = trang,
                SoTrang = soTrang,
                TongSoPhieu = tongSoPhieu,
                PhieuSuaChuas = await query.Skip((trang - 1) * KichThuocTrang)
                    .Take(KichThuocTrang).ToListAsync()
            };

            ViewBag.LoaiThietBis = await _db.LoaiThietBis.AsNoTracking()
                .OrderBy(l => l.TenLoai).ToListAsync();
            return View(model);
        }

        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> Details(int id)
        {
            var phieu = await _db.PhieuSuaChuas.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.TaiKhoanTiepNhan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.KyThuatVien)
                    .ThenInclude(k => k!.TaiKhoan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .Include(p => p.LichSus).ThenInclude(l => l.TaiKhoanThucHien)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();
            ViewBag.ConBaoHanh = phieu.ThietBi != null &&
                phieu.ThietBi.NgayMua.Date <= phieu.NgayTiepNhan.Date &&
                phieu.ThietBi.HanBaoHanh.Date >= phieu.NgayTiepNhan.Date;
            if (phieu.TrangThai == TrangThaiPhieuEnum.DangXuLy)
            {
                ViewBag.KyThuatViens = await _db.KyThuatViens.AsNoTracking()
                    .Include(k => k.TaiKhoan)
                    .Where(k => k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai)
                    .OrderBy(k => k.HoTen).ToListAsync();
                ViewBag.LinhKiens = await _db.LinhKiens.AsNoTracking()
                    .Where(l => l.TrangThai && l.SoLuongTon > 0)
                    .OrderBy(l => l.TenLinhKien).ToListAsync();
            }
            return View(phieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> ThemChiTietSuaChua(int id, ThemChiTietSuaChuaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Loi"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id });
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            var phieu = await _db.PhieuSuaChuas.FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();
            if (phieu.TrangThai != TrangThaiPhieuEnum.DangXuLy)
            {
                TempData["Loi"] = "Chỉ được ghi nhận chẩn đoán và chi phí khi phiếu đang xử lý.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var kyThuatVien = await _db.KyThuatViens
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKyThuatVien == model.MaKyThuatVien && k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai);
            if (kyThuatVien == null)
            {
                TempData["Loi"] = "Kỹ thuật viên không tồn tại hoặc đã ngừng hoạt động.";
                return RedirectToAction(nameof(Details), new { id });
            }

            LinhKien? linhKien = null;
            var soLuong = 1;
            decimal donGia;
            if (model.MaLinhKien.HasValue)
            {
                linhKien = await _db.LinhKiens.FirstOrDefaultAsync(l => l.MaLinhKien == model.MaLinhKien.Value && l.TrangThai);
                if (linhKien == null || linhKien.SoLuongTon < model.SoLuong)
                {
                    TempData["Loi"] = "Linh kiện không khả dụng hoặc tồn kho không đủ số lượng yêu cầu.";
                    return RedirectToAction(nameof(Details), new { id });
                }
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
                MaPhieu = id,
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
            TempData["ThanhCong"] = "Đã ghi nhận chẩn đoán/chi phí sửa chữa.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiPhieuEnum trangThaiMoi, string? ghiChu)
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (!maTaiKhoan.HasValue) return Forbid();

            await using var transaction = await _db.Database.BeginTransactionAsync();
            var phieu = await _db.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();

            var trangThaiCu = phieu.TrangThai;
            if (!ChuyenTrangThaiHopLe(trangThaiCu, trangThaiMoi))
            {
                TempData["Loi"] = "Luồng chuyển trạng thái không hợp lệ hoặc phiếu đã kết thúc.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var laTuChoiHoacHuy = trangThaiMoi is TrangThaiPhieuEnum.Huy or TrangThaiPhieuEnum.TuChoi;
            if (laTuChoiHoacHuy && string.IsNullOrWhiteSpace(ghiChu))
            {
                TempData["Loi"] = "Vui lòng nhập lý do hủy hoặc từ chối phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (ghiChu?.Length > 500)
            {
                TempData["Loi"] = "Ghi chú không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (phieu.ThietBi == null || phieu.ThietBi.MaKhachHang != phieu.MaKhachHang)
            {
                TempData["Loi"] = "Không thể xử lý: thiết bị không tồn tại hoặc không thuộc khách hàng trên phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var khachHangHoatDong = await _db.KhachHangs.AnyAsync(k =>
                k.MaKhachHang == phieu.MaKhachHang && k.TrangThai);
            if (!khachHangHoatDong)
            {
                TempData["Loi"] = "Không thể tiếp nhận phiếu của khách hàng không còn hoạt động.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (trangThaiMoi == TrangThaiPhieuEnum.DangXuLy)
            {
                if (phieu.ThietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                {
                    TempData["Loi"] = "Thiết bị hiện không khả dụng để tiếp nhận sửa chữa.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // LINQ kiểm soát khả năng đáp ứng: mỗi thiết bị chỉ có tối đa một phiếu đang mở.
                var daCoPhieuDangMo = await _db.PhieuSuaChuas.AnyAsync(p =>
                    p.MaThietBi == phieu.MaThietBi && p.MaPhieu != phieu.MaPhieu &&
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));
                if (daCoPhieuDangMo)
                {
                    TempData["Loi"] = "Thiết bị đã có phiếu khác đang chờ hoặc đang xử lý.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                phieu.MaTaiKhoanTiepNhan = maTaiKhoan.Value;
                phieu.ThietBi.TrangThai = TrangThaiThietBiEnum.DangSuaChua;
            }

            if (trangThaiMoi == TrangThaiPhieuEnum.HoanThanh)
            {
                var coChiTiet = await _db.ChiTietSuaChuas.AnyAsync(c => c.MaPhieu == id);
                if (!coChiTiet)
                {
                    TempData["Loi"] = "Chưa thể hoàn thành: phiếu cần có ít nhất một chi tiết sửa chữa/kết quả.";
                    return RedirectToAction(nameof(Details), new { id });
                }
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
                MaTaiKhoanThucHien = maTaiKhoan.Value,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = trangThaiMoi,
                ThoiGian = DateTime.Now,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim()
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["ThanhCong"] = "Đã cập nhật trạng thái phiếu và lưu lịch sử xử lý.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var kh = await LayKhachHangHienTaiAsync();
            if (kh == null)
            {
                TempData["Loi"] = "Tài khoản chưa có hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var model = new TaoPhieuSuaChuaViewModel();
            await NapThietBiAsync(model, kh.MaKhachHang);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoPhieuSuaChuaViewModel model)
        {
            var kh = await LayKhachHangHienTaiAsync();
            if (kh == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ khách hàng đang hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var homNay = DateTime.Today;
            if (model.HanDuKien.HasValue && model.HanDuKien.Value.Date < homNay)
                ModelState.AddModelError(nameof(model.HanDuKien), "Ngày hẹn dự kiến không được ở trong quá khứ.");

            ThietBi? thietBi = null;
            if (model.MaThietBi.HasValue)
            {
                thietBi = await _db.ThietBis.FirstOrDefaultAsync(t =>
                    t.MaThietBi == model.MaThietBi.Value &&
                    t.MaKhachHang == kh.MaKhachHang);
                if (thietBi == null)
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
                else if (thietBi.TrangThai != TrangThaiThietBiEnum.KhaDung)
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị hiện không ở trạng thái khả dụng để tiếp nhận sửa chữa.");
                else if (await _db.PhieuSuaChuas.AnyAsync(p =>
                    p.MaThietBi == thietBi.MaThietBi &&
                    (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy)))
                    ModelState.AddModelError(nameof(model.MaThietBi), "Thiết bị đang có phiếu chờ xử lý/đang sửa chữa. Không thể tạo phiếu trùng.");
            }

            if (!ModelState.IsValid)
            {
                await NapThietBiAsync(model, kh.MaKhachHang);
                return View(model);
            }

            // Các trường hệ thống không lấy từ form: mã khách, thời điểm nhận và trạng thái.
            var phieu = new PhieuSuaChua
            {
                MaThietBi = thietBi!.MaThietBi,
                MaKhachHang = kh.MaKhachHang,
                NgayTiepNhan = DateTime.Now,
                NoiDungLoi = model.NoiDungLoi.Trim(),
                MucDo = model.MucDo,
                HanDuKien = model.HanDuKien?.Date,
                TrangThai = TrangThaiPhieuEnum.ChoXuLy
            };

            _db.PhieuSuaChuas.Add(phieu);
            await _db.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã tạo phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}. Phiếu đang chờ tiếp nhận.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Huy(int id)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy)
            {
                TempData["Loi"] = "Chỉ được hủy phiếu ở trạng thái Chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            return View(new HuyPhieuViewModel { MaPhieu = phieu.MaPhieu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(HuyPhieuViewModel model)
        {
            var phieu = await LayPhieuCuaNguoiDungAsync(model.MaPhieu);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieuEnum.ChoXuLy)
            {
                TempData["Loi"] = "Phiếu không còn ở trạng thái Chờ xử lý nên không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) return View(model);

            phieu.TrangThai = TrangThaiPhieuEnum.Huy;
            phieu.LyDoHuyTuChoi = model.LyDo.Trim();
            await _db.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã hủy phiếu {UiHelper.Ma("PSC", phieu.MaPhieu)}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<KhachHang?> LayKhachHangHienTaiAsync()
        {
            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
            if (maTaiKhoan == null) return null;
            return await _db.KhachHangs.FirstOrDefaultAsync(k =>
                k.MaTaiKhoan == maTaiKhoan.Value && k.TrangThai);
        }

        private async Task<PhieuSuaChua?> LayPhieuCuaNguoiDungAsync(int id)
        {
            var vaiTro = HttpContext.Session.LayVaiTro();
            var query = _db.PhieuSuaChuas.Where(p => p.MaPhieu == id);
            if (vaiTro == VaiTroEnum.KhachHang)
            {
                var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan();
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

        private async Task NapThietBiAsync(TaoPhieuSuaChuaViewModel model, int maKhachHang)
        {
            model.ThietBiCoTheSua = await _db.ThietBis
                .AsNoTracking()
                .Where(t => t.MaKhachHang == maKhachHang && t.TrangThai == TrangThaiThietBiEnum.KhaDung)
                .OrderBy(t => t.TenThietBi)
                .Select(t => new SelectListItem
                {
                    Value = t.MaThietBi.ToString(),
                    Text = t.TenThietBi + " — SN: " + t.SerialNumber
                }).ToListAsync();
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
