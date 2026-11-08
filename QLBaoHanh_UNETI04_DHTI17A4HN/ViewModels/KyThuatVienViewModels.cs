// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - tạo và cập nhật hồ sơ kỹ thuật viên.

using System.ComponentModel.DataAnnotations;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class KyThuatVienViewModel
    {
        public int? MaKyThuatVien { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Chọn tài khoản nhân viên.")]
        [Display(Name = "Tài khoản nhân viên")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Chuyên môn")]
        public string? ChuyenMon { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150)]
        public string? Email { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}
