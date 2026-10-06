using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class TaiKhoan
{
    [Key] public int MaTaiKhoan { get; set; }
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required, StringLength(100)] public string MatKhau { get; set; } = "";
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, StringLength(20)] public string VaiTro { get; set; } = "KhachHang"; // Admin | NhanVien | KhachHang
    public bool TrangThai { get; set; } = true; // false = bị khóa
    public KhachHang? KhachHang { get; set; }
}