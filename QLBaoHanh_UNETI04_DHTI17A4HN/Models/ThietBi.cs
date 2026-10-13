// Họ và tên: ........................ (người phụ trách Module 2 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity ThietBi - quản lý thiết bị.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 2 (Hoàng Thành): Quản lý thiết bị
    // "Còn/hết bảo hành" KHÔNG lưu cột: tính từ HanBaoHanh bằng LINQ (Doc/THIETKE.md mục 2.1).
    [Table("ThietBi")]
    public class ThietBi : IValidatableObject
    {
        [Key]
        public int MaThietBi { get; set; }

        [Display(Name = "Loại thiết bị")]
        public int MaLoai { get; set; }

        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Serial number không được để trống")]
        [StringLength(100)]
        [Display(Name = "Serial number")]
        public string SerialNumber { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên thiết bị")]
        public string TenThietBi { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày mua")]
        public DateTime NgayMua { get; set; }

        // Gợi ý khi thêm mới: NgayMua + LoaiThietBi.ThoiHanBaoHanhMacDinh (tháng), vẫn cho sửa tay
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Hạn bảo hành")]
        public DateTime HanBaoHanh { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiThietBiEnum TrangThai { get; set; } = TrangThaiThietBiEnum.KhaDung;

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HanBaoHanh.Date < NgayMua.Date)
            {
                yield return new ValidationResult(
                    "Hạn bảo hành phải lớn hơn hoặc bằng ngày mua",
                    new[] { nameof(HanBaoHanh) });
            }
        }
    }
}
