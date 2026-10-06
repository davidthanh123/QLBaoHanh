using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class KhachHang
{
    [Key] public int MaKhachHang { get; set; }
    public int? MaTaiKhoan { get; set; }
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [Phone, StringLength(15)] public string? SoDienThoai { get; set; }
    [EmailAddress, StringLength(100)] public string? Email { get; set; }
    [StringLength(200)] public string? DiaChi { get; set; }
    [StringLength(12)] public string? CCCD { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
    [StringLength(30)] public string? LoaiKhachHang { get; set; }
    public bool TrangThai { get; set; } = true;
    [StringLength(500)] public string? GhiChu { get; set; }
    [StringLength(20)] public string? MaSoThue { get; set; }

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
    public ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
}