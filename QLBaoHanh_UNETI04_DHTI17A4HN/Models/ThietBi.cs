using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 2 (Hoàng Thành): Quản lý thiết bị
    public class ThietBi
    {
        [Key]
        public int MaThietBi { get; set; }

        [Display(Name = "Loại thiết bị")]
        public int MaLoai { get; set; }

        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Serial number không được để trống")]
        [StringLength(50)]
        [Display(Name = "Serial number")]
        public string SerialNumber { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên thiết bị")]
        public string TenThietBi { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày mua")]
        public DateTime NgayMua { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hạn bảo hành")]
        public DateTime HanBaoHanh { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // Gợi ý: "Khả dụng", "Đang sửa chữa", "Ngừng sử dụng"
        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Khả dụng";

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá trị")]
        public decimal GiaTri { get; set; }

        [ForeignKey(nameof(MaLoai))]
        public LoaiThietBi? LoaiThietBi { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }

        public ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}
