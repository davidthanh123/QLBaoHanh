// Họ và tên: ........................ (người phụ trách Module 4 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity KyThuatVien - hồ sơ kỹ thuật viên.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 4 (Thảo): Hồ sơ kỹ thuật viên, gắn 1-1 với tài khoản có vai trò NhanVien.
    // ChiTietSuaChua.MaKyThuatVien trỏ vào bảng này (không trỏ thẳng TaiKhoan).
    [Table("KyThuatVien")]
    public class KyThuatVien
    {
        [Key]
        public int MaKyThuatVien { get; set; }

        // Bắt buộc, unique: mỗi tài khoản NhanVien chỉ có 1 hồ sơ kỹ thuật viên
        [Display(Name = "Tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Chuyên môn")]
        public string? ChuyenMon { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        public string? Email { get; set; }

        // false = ngừng hoạt động, không nhận thêm việc
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}
