// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Hằng số khóa Session và hàm đọc thông tin đăng nhập từ Session.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Helpers
{
    /// <summary>
    /// Thông tin người đăng nhập lưu trong Session: MaTaiKhoan (int), HoTen, VaiTro (tên member của VaiTroEnum).
    /// Các module khác lấy người đang đăng nhập bằng HttpContext.Session.LayMaTaiKhoan() / LayVaiTro().
    /// </summary>
    public static class PhienDangNhap
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";

        public static bool DaDangNhap(this ISession session) => session.GetInt32(MaTaiKhoan) != null;

        public static int? LayMaTaiKhoan(this ISession session) => session.GetInt32(MaTaiKhoan);

        public static string? LayHoTen(this ISession session) => session.GetString(HoTen);

        public static VaiTroEnum? LayVaiTro(this ISession session)
        {
            var chuoi = session.GetString(VaiTro);
            if (chuoi != null && Enum.TryParse<VaiTroEnum>(chuoi, out var vaiTro))
                return vaiTro;
            return null;
        }

        // Ghi thông tin đăng nhập vào Session (gọi sau khi Session.Clear()).</summary>
        public static void GhiDangNhap(this ISession session, int maTaiKhoan, string hoTen, VaiTroEnum vaiTro)
        {
            session.SetInt32(MaTaiKhoan, maTaiKhoan);
            session.SetString(HoTen, hoTen);
            session.SetString(VaiTro, vaiTro.ToString());
        }
    }
}
