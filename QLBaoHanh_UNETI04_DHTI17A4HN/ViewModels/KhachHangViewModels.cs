// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - ViewModel hồ sơ cá nhân, form thêm/sửa và danh sách khách hàng (phân trang).

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class HoSoKhachHangViewModel
    {
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\+?[0-9]{9,15}$", ErrorMessage = "Số điện thoại phải có 9–15 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải gồm đúng 12 chữ số")]
        [Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [Display(Name = "Loại khách hàng")]
        public LoaiKhachHangEnum LoaiKhachHang { get; set; } = LoaiKhachHangEnum.CaNhan;

        [RegularExpression(@"^\d{10,14}$", ErrorMessage = "Mã số thuế phải có 10–14 chữ số")]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }
    }

    /// <summary>Form Admin/Nhân viên thêm hoặc sửa khách hàng. Trạng thái đổi bằng nút riêng, ngày tạo do hệ thống gán.</summary>
    public class KhachHangFormViewModel
    {
        public int MaKhachHang { get; set; }

        [Display(Name = "Tài khoản đăng nhập")]
        public int? MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\+?[0-9]{9,15}$", ErrorMessage = "Số điện thoại phải có 9–15 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải gồm đúng 12 chữ số")]
        [Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [Display(Name = "Loại khách hàng")]
        public LoaiKhachHangEnum LoaiKhachHang { get; set; } = LoaiKhachHangEnum.CaNhan;

        [RegularExpression(@"^\d{10,14}$", ErrorMessage = "Mã số thuế phải có 10–14 chữ số")]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public List<SelectListItem> TaiKhoanKhaDung { get; set; } = new();
    }

    public class KhachHangDanhSachViewModel
    {
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public LoaiKhachHangEnum? Loai { get; set; }
        public int Trang { get; set; } = 1;
        public int SoTrang { get; set; } = 1;
        public int TongSo { get; set; }
        public List<KhachHang> KhachHangs { get; set; } = new();
    }
}
