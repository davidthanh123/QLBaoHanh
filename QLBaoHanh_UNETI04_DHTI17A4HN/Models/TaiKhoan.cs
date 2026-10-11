// Họ và tên: ........................ (người phụ trách Module 1 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity TaiKhoan - tài khoản, đăng nhập, phân quyền.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 1 (Vân): Tài khoản - Đăng nhập - Phân quyền
    // Các trường hệ thống (SoLanDangNhapSai, KhoaDenNgay, NgayTaoTaiKhoan, LanDangNhapCuoi) do code gán,
    // không cho người dùng nhập từ form: dùng ViewModel hoặc [Bind] khi Create/Edit.
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        // Lưu bản băm (PasswordHasher<TaiKhoan>), không lưu plain text
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        // Chỉ dùng nếu nhóm tự viết hàm băm; để null khi dùng PasswordHasher<T>
        [StringLength(255)]
        public string? MatKhauSalt { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public VaiTroEnum VaiTro { get; set; } = VaiTroEnum.KhachHang;

        // true = hoạt động, false = bị khóa vĩnh viễn (do Admin)
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Range(0, int.MaxValue)]
        [Display(Name = "Số lần đăng nhập sai")]
        public int SoLanDangNhapSai { get; set; }

        // Khóa tạm thời sau N lần sai
        [Display(Name = "Khóa đến ngày")]
        public DateTime? KhoaDenNgay { get; set; }

        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime NgayTaoTaiKhoan { get; set; } = DateTime.Now;

        [Display(Name = "Lần đăng nhập cuối")]
        public DateTime? LanDangNhapCuoi { get; set; }

        public KhachHang? KhachHang { get; set; }              // 1 - 0..1
        public KyThuatVien? KyThuatVien { get; set; }          // 1 - 0..1
        public ICollection<NhatKyDangNhap> NhatKyDangNhaps { get; set; } = new List<NhatKyDangNhap>();
        public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
    }
}
