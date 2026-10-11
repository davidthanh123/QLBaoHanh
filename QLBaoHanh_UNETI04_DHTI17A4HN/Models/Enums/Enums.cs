// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Khai báo 9 enum dùng chung cho các Entity (khớp Doc/THIETKE.md mục 2.3).

// LƯU Ý CHUNG: mọi enum dưới đây được lưu trong CSDL dạng CHUỖI (nvarchar) = đúng tên member,
// cấu hình bằng HasConversion<string>() trong AppDbContext. Hệ quả:
//  - So sánh bằng (==, !=) và Contains trong LINQ chạy bình thường.
//  - KHÔNG dùng >, <, >= hay OrderBy trực tiếp trên cột enum (DB sẽ so sánh theo bảng chữ cái,
//    không theo thứ tự khai báo). Cần sắp theo mức độ thì dùng điều kiện/CASE hoặc sắp sau khi ToList().
//  - Nhãn tiếng Việt chỉ dùng khi hiển thị (UiHelper), không lưu DB.
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums
{
    public enum VaiTroEnum
    {
        Admin,
        NhanVien,
        KhachHang,
    }

    public enum LoaiKhachHangEnum
    {
        CaNhan,
        DoanhNghiep,
    }

    public enum TrangThaiThietBiEnum
    {
        KhaDung,
        DangSuaChua,
        KhongKhaDung,
    }

    // Luồng hợp lệ xem Doc/THIETKE.md mục 2.4
    public enum TrangThaiPhieuEnum
    {
        ChoXuLy,
        DangXuLy,
        HoanThanh,
        Huy,
        TuChoi,
    }

    public enum MucDoEnum
    {
        Thap,
        TrungBinh,
        Cao,
        KhanCap,
    }

    public enum KetQuaDangNhapEnum
    {
        ThanhCong,
        SaiMatKhau,
        KhongTonTai,
        BiKhoa,
    }

    public enum LoaiDoiTuongDinhKemEnum
    {
        ThietBi,
        PhieuSuaChua,
        ChiTietSuaChua,
    }

    public enum KenhThongBaoEnum
    {
        TrongHeThong,
        Email,
    }

    public enum TrangThaiGuiThongBaoEnum
    {
        ChoGui,
        DaGui,
        Loi,
    }
}
