// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - ghi nhận chẩn đoán, công sửa chữa và linh kiện sử dụng.

using System.ComponentModel.DataAnnotations;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class ThemChiTietSuaChuaViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Chọn kỹ thuật viên đang hoạt động.")]
        public int MaKyThuatVien { get; set; }

        public int? MaLinhKien { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int SoLuong { get; set; } = 1;

        [Range(0, 1000000000, ErrorMessage = "Chi phí công phải từ 0 trở lên.")]
        public decimal DonGiaCong { get; set; }

        [Required(ErrorMessage = "Nhập kết quả chẩn đoán hoặc nội dung sửa chữa.")]
        [StringLength(1000)]
        public string KetQuaChanDoan { get; set; } = string.Empty;

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}
