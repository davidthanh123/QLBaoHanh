// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - dữ liệu lọc và phân trang danh sách phiếu sửa chữa.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class PhieuSuaChuaDanhSachViewModel
    {
        public string? TuKhoa { get; set; }
        public int? MaLoai { get; set; }
        public TrangThaiPhieuEnum? TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string SapXep { get; set; } = "moi-nhat";
        public int Trang { get; set; }
        public int SoTrang { get; set; }
        public int TongSoPhieu { get; set; }
        public IList<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}
