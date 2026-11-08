// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - kiểu dữ liệu ra của luồng tạo/theo dõi phiếu (tách Service khỏi Controller).
//
// Họ và tên: [người phụ trách Module 4 điền]
// Mã sinh viên: [người phụ trách Module 4 điền]
// Nội dung thực hiện: Module 4 - bộ lọc và kết quả của danh sách/chi tiết phiếu xử lý.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    /// <summary>Điều kiện lọc/sắp xếp/phân trang của trang danh sách phiếu.</summary>
    public record PhieuSuaChuaBoLoc(
        string? TuKhoa,
        int? MaLoai,
        TrangThaiPhieuEnum? TrangThai,
        DateTime? TuNgay,
        DateTime? DenNgay,
        string SapXep = "moi-nhat",
        int Trang = 1);

    public class DanhSachPhieuKetQua
    {
        public required PhieuSuaChuaDanhSachViewModel Model { get; init; }
        public required List<LoaiThietBi> LoaiThietBis { get; init; }
        /// <summary>Có giá trị khi cần báo lỗi nhẹ (vd: tài khoản chưa có hồ sơ khách hàng).</summary>
        public string? CanhBao { get; init; }
    }

    public class ChiTietPhieuKetQua
    {
        public required PhieuSuaChua Phieu { get; init; }
        public bool ConBaoHanh { get; init; }
        /// <summary>Chỉ có dữ liệu khi phiếu đang ở trạng thái Đang xử lý.</summary>
        public List<KyThuatVien>? KyThuatViens { get; init; }
        public List<LinhKien>? LinhKiens { get; init; }
    }
}
