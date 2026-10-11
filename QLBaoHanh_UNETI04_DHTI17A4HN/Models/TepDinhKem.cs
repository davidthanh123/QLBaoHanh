// Họ và tên: ........................ (người phụ trách Module 2 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity TepDinhKem - tệp đính kèm dùng chung (nâng cao).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 2 (Hoàng Thành): Tệp đính kèm dùng chung cho ThietBi / PhieuSuaChua / ChiTietSuaChua.
    // Tham chiếu đa hình theo (LoaiDoiTuong, MaDoiTuong), nên KHÔNG có FK thật tới các bảng đó.
    [Table("TepDinhKem")]
    public class TepDinhKem
    {
        [Key]
        public int MaTepDinhKem { get; set; }

        [Display(Name = "Loại đối tượng")]
        public LoaiDoiTuongDinhKemEnum LoaiDoiTuong { get; set; }

        [Display(Name = "Mã đối tượng")]
        public int MaDoiTuong { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Đường dẫn file")]
        public string DuongDanFile { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Tên file gốc")]
        public string TenFileGoc { get; set; } = string.Empty;

        [Display(Name = "Ngày upload")]
        public DateTime NgayUpload { get; set; } = DateTime.Now;

        [Display(Name = "Người upload")]
        public int MaTaiKhoanUpload { get; set; }

        [ForeignKey(nameof(MaTaiKhoanUpload))]
        public TaiKhoan? TaiKhoanUpload { get; set; }
    }
}
