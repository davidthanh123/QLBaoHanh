// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Hàm tiện ích để Controller đổi KetQuaDichVu thành TempData/ModelState.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Helpers
{
    public static class KetQuaDichVuExtensions
    {
        /// <summary>
        /// Đưa ThongBao vào TempData. Mặc định dùng khóa "ThanhCong"/"Loi";
        /// LinhKien và ChiTietSuaChua đang dùng "Success"/"Error" nên truyền khóa riêng.
        /// </summary>
        public static void DatThongBao(this Controller controller, KetQuaDichVu ketQua,
            string khoaThanhCong = "ThanhCong", string khoaLoi = "Loi")
        {
            if (ketQua.ThongBao == null) return;
            controller.TempData[ketQua.ThanhCong ? khoaThanhCong : khoaLoi] = ketQua.ThongBao;
        }

        /// <summary>Thêm các lỗi theo ô nhập của Service vào ModelState để View hiển thị.</summary>
        public static void GopLoiVaoModelState(this KetQuaDichVu ketQua, ModelStateDictionary modelState)
        {
            foreach (var loi in ketQua.LoiTruong)
                modelState.AddModelError(loi.Truong, loi.ThongBao);
        }
    }
}
