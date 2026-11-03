// Họ và tên: Đào Hoàng Thành
// Mã sinh viên: 23103100223
// Nội dung thực hiện: Module 2 - ViewModel danh sách thiết bị (tìm kiếm, lọc, sắp xếp, phân trang).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels
{
    // Toàn bộ điều kiện tìm/lọc/sắp xếp/trang: bind từ query string, giữ nguyên khi chuyển trang.
    public class ThietBiFilter
    {
        public string? Keyword { get; set; }                 // tên thiết bị, serial, tên khách hàng
        public int? MaLoai { get; set; }
        public string? Hang { get; set; }                    // hãng sản xuất (LoaiThietBi.HangSanXuat)
        public string? BaoHanh { get; set; }                 // con | sap | het
        public TrangThaiThietBiEnum? TrangThai { get; set; } // khả dụng / đang sửa chữa / không khả dụng
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;
    }

    public class ThietBiIndexViewModel
    {
        public ThietBiFilter Filter { get; set; } = new();
        public List<ThietBi> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public List<LoaiThietBi> DsLoai { get; set; } = new();
        public List<string> DsHang { get; set; } = new();

        public bool CoLoc =>
            !string.IsNullOrWhiteSpace(Filter.Keyword) || Filter.MaLoai != null || !string.IsNullOrEmpty(Filter.Hang)
            || !string.IsNullOrEmpty(Filter.BaoHanh) || Filter.TrangThai != null
            || Filter.GiaTu != null || Filter.GiaDen != null;

        // Route values cho link phân trang: mang theo mọi điều kiện đang áp dụng
        public Dictionary<string, string> Route(int page)
        {
            var d = new Dictionary<string, string> { ["Page"] = page.ToString() };
            if (!string.IsNullOrWhiteSpace(Filter.Keyword)) d["Keyword"] = Filter.Keyword!;
            if (Filter.MaLoai != null) d["MaLoai"] = Filter.MaLoai.Value.ToString();
            if (!string.IsNullOrEmpty(Filter.Hang)) d["Hang"] = Filter.Hang!;
            if (!string.IsNullOrEmpty(Filter.BaoHanh)) d["BaoHanh"] = Filter.BaoHanh!;
            if (Filter.TrangThai != null) d["TrangThai"] = Filter.TrangThai.Value.ToString();
            if (Filter.GiaTu != null) d["GiaTu"] = Filter.GiaTu.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Filter.GiaDen != null) d["GiaDen"] = Filter.GiaDen.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(Filter.Sort)) d["Sort"] = Filter.Sort!;
            return d;
        }
    }
}