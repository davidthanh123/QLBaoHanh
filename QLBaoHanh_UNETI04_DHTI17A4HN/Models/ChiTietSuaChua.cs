using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 5 (Vũ): Kết quả sửa chữa - Dashboard - Thống kê
    public class ChiTietSuaChua
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Display(Name = "Phiếu sửa chữa")]
        public int MaPhieu { get; set; }

        // Kỹ thuật viên = tài khoản có vai trò NhanVien
        [Display(Name = "Kỹ thuật viên")]
        public int MaKyThuatVien { get; set; }

        // Nullable: có phiếu chỉ tính công sửa, không dùng linh kiện
        [Display(Name = "Linh kiện")]
        public int? MaLinhKien { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        // Controller gán = SoLuong * DonGia
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; }

        [StringLength(1000)]
        [Display(Name = "Kết quả chẩn đoán")]
        public string? KetQuaChanDoan { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [ForeignKey(nameof(MaPhieu))]
        public PhieuSuaChua? PhieuSuaChua { get; set; }

        [ForeignKey(nameof(MaKyThuatVien))]
        public TaiKhoan? KyThuatVien { get; set; }

        [ForeignKey(nameof(MaLinhKien))]
        public LinhKien? LinhKien { get; set; }
    }
}
