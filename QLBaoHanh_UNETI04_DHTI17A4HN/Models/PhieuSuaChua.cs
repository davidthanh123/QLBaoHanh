// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Entity PhieuSuaChua - tạo phiếu, theo dõi, hủy phiếu sửa chữa (Module 3).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models
{
    // MODULE 3 (David): Tạo phiếu sửa chữa - Theo dõi - Hủy phiếu
    // MODULE 4 (Thảo): Tiếp nhận - cập nhật trạng thái. Cần thêm cột vào file này thì báo David.
    // Các trường hệ thống (TrangThai, NgayTiepNhan, MaTaiKhoanTiepNhan, NgayHoanThanh) do Controller gán,
    // không cho người dùng nhập từ form: dùng ViewModel hoặc [Bind] khi tạo phiếu.
    // Luồng chuyển trạng thái hợp lệ và điều kiện tạo phiếu: Doc/THIETKE.md mục 2.4, 2.5.
    [Table("PhieuSuaChua")]
    public class PhieuSuaChua : IValidatableObject
    {
        [Key]
        public int MaPhieu { get; set; }

        [Display(Name = "Thiết bị")]
        public int MaThietBi { get; set; }

        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        // Null khi khách tự đăng ký và chưa có nhân viên tiếp nhận
        [Display(Name = "Nhân viên tiếp nhận")]
        public int? MaTaiKhoanTiepNhan { get; set; }

        [Display(Name = "Ngày tiếp nhận")]
        public DateTime NgayTiepNhan { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Nội dung lỗi không được để trống")]
        [StringLength(1000)]
        [Display(Name = "Nội dung lỗi")]
        public string NoiDungLoi { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public TrangThaiPhieuEnum TrangThai { get; set; } = TrangThaiPhieuEnum.ChoXuLy;

        [Display(Name = "Mức độ")]
        public MucDoEnum MucDo { get; set; } = MucDoEnum.TrungBinh;

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Hạn dự kiến")]
        public DateTime? HanDuKien { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? NgayHoanThanh { get; set; }

        // Bắt buộc khi TrangThai = Huy hoặc TuChoi (DB có CHECK, Controller kiểm tra trước khi lưu)
        [StringLength(500)]
        [Display(Name = "Lý do hủy/từ chối")]
        public string? LyDoHuyTuChoi { get; set; }

        [ForeignKey(nameof(MaThietBi))]
        public ThietBi? ThietBi { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey(nameof(MaTaiKhoanTiepNhan))]
        public TaiKhoan? TaiKhoanTiepNhan { get; set; }

        public ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
        public ICollection<LichSuTrangThaiPhieu> LichSus { get; set; } = new List<LichSuTrangThaiPhieu>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HanDuKien.HasValue && HanDuKien.Value.Date < NgayTiepNhan.Date)
            {
                yield return new ValidationResult(
                    "Hạn dự kiến phải lớn hơn hoặc bằng ngày tiếp nhận",
                    new[] { nameof(HanDuKien) });
            }
        }
    }
}
