using System.ComponentModel.DataAnnotations;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 3 (David): Quản lý khách hàng - Hồ sơ cá nhân
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        // Nullable: khách do nhân viên nhập trực tiếp có thể chưa có tài khoản
        [Display(Name = "Tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(12)]
        [Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Gợi ý: "Cá nhân", "Doanh nghiệp"
        [Required]
        [StringLength(30)]
        [Display(Name = "Loại khách hàng")]
        public string LoaiKhachHang { get; set; } = "Cá nhân";

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        public TaiKhoan? TaiKhoan { get; set; }   // quan hệ 1-1 cấu hình trong AppDbContext
        public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
        public ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}
