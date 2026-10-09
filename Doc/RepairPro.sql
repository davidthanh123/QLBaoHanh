/* =====================================================================
   RepairPro - Quan ly bao hanh & sua chua thiet bi (De tai 17)
   SQL Server | Sinh tu ERD.md
   ===================================================================== */
IF DB_ID(N'RepairProDB') IS NULL CREATE DATABASE RepairProDB;
GO
USE RepairProDB;
GO

/* ---------- XOA BANG CU (theo thu tu phu thuoc) ---------- */
DROP TABLE IF EXISTS ThongBao, TepDinhKem, ChiTietSuaChua, LichSuTrangThaiPhieu,
    PhieuSuaChua, LinhKien, ThietBi, KyThuatVien, KhachHang, LoaiThietBi,
    NhatKyDangNhap, TaiKhoan;
GO

/* ---------- TAI KHOAN ---------- */
CREATE TABLE TaiKhoan (
    MaTaiKhoan       INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap      NVARCHAR(50)  NOT NULL,
    MatKhau          NVARCHAR(256) NOT NULL,
    MatKhauSalt      NVARCHAR(128) NULL,
    HoTen            NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(150) NOT NULL,
    VaiTro           NVARCHAR(20)  NOT NULL,
    TrangThai        BIT           NOT NULL DEFAULT 1,   -- 0 = bi khoa
    SoLanDangNhapSai INT           NOT NULL DEFAULT 0,
    KhoaDenNgay      DATETIME      NULL,
    NgayTaoTaiKhoan  DATETIME      NOT NULL DEFAULT GETDATE(),
    LanDangNhapCuoi  DATETIME      NULL,
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'Admin', N'NhanVien', N'KhachHang')),
    CONSTRAINT CK_TaiKhoan_Email  CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_TaiKhoan_SaiMK  CHECK (SoLanDangNhapSai >= 0)
);

CREATE TABLE NhatKyDangNhap (
    MaNhatKy         INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan       INT NULL,
    TenDangNhapNhap  NVARCHAR(50) NOT NULL,
    ThoiGian         DATETIME NOT NULL DEFAULT GETDATE(),
    KetQua           NVARCHAR(20) NOT NULL,
    DiaChiIP         NVARCHAR(45) NULL,
    CONSTRAINT FK_NhatKy_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan) ON DELETE SET NULL,
    CONSTRAINT CK_NhatKy_KetQua CHECK (KetQua IN (N'ThanhCong', N'SaiMatKhau', N'KhongTonTai', N'BiKhoa'))
);

/* ---------- DANH MUC ---------- */
CREATE TABLE LoaiThietBi (
    MaLoai                INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai               NVARCHAR(100) NOT NULL,
    HangSanXuat           NVARCHAR(100) NULL,
    MoTa                  NVARCHAR(500) NULL,
    ThoiHanBaoHanhMacDinh INT NOT NULL DEFAULT 12,      -- thang
    TrangThai             BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_LoaiThietBi_Ten UNIQUE (TenLoai),
    CONSTRAINT CK_LoaiThietBi_BH CHECK (ThoiHanBaoHanhMacDinh >= 0)
);

CREATE TABLE KhachHang (
    MaKhachHang  INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan   INT NULL,
    HoTen        NVARCHAR(100) NOT NULL,
    SoDienThoai  NVARCHAR(15)  NOT NULL,
    Email        NVARCHAR(150) NULL,
    DiaChi       NVARCHAR(250) NULL,
    CCCD         NVARCHAR(12)  NULL,
    NgayTao      DATETIME NOT NULL DEFAULT GETDATE(),
    LoaiKhachHang NVARCHAR(20) NOT NULL DEFAULT N'CaNhan',
    TrangThai    BIT NOT NULL DEFAULT 1,
    GhiChu       NVARCHAR(500) NULL,
    MaSoThue     NVARCHAR(14) NULL,
    CONSTRAINT FK_KhachHang_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_KhachHang_Loai CHECK (LoaiKhachHang IN (N'CaNhan', N'DoanhNghiep')),
    CONSTRAINT CK_KhachHang_SDT  CHECK (SoDienThoai NOT LIKE '%[^0-9+]%')
);
CREATE UNIQUE INDEX UX_KhachHang_TaiKhoan ON KhachHang(MaTaiKhoan) WHERE MaTaiKhoan IS NOT NULL;
CREATE UNIQUE INDEX UX_KhachHang_CCCD     ON KhachHang(CCCD)     WHERE CCCD IS NOT NULL;

