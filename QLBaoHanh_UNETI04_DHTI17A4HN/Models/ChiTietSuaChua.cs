// Họ và tên: ........................ (người phụ trách Module 5 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity ChiTietSuaChua - kết quả chẩn đoán, linh kiện và chi phí sửa chữa.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 5 (Vũ): Kết quả sửa chữa - Linh kiện - Chi phí
    // Form thêm chi tiết nằm trong luồng sửa chữa của Module 4, nhưng file Entity này thuộc Module 5.
    // Tổng chi phí của phiếu KHÔNG lưu cột riêng: = SUM(ThanhTien) theo MaPhieu (Doc/THIETKE.md mục 2.1).
    [Table("ChiTietSuaChua")]
    public class ChiTietSuaChua
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Display(Name = "Phiếu sửa chữa")]
        public int MaPhieu { get; set; }

        // Trỏ vào hồ sơ KyThuatVien (không trỏ thẳng TaiKhoan)
        [Display(Name = "Kỹ thuật viên")]
        public int MaKyThuatVien { get; set; }

        // Null nếu dòng này chỉ tính công chẩn đoán/sửa, không dùng linh kiện
        [Display(Name = "Linh kiện")]
        public int? MaLinhKien { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        // KHÔNG cho người dùng nhập: Controller gán = SoLuong * DonGia trước SaveChanges (DB có CHECK đối chiếu)
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
        public KyThuatVien? KyThuatVien { get; set; }

        [ForeignKey(nameof(MaLinhKien))]
        public LinhKien? LinhKien { get; set; }
    }
}
