using System.ComponentModel.DataAnnotations;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 1 (Vân): Dữ liệu nền - Loại thiết bị
    public class LoaiThietBi
    {
        [Key]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Tên loại không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên loại")]
        public string TenLoai { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        [Required(ErrorMessage = "Hãng sản xuất không được để trống")]
        [StringLength(100)]
        [Display(Name = "Hãng sản xuất")]
        public string HangSanXuat { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // Đơn vị: tháng
        [Range(0, 120, ErrorMessage = "Thời hạn bảo hành phải từ 0 đến 120 tháng")]
        [Display(Name = "Thời hạn bảo hành mặc định (tháng)")]
        public int ThoiHanBaoHanhMacDinh { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
    }
}
