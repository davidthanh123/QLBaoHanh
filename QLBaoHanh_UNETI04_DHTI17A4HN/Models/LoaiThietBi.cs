using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class LoaiThietBi
{
    [Key] public int MaLoai { get; set; }
    [Required, StringLength(100)] public string TenLoai { get; set; } = "";
    [StringLength(100)] public string? HangSanXuat { get; set; }
    [StringLength(500)] public string? MoTa { get; set; }
    [Range(0, 120)] public int ThoiHanBaoHanhMacDinh { get; set; } // tháng
    public bool TrangThai { get; set; } = true;
    public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
}