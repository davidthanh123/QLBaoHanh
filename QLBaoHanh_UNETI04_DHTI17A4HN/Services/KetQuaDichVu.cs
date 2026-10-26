// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Kiểu kết quả dùng chung để Service báo lại cho Controller (thay cho TempData/RedirectToAction trong logic nghiệp vụ).

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public enum LoaiKetQua
    {
        ThanhCong,
        KhongTimThay,      // Controller trả NotFound()
        LoiNghiepVu,       // Vi phạm quy tắc nghiệp vụ -> Controller hiện ThongBao qua TempData
        DuLieuKhongHopLe   // Có lỗi theo từng ô nhập (LoiTruong) hoặc ModelState đã lỗi -> Controller trả lại View
    }

    /// <summary>Một lỗi gắn với một ô nhập. Truong = "" nghĩa là lỗi chung của form.</summary>
    public record LoiTruongNhap(string Truong, string ThongBao);

    public class KetQuaDichVu
    {
        public LoaiKetQua Loai { get; init; }
        public string? ThongBao { get; init; }
        public List<LoiTruongNhap> LoiTruong { get; init; } = new();
        public bool ThanhCong => Loai == LoaiKetQua.ThanhCong;

        public static KetQuaDichVu Ok(string? thongBao = null) =>
            new() { Loai = LoaiKetQua.ThanhCong, ThongBao = thongBao };

        public static KetQuaDichVu Loi(string thongBao) =>
            new() { Loai = LoaiKetQua.LoiNghiepVu, ThongBao = thongBao };

        public static KetQuaDichVu KhongTimThay() =>
            new() { Loai = LoaiKetQua.KhongTimThay };

        public static KetQuaDichVu DuLieuKhongHopLe(IEnumerable<LoiTruongNhap>? loi = null) =>
            new() { Loai = LoaiKetQua.DuLieuKhongHopLe, LoiTruong = loi?.ToList() ?? new() };

        /// <summary>Đính kèm thêm dữ liệu trả về cho Controller (vd: để nạp lại dropdown).</summary>
        public KetQuaDichVu<T> Kem<T>(T duLieu) => new()
        {
            Loai = Loai,
            ThongBao = ThongBao,
            LoiTruong = LoiTruong,
            DuLieu = duLieu
        };
    }

    public class KetQuaDichVu<T> : KetQuaDichVu
    {
        public T? DuLieu { get; init; }
    }
}
