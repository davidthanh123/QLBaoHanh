// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Entity KhachHang - quản lý khách hàng, hồ sơ cá nhân (Module 3).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 3 (David): Quản lý khách hàng - Hồ sơ cá nhân
    [Table("KhachHang")]
    public class KhachHang : IValidatableObject
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

        // DB có CHECK chỉ cho số và dấu '+', nên ngoài [Phone] còn ràng buộc bằng RegularExpression
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^\+?[0-9]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // Unique khi có giá trị: cấu hình trong AppDbContext
        [StringLength(12)]
        [Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Loại khách hàng")]
        public LoaiKhachHangEnum LoaiKhachHang { get; set; } = LoaiKhachHangEnum.CaNhan;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Chỉ dùng với LoaiKhachHang = DoanhNghiep
        [StringLength(14)]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        public TaiKhoan? TaiKhoan { get; set; }   // quan hệ 1 - 0..1 cấu hình trong AppDbContext
        public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
        public ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(MaSoThue) && LoaiKhachHang != LoaiKhachHangEnum.DoanhNghiep)
            {
                yield return new ValidationResult(
                    "Mã số thuế chỉ dùng cho khách hàng doanh nghiệp",
                    new[] { nameof(MaSoThue) });
            }
        }
    }
}
