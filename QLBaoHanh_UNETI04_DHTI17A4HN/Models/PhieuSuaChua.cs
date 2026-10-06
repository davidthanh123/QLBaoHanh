using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class PhieuSuaChua
{
    [Key] public int MaPhieu { get; set; }
    [Required] public int MaThietBi { get; set; }
    [Required] public int MaKhachHang { get; set; }
    public DateTime NgayTiepNhan { get; set; } = DateTime.Now;
    [Required, StringLength(1000)] public string NoiDungLoi { get; set; } = "";
    [Required, StringLength(20)] public string TrangThai { get; set; } = "ChoXuLy"; // ChoXuLy | DangXuLy | HoanThanh | Huy | TuChoi
    [StringLength(20)] public string MucDo { get; set; } = "BinhThuong";
    public DateTime? HanDuKien { get; set; }
    public DateTime? NgayHoanThanh { get; set; }

    public ThietBi? ThietBi { get; set; }
    public KhachHang? KhachHang { get; set; }
    public ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
}