CREATE TABLE KyThuatVien (
    MaKyThuatVien INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan    INT NOT NULL,
    HoTen         NVARCHAR(100) NOT NULL,
    ChuyenMon     NVARCHAR(150) NULL,
    SoDienThoai   NVARCHAR(15)  NULL,
    Email         NVARCHAR(150) NULL,
    TrangThai     BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_KyThuatVien_TaiKhoan UNIQUE (MaTaiKhoan),
    CONSTRAINT FK_KyThuatVien_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

CREATE TABLE ThietBi (
    MaThietBi   INT IDENTITY(1,1) PRIMARY KEY,
    MaLoai      INT NOT NULL,
    MaKhachHang INT NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL,
    TenThietBi  NVARCHAR(150) NOT NULL,
    NgayMua     DATE NULL,
    HanBaoHanh  DATE NULL,
    MoTa        NVARCHAR(500) NULL,
    TrangThai   NVARCHAR(20) NOT NULL DEFAULT N'KhaDung',
    GhiChu      NVARCHAR(500) NULL,
    GiaTri      DECIMAL(18,2) NULL,
    CONSTRAINT UQ_ThietBi_Serial UNIQUE (SerialNumber),
    CONSTRAINT FK_ThietBi_Loai      FOREIGN KEY (MaLoai)      REFERENCES LoaiThietBi(MaLoai),
    CONSTRAINT FK_ThietBi_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT CK_ThietBi_TrangThai CHECK (TrangThai IN (N'KhaDung', N'DangSuaChua', N'KhongKhaDung')),
    CONSTRAINT CK_ThietBi_HanBH CHECK (HanBaoHanh IS NULL OR NgayMua IS NULL OR HanBaoHanh >= NgayMua),
    CONSTRAINT CK_ThietBi_GiaTri CHECK (GiaTri IS NULL OR GiaTri >= 0)
);

CREATE TABLE LinhKien (
    MaLinhKien  INT IDENTITY(1,1) PRIMARY KEY,
    TenLinhKien NVARCHAR(150) NOT NULL,
    DonViTinh   NVARCHAR(30)  NOT NULL,
    DonGia      DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon  INT NOT NULL DEFAULT 0,
    TrangThai   BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_LinhKien_Ten UNIQUE (TenLinhKien),
    CONSTRAINT CK_LinhKien_DonGia CHECK (DonGia >= 0),
    CONSTRAINT CK_LinhKien_Ton    CHECK (SoLuongTon >= 0)
);

/* ---------- NGHIEP VU SUA CHUA ---------- */
CREATE TABLE PhieuSuaChua (
    MaPhieu            INT IDENTITY(1,1) PRIMARY KEY,
    MaThietBi          INT NOT NULL,
    MaKhachHang        INT NOT NULL,
    MaTaiKhoanTiepNhan INT NULL,
    NgayTiepNhan       DATETIME NOT NULL DEFAULT GETDATE(),
    NoiDungLoi         NVARCHAR(1000) NOT NULL,
    TrangThai          NVARCHAR(20) NOT NULL DEFAULT N'ChoXuLy',
    MucDo              NVARCHAR(20) NOT NULL DEFAULT N'TrungBinh',
    HanDuKien          DATE NULL,
    NgayHoanThanh      DATETIME NULL,
    LyDoHuyTuChoi      NVARCHAR(500) NULL,
    CONSTRAINT FK_Phieu_ThietBi   FOREIGN KEY (MaThietBi)   REFERENCES ThietBi(MaThietBi),
    CONSTRAINT FK_Phieu_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT FK_Phieu_TiepNhan  FOREIGN KEY (MaTaiKhoanTiepNhan) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_Phieu_TrangThai CHECK (TrangThai IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')),
    CONSTRAINT CK_Phieu_MucDo     CHECK (MucDo IN (N'Thap', N'TrungBinh', N'Cao', N'KhanCap')),
    CONSTRAINT CK_Phieu_LyDo      CHECK (TrangThai NOT IN (N'Huy', N'TuChoi') OR LyDoHuyTuChoi IS NOT NULL),
    CONSTRAINT CK_Phieu_HoanThanh CHECK (TrangThai <> N'HoanThanh' OR NgayHoanThanh IS NOT NULL),
    CONSTRAINT CK_Phieu_HanDuKien CHECK (HanDuKien IS NULL OR HanDuKien >= CAST(NgayTiepNhan AS DATE))
);

CREATE TABLE LichSuTrangThaiPhieu (
    MaLichSu           INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieu            INT NOT NULL,
    MaTaiKhoanThucHien INT NOT NULL,
    TrangThaiCu        NVARCHAR(20) NULL,
    TrangThaiMoi       NVARCHAR(20) NOT NULL,
    ThoiGian           DATETIME NOT NULL DEFAULT GETDATE(),
    GhiChu             NVARCHAR(500) NULL,
    CONSTRAINT FK_LichSu_Phieu   FOREIGN KEY (MaPhieu) REFERENCES PhieuSuaChua(MaPhieu) ON DELETE CASCADE,
    CONSTRAINT FK_LichSu_TaiKhoan FOREIGN KEY (MaTaiKhoanThucHien) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_LichSu_Cu  CHECK (TrangThaiCu IS NULL OR TrangThaiCu IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')),
    CONSTRAINT CK_LichSu_Moi CHECK (TrangThaiMoi IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi'))
);

CREATE TABLE ChiTietSuaChua (
    MaChiTiet       INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieu         INT NOT NULL,
    MaKyThuatVien   INT NOT NULL,
    MaLinhKien      INT NULL,
    SoLuong         INT NOT NULL DEFAULT 1,
    DonGia          DECIMAL(18,2) NOT NULL DEFAULT 0,
    ThanhTien       AS (CONVERT(DECIMAL(18,2), SoLuong * DonGia)) PERSISTED,
    KetQuaChanDoan  NVARCHAR(1000) NULL,
    GhiChu          NVARCHAR(500) NULL,
    CONSTRAINT FK_CT_Phieu    FOREIGN KEY (MaPhieu)       REFERENCES PhieuSuaChua(MaPhieu) ON DELETE CASCADE,
    CONSTRAINT FK_CT_KTV      FOREIGN KEY (MaKyThuatVien) REFERENCES KyThuatVien(MaKyThuatVien),
    CONSTRAINT FK_CT_LinhKien FOREIGN KEY (MaLinhKien)    REFERENCES LinhKien(MaLinhKien),
    CONSTRAINT CK_CT_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CT_DonGia  CHECK (DonGia >= 0)
);

/* ---------- TEP DINH KEM & THONG BAO ---------- */
CREATE TABLE TepDinhKem (
    MaTepDinhKem     INT IDENTITY(1,1) PRIMARY KEY,
    LoaiDoiTuong     NVARCHAR(20) NOT NULL,
    MaDoiTuong       INT NOT NULL,          -- tham chieu da hinh theo LoaiDoiTuong (khong co FK)
    DuongDanFile     NVARCHAR(500) NOT NULL,
    TenFileGoc       NVARCHAR(255) NOT NULL,
    NgayUpload       DATETIME NOT NULL DEFAULT GETDATE(),
    MaTaiKhoanUpload INT NOT NULL,
    CONSTRAINT FK_Tep_TaiKhoan FOREIGN KEY (MaTaiKhoanUpload) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_Tep_Loai CHECK (LoaiDoiTuong IN (N'ThietBi', N'PhieuSuaChua', N'ChiTietSuaChua'))
);

CREATE TABLE ThongBao (
    MaThongBao     INT IDENTITY(1,1) PRIMARY KEY,
    Kenh           NVARCHAR(20) NOT NULL DEFAULT N'TrongHeThong',
    MaTaiKhoanNhan INT NOT NULL,
    MaPhieu        INT NULL,
    NoiDung        NVARCHAR(1000) NOT NULL,
    NgayTao        DATETIME NOT NULL DEFAULT GETDATE(),
    NgayGui        DATETIME NULL,
    TrangThai      NVARCHAR(20) NOT NULL DEFAULT N'ChoGui',
    DaDoc          BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_TB_TaiKhoan FOREIGN KEY (MaTaiKhoanNhan) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT FK_TB_Phieu    FOREIGN KEY (MaPhieu) REFERENCES PhieuSuaChua(MaPhieu) ON DELETE SET NULL,
    CONSTRAINT CK_TB_Kenh      CHECK (Kenh IN (N'TrongHeThong', N'Email')),
    CONSTRAINT CK_TB_TrangThai CHECK (TrangThai IN (N'ChoGui', N'DaGui', N'Loi'))
);
GO

/* ---------- INDEX HO TRO TIM KIEM / THONG KE ---------- */
CREATE INDEX IX_ThietBi_KhachHang  ON ThietBi(MaKhachHang);
CREATE INDEX IX_ThietBi_Loai       ON ThietBi(MaLoai);
CREATE INDEX IX_ThietBi_TrangThai  ON ThietBi(TrangThai);
CREATE INDEX IX_Phieu_ThietBi      ON PhieuSuaChua(MaThietBi);
CREATE INDEX IX_Phieu_KhachHang    ON PhieuSuaChua(MaKhachHang);
CREATE INDEX IX_Phieu_TrangThai    ON PhieuSuaChua(TrangThai, NgayTiepNhan);
CREATE INDEX IX_Phieu_HanDuKien    ON PhieuSuaChua(HanDuKien);
CREATE INDEX IX_LichSu_Phieu       ON LichSuTrangThaiPhieu(MaPhieu, ThoiGian);
CREATE INDEX IX_CT_Phieu           ON ChiTietSuaChua(MaPhieu);
CREATE INDEX IX_CT_LinhKien        ON ChiTietSuaChua(MaLinhKien);
CREATE INDEX IX_Tep_DoiTuong       ON TepDinhKem(LoaiDoiTuong, MaDoiTuong);
CREATE INDEX IX_TB_Nhan            ON ThongBao(MaTaiKhoanNhan, DaDoc);
CREATE INDEX IX_NhatKy_TaiKhoan    ON NhatKyDangNhap(MaTaiKhoan, ThoiGian);
GO

/* =====================================================================
   DU LIEU MAU
   Mat khau mau = SHA2_256(salt + '123456') dang hex.
   Neu app BAM MAT KHAU KHAC (BCrypt/PBKDF2...) thi tao lai tai khoan
   bang chuc nang dang ky cua app hoac cap nhat cot MatKhau cho khop.
   ===================================================================== */
DECLARE @pw NVARCHAR(256);
INSERT TaiKhoan (TenDangNhap, MatKhau, MatKhauSalt, HoTen, Email, VaiTro) VALUES
 (N'admin', N'', N'salt_admin', N'Quản trị viên',  N'admin@repairpro.vn', N'Admin'),
 (N'nv01',  N'', N'salt_nv01',  N'Nguyễn Văn Kỹ',  N'nv01@repairpro.vn',  N'NhanVien'),
 (N'nv02',  N'', N'salt_nv02',  N'Trần Thị Thợ',   N'nv02@repairpro.vn',  N'NhanVien'),
 (N'kh01',  N'', N'salt_kh01',  N'Lê Minh Anh',    N'kh01@gmail.com',     N'KhachHang'),
 (N'kh02',  N'', N'salt_kh02',  N'Công ty ABC',    N'abc@company.vn',     N'KhachHang');
UPDATE TaiKhoan SET MatKhau = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', MatKhauSalt + N'123456'), 2);

INSERT LoaiThietBi (TenLoai, HangSanXuat, MoTa, ThoiHanBaoHanhMacDinh) VALUES
 (N'Laptop',        N'Dell/HP/Asus', N'Máy tính xách tay', 24),
 (N'Điện thoại',    N'Apple/Samsung', N'Điện thoại thông minh', 12),
 (N'Máy in',        N'Canon/HP', N'Máy in văn phòng', 12),
 (N'Máy lạnh',      N'Daikin/Panasonic', N'Điều hòa không khí', 24);

INSERT KhachHang (MaTaiKhoan, HoTen, SoDienThoai, Email, DiaChi, CCCD, LoaiKhachHang, MaSoThue) VALUES
 (4, N'Lê Minh Anh', N'0912345678', N'kh01@gmail.com', N'Cầu Giấy, Hà Nội', N'001099000001', N'CaNhan', NULL),
 (5, N'Công ty ABC', N'0987654321', N'abc@company.vn', N'Nam Từ Liêm, Hà Nội', NULL, N'DoanhNghiep', N'0101234567');

INSERT KyThuatVien (MaTaiKhoan, HoTen, ChuyenMon, SoDienThoai, Email) VALUES
 (2, N'Nguyễn Văn Kỹ', N'Laptop, Điện thoại', N'0901111111', N'nv01@repairpro.vn'),
 (3, N'Trần Thị Thợ',  N'Máy in, Máy lạnh',   N'0902222222', N'nv02@repairpro.vn');

INSERT ThietBi (MaLoai, MaKhachHang, SerialNumber, TenThietBi, NgayMua, HanBaoHanh, GiaTri, TrangThai) VALUES
 (1, 1, N'DL-XPS13-0001', N'Dell XPS 13',      '2025-03-01', '2027-03-01', 32000000, N'DangSuaChua'),
 (2, 1, N'IP15-0002',     N'iPhone 15',        '2024-01-15', '2025-01-15', 22000000, N'KhaDung'),
 (3, 2, N'CN-LBP-0003',   N'Canon LBP2900',    '2025-06-10', '2026-06-10',  3500000, N'KhaDung'),
 (4, 2, N'DK-INV-0004',   N'Daikin Inverter 1.5HP', '2025-08-20', '2027-08-20', 15000000, N'KhaDung');

INSERT LinhKien (TenLinhKien, DonViTinh, DonGia, SoLuongTon) VALUES
 (N'Pin laptop Dell XPS', N'Cái', 1500000, 10),
 (N'Màn hình iPhone 15',  N'Cái', 4500000, 5),
 (N'Hộp mực Canon 303',   N'Hộp',  850000, 20),
 (N'Gas R32',             N'Bình', 400000,  3),
 (N'Ổ SSD 512GB',         N'Cái', 1800000, 0);

INSERT PhieuSuaChua (MaThietBi, MaKhachHang, MaTaiKhoanTiepNhan, NgayTiepNhan, NoiDungLoi, TrangThai, MucDo, HanDuKien, NgayHoanThanh, LyDoHuyTuChoi) VALUES
 (1, 1, 2, DATEADD(DAY,-5,GETDATE()), N'Laptop sụt pin nhanh, tự tắt nguồn', N'DangXuLy', N'Cao', CAST(DATEADD(DAY,3,GETDATE()) AS DATE), NULL, NULL),
 (2, 1, 2, DATEADD(DAY,-20,GETDATE()), N'Vỡ màn hình', N'HoanThanh', N'TrungBinh', CAST(DATEADD(DAY,-15,GETDATE()) AS DATE), DATEADD(DAY,-16,GETDATE()), NULL),
 (3, 2, NULL, GETDATE(), N'Máy in bị kẹt giấy liên tục', N'ChoXuLy', N'Thap', CAST(DATEADD(DAY,7,GETDATE()) AS DATE), NULL, NULL),
 (4, 2, 3, DATEADD(DAY,-2,GETDATE()), N'Máy lạnh không mát', N'TuChoi', N'Thap', NULL, NULL, N'Thiết bị không thuộc phạm vi bảo hành');

INSERT LichSuTrangThaiPhieu (MaPhieu, MaTaiKhoanThucHien, TrangThaiCu, TrangThaiMoi, GhiChu) VALUES
 (1, 2, NULL, N'ChoXuLy', N'Tạo phiếu'),
 (1, 2, N'ChoXuLy', N'DangXuLy', N'Bắt đầu chẩn đoán'),
 (2, 2, NULL, N'ChoXuLy', N'Tạo phiếu'),
 (2, 2, N'ChoXuLy', N'DangXuLy', N'Thay màn hình'),
 (2, 2, N'DangXuLy', N'HoanThanh', N'Đã bàn giao'),
 (3, 5, NULL, N'ChoXuLy', N'Khách hàng đăng ký'),
 (4, 3, NULL, N'ChoXuLy', N'Tạo phiếu'),
 (4, 3, N'ChoXuLy', N'TuChoi', N'Hết bảo hành');

INSERT ChiTietSuaChua (MaPhieu, MaKyThuatVien, MaLinhKien, SoLuong, DonGia, KetQuaChanDoan) VALUES
 (1, 1, 1, 1, 1500000, N'Pin chai, cần thay mới'),
 (2, 1, 2, 1, 4500000, N'Thay màn hình chính hãng');

INSERT ThongBao (Kenh, MaTaiKhoanNhan, MaPhieu, NoiDung, NgayGui, TrangThai) VALUES
 (N'TrongHeThong', 4, 1, N'Phiếu #1 đang được xử lý.', GETDATE(), N'DaGui'),
 (N'TrongHeThong', 4, 2, N'Phiếu #2 đã hoàn thành.',    GETDATE(), N'DaGui'),
 (N'Email',        5, 4, N'Phiếu #4 bị từ chối.',       NULL,      N'ChoGui');

INSERT NhatKyDangNhap (MaTaiKhoan, TenDangNhapNhap, KetQua, DiaChiIP) VALUES
 (1, N'admin', N'ThanhCong', N'127.0.0.1'),
 (NULL, N'hacker', N'KhongTonTai', N'10.0.0.5');
GO
