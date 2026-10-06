using System.ComponentModel.DataAnnotations;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models;

public class LinhKien
{
    [Key] public int MaLinhKien { get; set; }
    [Required, StringLength(150)] public string TenLinhKien { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal DonGia { get; set; }
    [Range(0, int.MaxValue)] public int SoLuongTon { get; set; }
    [StringLength(500)] public string? MoTa { get; set; }
    public bool TrangThai { get; set; } = true;
}