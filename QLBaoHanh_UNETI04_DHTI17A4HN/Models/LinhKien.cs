// Họ và tên: Nguyễn Văn Vũ
// Mã sinh viên: 23103100213
// Nội dung thực hiện: Entity LinhKien - quản lý linh kiện.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    [Table("LinhKien")]
    public class LinhKien
    {
        [Key]
        public int MaLinhKien { get; set; }

        [Required(ErrorMessage = "Tên linh kiện không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên linh kiện")]
        public string TenLinhKien { get; set; } = string.Empty;   // unique: cấu hình trong AppDbContext

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(30)]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}
