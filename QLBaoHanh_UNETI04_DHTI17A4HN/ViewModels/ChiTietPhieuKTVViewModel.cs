using System.Collections.Generic;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class ChiTietPhieuKTVViewModel
    {
        public PhieuSuaChua PhieuSuaChua { get; set; } = new PhieuSuaChua();
        public List<LichSuTrangThaiPhieu> LichSuTrangThais { get; set; } = new List<LichSuTrangThaiPhieu>();
    }
}
