// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: ViewModel cho Trang chủ (Home/Index) - số liệu tóm tắt + phiếu gần đây theo vai trò.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class HomeViewModel
    {
        public VaiTroEnum VaiTro { get; set; }
        public string HoTen { get; set; } = string.Empty;

        // Khách hàng đăng nhập nhưng chưa có hồ sơ KhachHang -> View hiện thông báo thay vì số liệu
        public bool ThieuHoSoKhachHang { get; set; }

        // 4 ô KPI. Ý nghĩa đổi theo vai trò (xem HomeController):
        //  - Admin/Nhân viên: Chờ xử lý | Đang xử lý | Quá hạn | Khách hàng
        //  - Khách hàng:      Thiết bị của tôi | Phiếu đang mở | Sắp hết bảo hành | Đã hoàn thành
        public int So1 { get; set; }
        public int So2 { get; set; }
        public int So3 { get; set; }
        public int So4 { get; set; }

        // 5 phiếu mới nhất (đã Include ThietBi, KhachHang)
        public List<PhieuSuaChua> PhieuGanDay { get; set; } = new();

        // Chỉ dùng cho khách hàng: thiết bị còn <= 30 ngày bảo hành
        public List<ThietBi> ThietBiSapHetBaoHanh { get; set; } = new();
    }

    // Trang giới thiệu công khai (chưa đăng nhập): chỉ chứa thông tin không nhạy cảm
    public class GioiThieuViewModel
    {
        // Loại thiết bị đang tiếp nhận (LoaiThietBi.TrangThai = true)
        public List<LoaiThietBi> LoaiThietBis { get; set; } = new();
    }
}
