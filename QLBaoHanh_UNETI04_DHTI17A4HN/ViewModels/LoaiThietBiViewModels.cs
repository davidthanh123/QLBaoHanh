// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - ViewModel dòng danh sách Loại thiết bị (kèm số thiết bị đang dùng loại này).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class LoaiThietBiDongViewModel
    {
        public LoaiThietBi Loai { get; set; } = new();
        public int SoThietBi { get; set; }
    }
}
