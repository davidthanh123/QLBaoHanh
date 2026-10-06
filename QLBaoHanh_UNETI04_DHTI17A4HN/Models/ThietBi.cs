// Họ và tên: Đào Hoàng Thành
// Mã sinh viên: 23103100223
// Nội dung thực hiện: Entity ThietBi và kiểm tra dữ liệu (Module 2 - Quản lý thiết bị).
using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class ThietBi : IValidatableObject
{
    [Key] public int MaThietBi { get; set; }

    [Display(Name = "Loại thiết bị")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại thiết bị.")]
    public int MaLoai { get; set; }

    [Display(Name = "Khách hàng")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng.")]
    public int MaKhachHang { get; set; }

    [Display(Name = "Số serial")]
    [Required(ErrorMessage = "Vui lòng nhập số serial."), StringLength(50)]
    public string SerialNumber { get; set; } = "";

    [Display(Name = "Tên thiết bị")]
    [Required(ErrorMessage = "Vui lòng nhập tên thiết bị."), StringLength(150)]
    public string TenThietBi { get; set; } = "";

    [Display(Name = "Ngày mua")]
    [Required(ErrorMessage = "Vui lòng nhập ngày mua."), DataType(DataType.Date)]
    public DateTime NgayMua { get; set; } = DateTime.Today;

    [Display(Name = "Hạn bảo hành")]
    [Required(ErrorMessage = "Vui lòng nhập hạn bảo hành."), DataType(DataType.Date)]
    public DateTime HanBaoHanh { get; set; } = DateTime.Today;

    [Display(Name = "Mô tả"), StringLength(500)]
    public string? MoTa { get; set; }

    [Display(Name = "Trạng thái")]
    [Required, RegularExpression("KhaDung|DangSua|NgungSuDung", ErrorMessage = "Trạng thái không hợp lệ.")]
    public string TrangThai { get; set; } = "KhaDung"; // KhaDung | DangSua | NgungSuDung

    [Display(Name = "Ghi chú"), StringLength(500)]
    public string? GhiChu { get; set; }

    [Display(Name = "Giá trị")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0.")]
    public decimal GiaTri { get; set; }

    public LoaiThietBi? LoaiThietBi { get; set; }
    public KhachHang? KhachHang { get; set; }
    public ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (NgayMua > DateTime.Today)
            yield return new ValidationResult("Ngày mua không được ở tương lai.", new[] { nameof(NgayMua) });
        if (HanBaoHanh < NgayMua)
            yield return new ValidationResult("Hạn bảo hành phải từ ngày mua trở đi.", new[] { nameof(HanBaoHanh) });
    }
}