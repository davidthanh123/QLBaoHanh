using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class ChiTietSuaChuaController : Controller
    {
        // Controller này dùng khóa TempData "Success"/"Error" (khác "ThanhCong"/"Loi" của các module khác).
        private const string KhoaThanhCong = "Success";
        private const string KhoaLoi = "Error";

        private readonly IChiTietSuaChuaService _service;

        public ChiTietSuaChuaController(IChiTietSuaChuaService service) => _service = service;

        // Nạp 3 dropdown (Phiếu, Kỹ thuật viên, Linh kiện) vào ViewData; Edit còn cần bảng giá linh kiện cho JavaScript.
        // Mỗi màn hình lọc linh kiện và hiển thị cột Phiếu khác nhau nên truyền vào từng lần gọi.
        private async Task NapDanhMucAsync(
            ChiTietSuaChua? dangChon, LocLinhKien locLinhKien, string cotHienThiPhieu, bool kemGiaLinhKien)
        {
            ViewData["MaPhieu"] = new SelectList(
                await _service.LayDsPhieuAsync(), "MaPhieu", cotHienThiPhieu, dangChon?.MaPhieu);

            ViewData["MaKyThuatVien"] = new SelectList(
                await _service.LayDsKyThuatVienAsync(), "MaKyThuatVien", "HoTen", dangChon?.MaKyThuatVien);

            ViewData["MaLinhKien"] = new SelectList(
                await _service.LayDsLinhKienAsync(locLinhKien, dangChon?.MaLinhKien),
                "MaLinhKien", "TenLinhKien", dangChon?.MaLinhKien);

            if (kemGiaLinhKien)
                ViewBag.GiaLinhKien = await _service.LayGiaLinhKienAsync();
        }

        // GET: ChiTietSuaChua
        public async Task<IActionResult> Index() => View(await _service.LayDanhSachAsync());

        // GET: ChiTietSuaChua/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var chiTiet = await _service.LayChiTietAsync(id.Value);
            if (chiTiet == null) return NotFound();
            return View(chiTiet);
        }

        // GET: ChiTietSuaChua/Create
        public async Task<IActionResult> Create()
        {
            await NapDanhMucAsync(null, LocLinhKien.TatCa, "MaPhieu", kemGiaLinhKien: false);
            return View(new ChiTietSuaChua { SoLuong = 1 });
        }

        // POST: ChiTietSuaChua/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,DonGia,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua chiTietSuaChua)
        {
            var ketQua = await _service.ThemAsync(chiTietSuaChua, ModelState.IsValid);
            if (!ketQua.ThanhCong)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapDanhMucAsync(chiTietSuaChua, LocLinhKien.DangHoatDongVaConHang, "NoiDungLoi", kemGiaLinhKien: false);
                return View(chiTietSuaChua);
            }

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }

        // GET: ChiTietSuaChua/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var chiTiet = await _service.LayDeSuaAsync(id.Value);
            if (chiTiet == null) return NotFound();

            await NapDanhMucAsync(chiTiet, LocLinhKien.DangHoatDong, "NoiDungLoi", kemGiaLinhKien: true);
            return View(chiTiet);
        }

        // POST: ChiTietSuaChua/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,DonGia,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua chiTietSuaChua)
        {
            if (id != chiTietSuaChua.MaChiTiet) return NotFound();

            var ketQua = await _service.CapNhatAsync(id, chiTietSuaChua, ModelState.IsValid);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();
            if (ketQua.Loai == LoaiKetQua.DuLieuKhongHopLe)
            {
                ketQua.GopLoiVaoModelState(ModelState);
                await NapDanhMucAsync(chiTietSuaChua, LocLinhKien.DangHoatDongConHangHoacDangChon, "NoiDungLoi", kemGiaLinhKien: true);
                return View(chiTietSuaChua);
            }

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }

        // GET: ChiTietSuaChua/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var chiTiet = await _service.LayChiTietAsync(id.Value);
            if (chiTiet == null) return NotFound();
            return View(chiTiet);
        }

        // POST: ChiTietSuaChua/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketQua = await _service.XoaAsync(id);
            if (ketQua.Loai == LoaiKetQua.KhongTimThay) return NotFound();

            this.DatThongBao(ketQua, KhoaThanhCong, KhoaLoi);
            return RedirectToAction(nameof(Index));
        }
    }
}
