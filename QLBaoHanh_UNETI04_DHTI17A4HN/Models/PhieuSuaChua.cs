using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 3 (David) tạo phiếu - MODULE 4 (Thảo) xử lý trạng thái
    // Quy ước: David là chủ file entity này, Thảo cần thêm cột thì nhắn David.
    public class PhieuSuaChua
    {
        [Key]
        public int MaPhieu { get; set; }

        [Display(Name = "Thiết bị")]
        public int MaThietBi { get; set; }

        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [Display(Name = "Ngày tiếp nhận")]
        public DateTime NgayTiepNhan { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Nội dung lỗi không được để trống")]
        [StringLength(1000)]
        [Display(Name = "Nội dung lỗi")]
        public string NoiDungLoi { get; set; } = string.Empty;

        // Luồng: "Chờ xử lý" -> "Đang xử lý" -> "Hoàn thành" (hoặc "Hủy")
        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chờ xử lý";

        // Gợi ý: "Thấp", "Trung bình", "Cao"
        [Required]
        [StringLength(30)]
        [Display(Name = "Mức độ")]
        public string MucDo { get; set; } = "Trung bình";

        [Display(Name = "Hạn dự kiến")]
        public DateTime? HanDuKien { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? NgayHoanThanh { get; set; }

        [ForeignKey(nameof(MaThietBi))]
        public ThietBi? ThietBi { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }

        public ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}
