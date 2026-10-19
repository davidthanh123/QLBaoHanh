using System.ComponentModel.DataAnnotations;
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
}
