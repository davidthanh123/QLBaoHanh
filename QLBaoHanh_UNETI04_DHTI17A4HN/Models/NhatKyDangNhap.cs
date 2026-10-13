// Họ và tên: ........................ (người phụ trách Module 1 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity NhatKyDangNhap - nhật ký đăng nhập (nâng cao).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 1 (Vân): Nhật ký mọi lần thử đăng nhập. Chỉ thêm, không sửa/xóa.
    [Table("NhatKyDangNhap")]
    public class NhatKyDangNhap
    {
        [Key]
        public int MaNhatKy { get; set; }

        // Null khi nhập sai tên đăng nhập (tài khoản không tồn tại)
        [Display(Name = "Tài khoản")]
        public int? MaTaiKhoan { get; set; }

        // Lưu đúng giá trị người dùng gõ
        [Required]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập đã nhập")]
        public string TenDangNhapNhap { get; set; } = string.Empty;

        [Display(Name = "Thời gian")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [Display(Name = "Kết quả")]
        public KetQuaDangNhapEnum KetQua { get; set; }

        [StringLength(45)]
        [Display(Name = "Địa chỉ IP")]
        public string? DiaChiIP { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}
