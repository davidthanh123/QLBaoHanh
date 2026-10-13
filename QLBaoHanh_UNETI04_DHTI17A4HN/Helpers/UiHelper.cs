// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: UiHelper - nơi duy nhất quy định nhãn tiếng Việt, màu badge của enum và định dạng hiển thị (theo Doc/GIAODIEN.md mục 4, 5).

#nullable enable
using System.Globalization;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Helpers
{
    /// <summary>
    /// Thông tin để vẽ một badge (nhãn màu). Partial _StatusBadge nhận đúng struct này.
    /// Kind quyết định kiểu thẻ: "status" (viên thuốc bo tròn), "level" (thẻ vuông có thanh màu bên trái, dùng cho MucDo),
    /// "role" (chip vai trò/loại khách hàng).
    /// Note là dòng chú thích nhỏ bên dưới ("còn 12 ngày", "quá 3 ngày"); NoteCss là "is-warning" / "is-danger" hoặc rỗng.
    /// </summary>
    public readonly record struct BadgeInfo(
        string Text,
        string Css,
        string Kind = "status",
        string? Note = null,
        string NoteCss = "");

    /// <summary>Icon (tên Material Symbols) + nhãn, dùng cho enum không có badge màu.</summary>
    public readonly record struct IconLabel(string Icon, string Text);

    /// <summary>
    /// NƠI DUY NHẤT quy định: nhãn tiếng Việt + màu badge của mỗi enum, ngưỡng cảnh báo và định dạng tiền/ngày.
    /// Không tự viết "if (TrangThai == ...) class=..." trong View: hãy gọi UiHelper.Badge(...).
    /// Một trạng thái chỉ có MỘT màu trên toàn hệ thống. Bảng màu đầy đủ: Doc/GIAODIEN.md mục 4.
    /// </summary>
    public static class UiHelper
    {
        // NGƯỠNG CẢNH BÁO (đổi ở đây là đổi toàn hệ thống)

        /// <summary>Còn bao nhiêu ngày bảo hành thì tính là "Sắp hết hạn".</summary>
        public const int NgayCanhBaoBaoHanh = 30;

        /// <summary>Còn bao nhiêu ngày đến hạn dự kiến của phiếu thì tính là "Sắp đến hạn".</summary>
        public const int NgayCanhBaoHanDuKien = 2;

        /// <summary>Tồn kho từ mức này trở xuống (và còn > 0) thì tính là "Sắp hết".</summary>
        public const int SoLuongSapHet = 5;

        // ĐỊNH DẠNG

        private static readonly NumberFormatInfo SoViet = new()
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ",",
        };

        /// <summary>1250000 -> "1.250.000 ₫"; null -> "—"</summary>
        public static string Tien(decimal? value) =>
            value is null ? "—" : value.Value.ToString("N0", SoViet) + " ₫";

        /// <summary>dd/MM/yyyy; null -> "—"</summary>
        public static string Ngay(DateTime? value) => value?.ToString("dd/MM/yyyy") ?? "—";

        /// <summary>dd/MM/yyyy HH:mm; null -> "—"</summary>
        public static string NgayGio(DateTime? value) => value?.ToString("dd/MM/yyyy HH:mm") ?? "—";

        /// <summary>Mã hiển thị: Ma("PSC", 7) -> "PSC-0007" (PSC = phiếu sửa chữa, TB = thiết bị, KH = khách hàng, LK = linh kiện).</summary>
        public static string Ma(string tienTo, int id) => $"{tienTo}-{id:D4}";

        /// <summary>Chữ viết tắt cho avatar: "Nguyễn Quản Trị" -> "QT" (chữ cái đầu của 2 từ cuối).</summary>
        public static string VietTat(string? hoTen)
        {
            var tu = (hoTen ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tu.Length == 0) return "?";
            if (tu.Length == 1) return char.ToUpperInvariant(tu[0][0]).ToString();
            return string.Concat(char.ToUpperInvariant(tu[^2][0]), char.ToUpperInvariant(tu[^1][0]));
        }

        // BADGE THEO ENUM

        public static BadgeInfo Badge(TrangThaiThietBiEnum v) => v switch
        {
            TrangThaiThietBiEnum.KhaDung => new("Khả dụng", "is-success"),
            TrangThaiThietBiEnum.DangSuaChua => new("Đang sửa chữa", "is-info"),
            TrangThaiThietBiEnum.KhongKhaDung => new("Không khả dụng", "is-muted"),
            _ => new(v.ToString(), "is-muted"),
        };

        public static BadgeInfo Badge(TrangThaiPhieuEnum v) => v switch
        {
            TrangThaiPhieuEnum.ChoXuLy => new("Chờ xử lý", "is-warning"),
            TrangThaiPhieuEnum.DangXuLy => new("Đang xử lý", "is-info is-strong"),
            TrangThaiPhieuEnum.HoanThanh => new("Hoàn thành", "is-success"),
            TrangThaiPhieuEnum.Huy => new("Đã hủy", "is-muted"),
            TrangThaiPhieuEnum.TuChoi => new("Từ chối", "is-danger"),
            _ => new(v.ToString(), "is-muted"),
        };

        /// <summary>Mức độ: thẻ vuông có thanh màu bên trái (Kind = "level").</summary>
        public static BadgeInfo Badge(MucDoEnum v) => v switch
        {
            MucDoEnum.Thap => new("Thấp", "is-thap", "level"),
            MucDoEnum.TrungBinh => new("Trung bình", "is-trungbinh", "level"),
            MucDoEnum.Cao => new("Cao", "is-cao", "level"),
            MucDoEnum.KhanCap => new("Khẩn cấp", "is-khancap", "level"),
            _ => new(v.ToString(), "is-thap", "level"),
        };

        /// <summary>Vai trò: chip (Kind = "role").</summary>
        public static BadgeInfo Badge(VaiTroEnum v) => v switch
        {
            VaiTroEnum.Admin => new("Quản trị viên", "is-admin", "role"),
            VaiTroEnum.NhanVien => new("Nhân viên", "is-nhanvien", "role"),
            VaiTroEnum.KhachHang => new("Khách hàng", "is-khachhang", "role"),
            _ => new(v.ToString(), "is-khachhang", "role"),
        };

        /// <summary>Loại khách hàng: chip (Kind = "role").</summary>
        public static BadgeInfo Badge(LoaiKhachHangEnum v) => v switch
        {
            LoaiKhachHangEnum.CaNhan => new("Cá nhân", "is-canhan", "role"),
            LoaiKhachHangEnum.DoanhNghiep => new("Doanh nghiệp", "is-doanhnghiep", "role"),
            _ => new(v.ToString(), "is-canhan", "role"),
        };

        public static BadgeInfo Badge(KetQuaDangNhapEnum v) => v switch
        {
            KetQuaDangNhapEnum.ThanhCong => new("Thành công", "is-success"),
            KetQuaDangNhapEnum.SaiMatKhau => new("Sai mật khẩu", "is-warning"),
            KetQuaDangNhapEnum.BiKhoa => new("Bị khóa", "is-danger"),
            KetQuaDangNhapEnum.KhongTonTai => new("Không tồn tại", "is-muted"),
            _ => new(v.ToString(), "is-muted"),
        };

        public static BadgeInfo Badge(TrangThaiGuiThongBaoEnum v) => v switch
        {
            TrangThaiGuiThongBaoEnum.ChoGui => new("Chờ gửi", "is-warning"),
            TrangThaiGuiThongBaoEnum.DaGui => new("Đã gửi", "is-success"),
            TrangThaiGuiThongBaoEnum.Loi => new("Lỗi", "is-danger"),
            _ => new(v.ToString(), "is-muted"),
        };

        /// <summary>
        /// Cột TrangThai kiểu bit. true = "Hoạt động" (xanh lá); false = "Ngừng hoạt động" (xám).
        /// Với tài khoản (laTaiKhoan: true) thì false hiện "Bị khóa" (đỏ).
        /// </summary>
        public static BadgeInfo BadgeHoatDong(bool hoatDong, bool laTaiKhoan = false) =>
            hoatDong
                ? new("Hoạt động", "is-success")
                : laTaiKhoan
                    ? new("Bị khóa", "is-danger")
                    : new("Ngừng hoạt động", "is-muted");

        // ENUM KHÔNG DÙNG BADGE MÀU: chỉ hiện icon + nhãn

        public static IconLabel Nhan(KenhThongBaoEnum v) => v switch
        {
            KenhThongBaoEnum.TrongHeThong => new("notifications", "Trong hệ thống"),
            KenhThongBaoEnum.Email => new("mail", "Email"),
            _ => new("notifications", v.ToString()),
        };

        public static IconLabel Nhan(LoaiDoiTuongDinhKemEnum v) => v switch
        {
            LoaiDoiTuongDinhKemEnum.ThietBi => new("devices", "Thiết bị"),
            LoaiDoiTuongDinhKemEnum.PhieuSuaChua => new("assignment", "Phiếu sửa chữa"),
            LoaiDoiTuongDinhKemEnum.ChiTietSuaChua => new("build", "Chi tiết sửa chữa"),
            _ => new("attach_file", v.ToString()),
        };

        // GIÁ TRỊ TÍNH ĐỘNG (không lưu DB, xem Doc/THIETKE.md mục 2.1)

        /// <summary>
        /// Bảo hành tính từ ThietBi.HanBaoHanh (hoặc so với ngày tiếp nhận phiếu: truyền ngayThamChieu).
        /// Còn bảo hành (xanh lá) · Sắp hết hạn (≤ 30 ngày, vàng, kèm "còn N ngày") · Hết bảo hành (xám).
        /// Còn bảo hành khi HanBaoHanh >= ngày tham chiếu.
        /// </summary>
        public static BadgeInfo BaoHanh(DateTime hanBaoHanh, DateTime? ngayThamChieu = null)
        {
            var soNgay = (hanBaoHanh.Date - (ngayThamChieu ?? DateTime.Today).Date).Days;

            if (soNgay < 0)
                return new("Hết bảo hành", "is-muted");

            if (soNgay <= NgayCanhBaoBaoHanh)
                return new("Sắp hết hạn", "is-warning", Note: soNgay == 0 ? "hết hạn hôm nay" : $"còn {soNgay} ngày", NoteCss: "is-warning");

            return new("Còn bảo hành", "is-success");
        }

        /// <summary>Phiếu đã đóng (không đổi trạng thái nữa): HoanThanh, Huy, TuChoi.</summary>
        public static bool PhieuDaDong(TrangThaiPhieuEnum trangThai) =>
            trangThai is TrangThaiPhieuEnum.HoanThanh or TrangThaiPhieuEnum.Huy or TrangThaiPhieuEnum.TuChoi;

        /// <summary>
        /// Hạn dự kiến của phiếu. Chỉ tính khi phiếu chưa đóng và có HanDuKien, ngược lại trả về null (View không vẽ gì).
        /// Còn hạn (xanh lá) · Sắp đến hạn (≤ 2 ngày, vàng) · Quá hạn (đỏ, kèm "quá N ngày").
        /// Dùng: @if (UiHelper.HanDuKien(p.HanDuKien, p.TrangThai) is { } han) { ...partial _StatusBadge model="han"... }
        /// </summary>
        public static BadgeInfo? HanDuKien(DateTime? hanDuKien, TrangThaiPhieuEnum trangThai, DateTime? homNay = null)
        {
            if (hanDuKien is null || PhieuDaDong(trangThai)) return null;

            var soNgay = (hanDuKien.Value.Date - (homNay ?? DateTime.Today).Date).Days;

            if (soNgay < 0)
                return new("Quá hạn", "is-danger", Note: $"quá {-soNgay} ngày", NoteCss: "is-danger");

            if (soNgay <= NgayCanhBaoHanDuKien)
                return new("Sắp đến hạn", "is-warning", Note: soNgay == 0 ? "hôm nay" : $"còn {soNgay} ngày", NoteCss: "is-warning");

            return new("Còn hạn", "is-success", Note: $"còn {soNgay} ngày");
        }

        /// <summary>Tồn kho linh kiện: Còn hàng (xanh lá) · Sắp hết (≤ 5, vàng, kèm "còn N cái") · Hết hàng (đỏ).</summary>
        public static BadgeInfo TonKho(int soLuongTon)
        {
            if (soLuongTon <= 0)
                return new("Hết hàng", "is-danger");

            if (soLuongTon <= SoLuongSapHet)
                return new("Sắp hết", "is-warning", Note: $"còn {soLuongTon} cái", NoteCss: "is-warning");

            return new("Còn hàng", "is-success");
        }

        // HÀNG BẢNG MỜ

        /// <summary>Class cho &lt;tr&gt;: phiếu Đã hủy thì mờ cả hàng. Dùng: &lt;tr class="@UiHelper.HangMo(p.TrangThai)"&gt;</summary>
        public static string HangMo(TrangThaiPhieuEnum v) => v == TrangThaiPhieuEnum.Huy ? "is-muted" : "";

        /// <summary>Thiết bị Không khả dụng thì mờ cả hàng (và ẩn nút "Tạo phiếu sửa chữa").</summary>
        public static string HangMo(TrangThaiThietBiEnum v) => v == TrangThaiThietBiEnum.KhongKhaDung ? "is-muted" : "";
    }
}
