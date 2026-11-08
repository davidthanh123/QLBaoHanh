using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    public class TaoPhieuSuaChuaViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
        [Display(Name = "Thiết bị cần sửa")]
        public int? MaThietBi { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả lỗi")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Nội dung lỗi phải từ 5 đến 1000 ký tự")]
        [Display(Name = "Mô tả tình trạng / lỗi")]
        public string NoiDungLoi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn mức độ")]
        [Display(Name = "Mức độ ưu tiên")]
        public MucDoEnum MucDo { get; set; } = MucDoEnum.TrungBinh;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn dự kiến (không bắt buộc)")]
        public DateTime? HanDuKien { get; set; }

        public List<SelectListItem> ThietBiCoTheSua { get; set; } = new();
    }

    public class HuyPhieuViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Lý do hủy phải từ 3 đến 500 ký tự")]
        [Display(Name = "Lý do hủy")]
        public string LyDo { get; set; } = "";
    }
}
