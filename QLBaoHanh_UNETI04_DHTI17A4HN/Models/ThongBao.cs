// Họ và tên: ........................ (người phụ trách Module 5 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity ThongBao - thông báo trong hệ thống/email (nâng cao).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 5 (Vũ): Thông báo cho người dùng (vd: phiếu đổi trạng thái, thiết bị sắp hết bảo hành).
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        [Display(Name = "Kênh")]
        public KenhThongBaoEnum Kenh { get; set; } = KenhThongBaoEnum.TrongHeThong;

        [Display(Name = "Người nhận")]
        public int MaTaiKhoanNhan { get; set; }

        // Null nếu thông báo không gắn với phiếu nào
        [Display(Name = "Phiếu sửa chữa")]
        public int? MaPhieu { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [StringLength(1000)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày gửi")]
        public DateTime? NgayGui { get; set; }

        [Display(Name = "Trạng thái gửi")]
        public TrangThaiGuiThongBaoEnum TrangThai { get; set; } = TrangThaiGuiThongBaoEnum.ChoGui;

        [Display(Name = "Đã đọc")]
        public bool DaDoc { get; set; }

        [ForeignKey(nameof(MaTaiKhoanNhan))]
        public TaiKhoan? TaiKhoanNhan { get; set; }

        [ForeignKey(nameof(MaPhieu))]
        public PhieuSuaChua? PhieuSuaChua { get; set; }
    }
}
