using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class ChiTietSuaChua
{
    [Key] public int MaChiTiet { get; set; }
    [Required] public int MaPhieu { get; set; }
    [Required] public int MaKyThuatVien { get; set; } // FK -> TaiKhoan (vai trò NhanVien)
    public int? MaLinhKien { get; set; }
    [Range(1, int.MaxValue)] public int SoLuong { get; set; } = 1;
    [Range(0, double.MaxValue)] public decimal DonGia { get; set; }
    [Range(0, double.MaxValue)] public decimal ThanhTien { get; set; }
    [StringLength(1000)] public string? KetQuaChanDoan { get; set; }
    [StringLength(500)] public string? GhiChu { get; set; }

    public PhieuSuaChua? PhieuSuaChua { get; set; }
    public TaiKhoan? KyThuatVien { get; set; }
    public LinhKien? LinhKien { get; set; }
}