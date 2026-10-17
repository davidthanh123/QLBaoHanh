// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Băm và kiểm tra mật khẩu bằng PasswordHasher<TaiKhoan> (Doc/THIETKE.md mục 2.9).

using System;
using Microsoft.AspNetCore.Identity;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Helpers
{
    public static class MatKhauHelper
    {
        private static readonly PasswordHasher<TaiKhoan> Hasher = new();

        // Chuỗi băm của PasswordHasher dài hơn 60 ký tự; mật khẩu plain text của dữ liệu mẫu cũ thì ngắn hơn nhiều.
        private const int DoDaiToiThieuChuoiBam = 60;

        // Băm mật khẩu sử dụng PasswordHasher của ASP.NET Core Identity
        public static string Bam(TaiKhoan taiKhoan, string matKhau) => Hasher.HashPassword(taiKhoan, matKhau);

        // Kiểm tra mật khẩu người dùng nhập. canBamLai = true khi nên băm lại và lưu (chuẩn băm cũ hoặc dữ liệu mẫu cũ chưa băm).
        public static bool KiemTra(TaiKhoan taiKhoan, string matKhauNhap, out bool canBamLai)
        {
            canBamLai = false;

            // Kiểm tra tính hợp lệ của dữ liệu đầu vào để tránh lỗi NullReferenceException
            if (taiKhoan == null || string.IsNullOrEmpty(taiKhoan.MatKhau) || string.IsNullOrEmpty(matKhauNhap))
            {
                return false;
            }

            var daBam = taiKhoan.MatKhau.Length >= DoDaiToiThieuChuoiBam;

            if (daBam)
            {
                try
                {
                    var ketQua = Hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhau, matKhauNhap);

                    // Cần băm lại nếu thuật toán nâng cấp hoặc định dạng thay đổi nhưng mật khẩu vẫn đúng
                    canBamLai = ketQua == PasswordVerificationResult.SuccessRehashNeeded;

                    return ketQua != PasswordVerificationResult.Failed;
                }
                catch (Exception)
                {
                    // Bắt toàn bộ ngoại lệ mã hóa (FormatException, CryptographicException...) để bảo vệ hệ thống không bị sập
                    return false;
                }
            }

            if (taiKhoan.MatKhau == matKhauNhap)
            {
                canBamLai = true;
                return true;
            }

            return false;
        }
    }
}
