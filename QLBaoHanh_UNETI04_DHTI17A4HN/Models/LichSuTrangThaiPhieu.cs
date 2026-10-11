// Họ và tên: ........................ (người phụ trách Module 4 điền)
// Mã sinh viên: ........................
// Nội dung thực hiện: Entity LichSuTrangThaiPhieu - lịch sử đổi trạng thái phiếu.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 4 (Thảo): Mỗi lần phiếu đổi trạng thái (kể cả lúc tạo mới) ghi 1 dòng vào đây.
    // Chỉ thêm, không có chức năng sửa/xóa.
    [Table("LichSuTrangThaiPhieu")]
    public class LichSuTrangThaiPhieu
    {
        [Key]
        public int MaLichSu { get; set; }

        [Display(Name = "Phiếu sửa chữa")]
        public int MaPhieu { get; set; }

        [Display(Name = "Người thực hiện")]
        public int MaTaiKhoanThucHien { get; set; }

        // Null khi là bản ghi tạo phiếu mới
        [Display(Name = "Trạng thái cũ")]
        public TrangThaiPhieuEnum? TrangThaiCu { get; set; }

        [Display(Name = "Trạng thái mới")]
        public TrangThaiPhieuEnum TrangThaiMoi { get; set; }

        [Display(Name = "Thời gian")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [ForeignKey(nameof(MaPhieu))]
        public PhieuSuaChua? PhieuSuaChua { get; set; }

        [ForeignKey(nameof(MaTaiKhoanThucHien))]
        public TaiKhoan? TaiKhoanThucHien { get; set; }
    }
}
