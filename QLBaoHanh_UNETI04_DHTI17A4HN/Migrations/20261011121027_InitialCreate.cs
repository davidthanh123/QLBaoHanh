using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinhKien",
                columns: table => new
                {
                    MaLinhKien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLinhKien = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhKien", x => x.MaLinhKien);
                    table.CheckConstraint("CK_LinhKien_DonGia", "[DonGia] >= 0");
                    table.CheckConstraint("CK_LinhKien_Ton", "[SoLuongTon] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "LoaiThietBi",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HangSanXuat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiHanBaoHanhMacDinh = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiThietBi", x => x.MaLoai);
                    table.CheckConstraint("CK_LoaiThietBi_BH", "[ThoiHanBaoHanhMacDinh] BETWEEN 0 AND 120");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MatKhauSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    SoLanDangNhapSai = table.Column<int>(type: "int", nullable: false),
                    KhoaDenNgay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTaoTaiKhoan = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                    table.CheckConstraint("CK_TaiKhoan_Email", "[Email] LIKE '%_@_%._%'");
                    table.CheckConstraint("CK_TaiKhoan_SaiMK", "[SoLanDangNhapSai] >= 0");
                    table.CheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] IN (N'Admin', N'NhanVien', N'KhachHang')");
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LoaiKhachHang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaSoThue = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKhachHang);
                    table.CheckConstraint("CK_KhachHang_Loai", "[LoaiKhachHang] IN (N'CaNhan', N'DoanhNghiep')");
                    table.CheckConstraint("CK_KhachHang_MST", "[MaSoThue] IS NULL OR [LoaiKhachHang] = N'DoanhNghiep'");
                    table.CheckConstraint("CK_KhachHang_SDT", "[SoDienThoai] NOT LIKE '%[^0-9+]%'");
                    table.ForeignKey(
                        name: "FK_KhachHang_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KyThuatVien",
                columns: table => new
                {
                    MaKyThuatVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyThuatVien", x => x.MaKyThuatVien);
                    table.ForeignKey(
                        name: "FK_KyThuatVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyDangNhap",
                columns: table => new
                {
                    MaNhatKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    TenDangNhapNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    KetQua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyDangNhap", x => x.MaNhatKy);
                    table.CheckConstraint("CK_NhatKy_KetQua", "[KetQua] IN (N'ThanhCong', N'SaiMatKhau', N'KhongTonTai', N'BiKhoa')");
                    table.ForeignKey(
                        name: "FK_NhatKyDangNhap_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKem",
                columns: table => new
                {
                    MaTepDinhKem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaDoiTuong = table.Column<int>(type: "int", nullable: false),
                    DuongDanFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TenFileGoc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayUpload = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    MaTaiKhoanUpload = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKem", x => x.MaTepDinhKem);
                    table.CheckConstraint("CK_Tep_Loai", "[LoaiDoiTuong] IN (N'ThietBi', N'PhieuSuaChua', N'ChiTietSuaChua')");
                    table.ForeignKey(
                        name: "FK_TepDinhKem_TaiKhoan_MaTaiKhoanUpload",
                        column: x => x.MaTaiKhoanUpload,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThietBi",
                columns: table => new
                {
                    MaThietBi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLoai = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenThietBi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NgayMua = table.Column<DateTime>(type: "date", nullable: false),
                    HanBaoHanh = table.Column<DateTime>(type: "date", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBi", x => x.MaThietBi);
                    table.CheckConstraint("CK_ThietBi_GiaTri", "[GiaTri] >= 0");
                    table.CheckConstraint("CK_ThietBi_HanBH", "[HanBaoHanh] >= [NgayMua]");
                    table.CheckConstraint("CK_ThietBi_TrangThai", "[TrangThai] IN (N'KhaDung', N'DangSuaChua', N'KhongKhaDung')");
                    table.ForeignKey(
                        name: "FK_ThietBi_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietBi_LoaiThietBi_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LoaiThietBi",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuSuaChua",
                columns: table => new
                {
                    MaPhieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThietBi = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaTaiKhoanTiepNhan = table.Column<int>(type: "int", nullable: true),
                    NgayTiepNhan = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NoiDungLoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MucDo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HanDuKien = table.Column<DateTime>(type: "date", nullable: true),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoHuyTuChoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuSuaChua", x => x.MaPhieu);
                    table.CheckConstraint("CK_Phieu_HanDuKien", "[HanDuKien] IS NULL OR [HanDuKien] >= CAST([NgayTiepNhan] AS date)");
                    table.CheckConstraint("CK_Phieu_HoanThanh", "[TrangThai] <> N'HoanThanh' OR [NgayHoanThanh] IS NOT NULL");
                    table.CheckConstraint("CK_Phieu_LyDo", "[TrangThai] NOT IN (N'Huy', N'TuChoi') OR [LyDoHuyTuChoi] IS NOT NULL");
                    table.CheckConstraint("CK_Phieu_MucDo", "[MucDo] IN (N'Thap', N'TrungBinh', N'Cao', N'KhanCap')");
                    table.CheckConstraint("CK_Phieu_TrangThai", "[TrangThai] IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')");
                    table.ForeignKey(
                        name: "FK_PhieuSuaChua_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuSuaChua_TaiKhoan_MaTaiKhoanTiepNhan",
                        column: x => x.MaTaiKhoanTiepNhan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuSuaChua_ThietBi_MaThietBi",
                        column: x => x.MaThietBi,
                        principalTable: "ThietBi",
                        principalColumn: "MaThietBi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietSuaChua",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<int>(type: "int", nullable: false),
                    MaKyThuatVien = table.Column<int>(type: "int", nullable: false),
                    MaLinhKien = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KetQuaChanDoan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietSuaChua", x => x.MaChiTiet);
                    table.CheckConstraint("CK_CT_DonGia", "[DonGia] >= 0");
                    table.CheckConstraint("CK_CT_SoLuong", "[SoLuong] > 0");
                    table.CheckConstraint("CK_CT_ThanhTien", "[ThanhTien] = [SoLuong] * [DonGia]");
                    table.ForeignKey(
                        name: "FK_ChiTietSuaChua_KyThuatVien_MaKyThuatVien",
                        column: x => x.MaKyThuatVien,
                        principalTable: "KyThuatVien",
                        principalColumn: "MaKyThuatVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietSuaChua_LinhKien_MaLinhKien",
                        column: x => x.MaLinhKien,
                        principalTable: "LinhKien",
                        principalColumn: "MaLinhKien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietSuaChua_PhieuSuaChua_MaPhieu",
                        column: x => x.MaPhieu,
                        principalTable: "PhieuSuaChua",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuTrangThaiPhieu",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<int>(type: "int", nullable: false),
                    MaTaiKhoanThucHien = table.Column<int>(type: "int", nullable: false),
                    TrangThaiCu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TrangThaiMoi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTrangThaiPhieu", x => x.MaLichSu);
                    table.CheckConstraint("CK_LichSu_Cu", "[TrangThaiCu] IS NULL OR [TrangThaiCu] IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')");
                    table.CheckConstraint("CK_LichSu_Moi", "[TrangThaiMoi] IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')");
                    table.ForeignKey(
                        name: "FK_LichSuTrangThaiPhieu_PhieuSuaChua_MaPhieu",
                        column: x => x.MaPhieu,
                        principalTable: "PhieuSuaChua",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuTrangThaiPhieu_TaiKhoan_MaTaiKhoanThucHien",
                        column: x => x.MaTaiKhoanThucHien,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kenh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaTaiKhoanNhan = table.Column<int>(type: "int", nullable: false),
                    MaPhieu = table.Column<int>(type: "int", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.CheckConstraint("CK_TB_Kenh", "[Kenh] IN (N'TrongHeThong', N'Email')");
                    table.CheckConstraint("CK_TB_TrangThai", "[TrangThai] IN (N'ChoGui', N'DaGui', N'Loi')");
                    table.ForeignKey(
                        name: "FK_ThongBao_PhieuSuaChua_MaPhieu",
                        column: x => x.MaPhieu,
                        principalTable: "PhieuSuaChua",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongBao_TaiKhoan_MaTaiKhoanNhan",
                        column: x => x.MaTaiKhoanNhan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSuaChua_MaKyThuatVien",
                table: "ChiTietSuaChua",
                column: "MaKyThuatVien");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSuaChua_MaLinhKien",
                table: "ChiTietSuaChua",
                column: "MaLinhKien");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSuaChua_MaPhieu",
                table: "ChiTietSuaChua",
                column: "MaPhieu");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_CCCD",
                table: "KhachHang",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_MaTaiKhoan",
                table: "KhachHang",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KyThuatVien_MaTaiKhoan",
                table: "KyThuatVien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTrangThaiPhieu_MaPhieu_ThoiGian",
                table: "LichSuTrangThaiPhieu",
                columns: new[] { "MaPhieu", "ThoiGian" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTrangThaiPhieu_MaTaiKhoanThucHien",
                table: "LichSuTrangThaiPhieu",
                column: "MaTaiKhoanThucHien");

            migrationBuilder.CreateIndex(
                name: "IX_LinhKien_TenLinhKien",
                table: "LinhKien",
                column: "TenLinhKien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoaiThietBi_TenLoai",
                table: "LoaiThietBi",
                column: "TenLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyDangNhap_MaTaiKhoan_ThoiGian",
                table: "NhatKyDangNhap",
                columns: new[] { "MaTaiKhoan", "ThoiGian" });

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChua_HanDuKien",
                table: "PhieuSuaChua",
                column: "HanDuKien");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChua_MaKhachHang",
                table: "PhieuSuaChua",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChua_MaTaiKhoanTiepNhan",
                table: "PhieuSuaChua",
                column: "MaTaiKhoanTiepNhan");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChua_MaThietBi",
                table: "PhieuSuaChua",
                column: "MaThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChua_TrangThai_NgayTiepNhan",
                table: "PhieuSuaChua",
                columns: new[] { "TrangThai", "NgayTiepNhan" });

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_LoaiDoiTuong_MaDoiTuong",
                table: "TepDinhKem",
                columns: new[] { "LoaiDoiTuong", "MaDoiTuong" });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_MaTaiKhoanUpload",
                table: "TepDinhKem",
                column: "MaTaiKhoanUpload");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_MaKhachHang",
                table: "ThietBi",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_MaLoai_MaKhachHang_TrangThai",
                table: "ThietBi",
                columns: new[] { "MaLoai", "MaKhachHang", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_SerialNumber",
                table: "ThietBi",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_TrangThai",
                table: "ThietBi",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaPhieu",
                table: "ThongBao",
                column: "MaPhieu");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaTaiKhoanNhan_DaDoc",
                table: "ThongBao",
                columns: new[] { "MaTaiKhoanNhan", "DaDoc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietSuaChua");

            migrationBuilder.DropTable(
                name: "LichSuTrangThaiPhieu");

            migrationBuilder.DropTable(
                name: "NhatKyDangNhap");

            migrationBuilder.DropTable(
                name: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "KyThuatVien");

            migrationBuilder.DropTable(
                name: "LinhKien");

            migrationBuilder.DropTable(
                name: "PhieuSuaChua");

            migrationBuilder.DropTable(
                name: "ThietBi");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "LoaiThietBi");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
