// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Filter phân quyền theo vai trò đọc từ Session, kiểm tra TẠI CONTROLLER (đề mục 5.4).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Helpers
{
    // Cách dùng ở Controller của mọi module:
    //   [PhanQuyen(VaiTroEnum.Admin)]                        chỉ Admin
    //  [PhanQuyen(VaiTroEnum.Admin, VaiTroEnum.NhanVien)]    Admin hoặc Nhân viên
    //  [PhanQuyen]                                           chỉ cần đã đăng nhập
    // Đặt trên class (cả Controller) hoặc trên từng Action. Action có [AllowAnonymous] thì được bỏ qua.
    // Chưa đăng nhập -> về trang đăng nhập; sai vai trò -> trang 403 (TaiKhoan/TuChoi).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class PhanQuyenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly VaiTroEnum[] _vaiTros;

        public PhanQuyenAttribute(params VaiTroEnum[] vaiTros)
        {
            _vaiTros = vaiTros;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
                return;

            var session = context.HttpContext.Session;
            var vaiTro = session.LayVaiTro();

            if (!session.DaDangNhap() || vaiTro == null)
            {
                context.Result = DangNhapFilter.VeTrangDangNhap(context.HttpContext);
                return;
            }

            if (_vaiTros.Length > 0 && !_vaiTros.Contains(vaiTro.Value))
            {
                context.Result = new RedirectToActionResult("TuChoi", "TaiKhoan", null);
            }
        }
    }

    // Filter toàn cục: mọi trang đều phải đăng nhập (trừ action có [AllowAnonymous]).
    // Đồng thời đối chiếu tài khoản trong CSDL: bị khóa/xóa thì hủy Session, đổi vai trò thì cập nhật Session.
    public sealed class DangNhapFilter : IAsyncAuthorizationFilter
    {
        private readonly ITaiKhoanRepository _taiKhoanRepo;

        public DangNhapFilter(ITaiKhoanRepository taiKhoanRepo)
        {
            _taiKhoanRepo = taiKhoanRepo;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
                return;

            // Trang lỗi chung của hệ thống luôn mở được
            var route = context.RouteData.Values;
            if (string.Equals(route["controller"]?.ToString(), "Home", StringComparison.OrdinalIgnoreCase)
                && string.Equals(route["action"]?.ToString(), "Error", StringComparison.OrdinalIgnoreCase))
                return;

            var session = context.HttpContext.Session;
            var ma = session.LayMaTaiKhoan();
            if (ma == null)
            {
                context.Result = VeTrangDangNhap(context.HttpContext);
                return;
            }

            var tk = await _taiKhoanRepo.LayThongTinXacThucAsync(ma.Value);

            if (tk == null || !tk.TrangThai)
            {
                session.Clear();
                context.Result = VeTrangDangNhap(context.HttpContext);
                return;
            }

            if (session.LayVaiTro() != tk.VaiTro || session.LayHoTen() != tk.HoTen)
                session.GhiDangNhap(ma.Value, tk.HoTen, tk.VaiTro);
        }

        internal static IActionResult VeTrangDangNhap(HttpContext http)
        {
            // Chỉ nhớ địa chỉ trang khi là GET để đăng nhập xong quay lại đúng trang
            var returnUrl = HttpMethods.IsGet(http.Request.Method)
                ? http.Request.Path + http.Request.QueryString
                : null;
            return new RedirectToActionResult("DangNhap", "TaiKhoan",
                string.IsNullOrEmpty(returnUrl) || returnUrl == "/" ? null : new { returnUrl });
        }
    }
}
