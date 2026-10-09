using System.ComponentModel.DataAnnotations;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 1 (Vân): Tài khoản - Đăng nhập - Phân quyền
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Giá trị: "Admin", "NhanVien", "KhachHang"
        [Required]
        [StringLength(30)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "KhachHang";

        // true = hoạt động, false = bị khóa
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public KhachHang? KhachHang { get; set; }
    }
}
