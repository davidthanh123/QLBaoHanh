/* =====================================================================
   RepairPro - Quan ly bao hanh & sua chua thiet bi (De tai 17)
   SQL Server | PHIEN BAN 1.0 - CHOT NGAY 11/10/2026
   Dong bo voi: schema.dbml, ERD.md, THIETKE.md (doi 1 file -> doi ca 4 trong cung PR)

   12 bang: 7 bat buoc (de muc 10) + 2 bo sung nghiep vu (KyThuatVien,
            LichSuTrangThaiPhieu) + 3 nang cao (NhatKyDangNhap, TepDinhKem, ThongBao).

   Cach dung:
   - Day la script THAM CHIEU de doc/doi chieu va de tao DB bang tay khi can.
     Khi chay ung dung, EF Core Migration tu tao DB (database QLBaoHanh, xem
     appsettings.json) nen KHONG can chay file nay.
   - Chay tren database TRONG. Muon tao lai tu dau: bo comment khoi DROP ben duoi.
   - Khong co du lieu mau o day: nguon du lieu mau duy nhat la Data/DbSeeder.cs.

   Quy uoc:
   - Cot enum luu dang CHUOI (nvarchar + CHECK, gia tri ASCII khong dau);
     trong EF Core dung .HasConversion<string>() cho cac cot enum.
   - Moi FK la NO ACTION (= DeleteBehavior.Restrict trong EF Core): khong xoa
     day chuyen, bao toan lich su sua chua. Ngung dung thi doi TrangThai.
   ===================================================================== */

/* ---------- (TUY CHON) XOA BANG CU THEO THU TU PHU THUOC ----------
DROP TABLE IF EXISTS ThongBao, TepDinhKem, ChiTietSuaChua, LichSuTrangThaiPhieu,
    PhieuSuaChua, LinhKien, ThietBi, KyThuatVien, KhachHang, LoaiThietBi,
    NhatKyDangNhap, TaiKhoan;
GO
------------------------------------------------------------------- */

/* =====================================================================
   MODULE 1: TaiKhoan, LoaiThietBi, NhatKyDangNhap
   ===================================================================== */
CREATE TABLE TaiKhoan (
    MaTaiKhoan       INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap      NVARCHAR(50)  NOT NULL,
    MatKhau          NVARCHAR(255) NOT NULL,            -- luu ban hash (PasswordHasher<T>), khong luu plain text
    MatKhauSalt      NVARCHAR(255) NULL,                -- de NULL neu dung PasswordHasher<T>
    HoTen            NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(150) NOT NULL,
    VaiTro           NVARCHAR(20)  NOT NULL,
    TrangThai        BIT           NOT NULL DEFAULT 1,  -- 0 = bi khoa (vinh vien, do Admin)
    SoLanDangNhapSai INT           NOT NULL DEFAULT 0,  -- reset ve 0 khi dang nhap thanh cong
    KhoaDenNgay      DATETIME      NULL,                -- khoa tam thoi sau N lan sai
    NgayTaoTaiKhoan  DATETIME      NOT NULL DEFAULT GETDATE(),
    LanDangNhapCuoi  DATETIME      NULL,
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'Admin', N'NhanVien', N'KhachHang')),
    CONSTRAINT CK_TaiKhoan_Email  CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_TaiKhoan_SaiMK  CHECK (SoLanDangNhapSai >= 0)
);

CREATE TABLE NhatKyDangNhap (
    MaNhatKy         INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan       INT NULL,                          -- NULL neu nhap sai TenDangNhap
    TenDangNhapNhap  NVARCHAR(50) NOT NULL,
    ThoiGian         DATETIME NOT NULL DEFAULT GETDATE(),
    KetQua           NVARCHAR(20) NOT NULL,
    DiaChiIP         NVARCHAR(45) NULL,
    CONSTRAINT FK_NhatKy_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_NhatKy_KetQua CHECK (KetQua IN (N'ThanhCong', N'SaiMatKhau', N'KhongTonTai', N'BiKhoa'))
);

CREATE TABLE LoaiThietBi (
    MaLoai                INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai               NVARCHAR(100) NOT NULL,
    HangSanXuat           NVARCHAR(100) NOT NULL,
    MoTa                  NVARCHAR(500) NULL,
    ThoiHanBaoHanhMacDinh INT NOT NULL DEFAULT 12,      -- thang
    TrangThai             BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_LoaiThietBi_Ten UNIQUE (TenLoai),
    CONSTRAINT CK_LoaiThietBi_BH  CHECK (ThoiHanBaoHanhMacDinh BETWEEN 0 AND 120)
);

/* =====================================================================
   MODULE 3: KhachHang   |   MODULE 4: KyThuatVien
   ===================================================================== */
CREATE TABLE KhachHang (
    MaKhachHang   INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan    INT NULL,                             -- khach nhan vien nhap tay co the chua co tai khoan
    HoTen         NVARCHAR(100) NOT NULL,
    SoDienThoai   NVARCHAR(15)  NOT NULL,
    Email         NVARCHAR(150) NULL,
    DiaChi        NVARCHAR(250) NULL,
    CCCD          NVARCHAR(12)  NULL,
    NgayTao       DATETIME NOT NULL DEFAULT GETDATE(),
    LoaiKhachHang NVARCHAR(20) NOT NULL DEFAULT N'CaNhan',
    TrangThai     BIT NOT NULL DEFAULT 1,
    GhiChu        NVARCHAR(500) NULL,
    MaSoThue      NVARCHAR(14) NULL,                    -- chi dung voi DoanhNghiep
    CONSTRAINT FK_KhachHang_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_KhachHang_Loai CHECK (LoaiKhachHang IN (N'CaNhan', N'DoanhNghiep')),
    CONSTRAINT CK_KhachHang_SDT  CHECK (SoDienThoai NOT LIKE '%[^0-9+]%'),
    CONSTRAINT CK_KhachHang_MST  CHECK (MaSoThue IS NULL OR LoaiKhachHang = N'DoanhNghiep')
);
CREATE UNIQUE INDEX UX_KhachHang_TaiKhoan ON KhachHang(MaTaiKhoan) WHERE MaTaiKhoan IS NOT NULL;
CREATE UNIQUE INDEX UX_KhachHang_CCCD     ON KhachHang(CCCD)       WHERE CCCD IS NOT NULL;

CREATE TABLE KyThuatVien (
    MaKyThuatVien INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan    INT NOT NULL,                         -- tai khoan VaiTro = NhanVien
    HoTen         NVARCHAR(100) NOT NULL,
    ChuyenMon     NVARCHAR(150) NULL,
    SoDienThoai   NVARCHAR(15)  NULL,
    Email         NVARCHAR(150) NULL,
    TrangThai     BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_KyThuatVien_TaiKhoan UNIQUE (MaTaiKhoan),
    CONSTRAINT FK_KyThuatVien_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);

/* =====================================================================
   MODULE 2: ThietBi
   ===================================================================== */
CREATE TABLE ThietBi (
    MaThietBi    INT IDENTITY(1,1) PRIMARY KEY,
    MaLoai       INT NOT NULL,
    MaKhachHang  INT NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL,
    TenThietBi   NVARCHAR(150) NOT NULL,
    NgayMua      DATE NOT NULL,
    HanBaoHanh   DATE NOT NULL,
    MoTa         NVARCHAR(500) NULL,
    TrangThai    NVARCHAR(20) NOT NULL DEFAULT N'KhaDung',
    GhiChu       NVARCHAR(500) NULL,
    GiaTri       DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ThietBi_Serial    UNIQUE (SerialNumber),
    CONSTRAINT FK_ThietBi_Loai      FOREIGN KEY (MaLoai)      REFERENCES LoaiThietBi(MaLoai),
    CONSTRAINT FK_ThietBi_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    CONSTRAINT CK_ThietBi_TrangThai CHECK (TrangThai IN (N'KhaDung', N'DangSuaChua', N'KhongKhaDung')),
    CONSTRAINT CK_ThietBi_HanBH     CHECK (HanBaoHanh >= NgayMua),
    CONSTRAINT CK_ThietBi_GiaTri    CHECK (GiaTri >= 0)
);

/* =====================================================================
   MODULE 5: LinhKien
   ===================================================================== */
CREATE TABLE LinhKien (
    MaLinhKien  INT IDENTITY(1,1) PRIMARY KEY,
    TenLinhKien NVARCHAR(150) NOT NULL,
    DonViTinh   NVARCHAR(30)  NOT NULL,                 -- vd: Cai, Hop, Binh...
    DonGia      DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon  INT NOT NULL DEFAULT 0,
    MoTa        NVARCHAR(500) NULL,
    TrangThai   BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_LinhKien_Ten    UNIQUE (TenLinhKien),
    CONSTRAINT CK_LinhKien_DonGia CHECK (DonGia >= 0),
    CONSTRAINT CK_LinhKien_Ton    CHECK (SoLuongTon >= 0)
);

/* =====================================================================
   MODULE 3, 4, 5: PhieuSuaChua, LichSuTrangThaiPhieu, ChiTietSuaChua
   ===================================================================== */
CREATE TABLE PhieuSuaChua (
    MaPhieu            INT IDENTITY(1,1) PRIMARY KEY,
    MaThietBi          INT NOT NULL,
    MaKhachHang        INT NOT NULL,
    MaTaiKhoanTiepNhan INT NULL,                        -- NULL khi khach tu dang ky, chua co nhan vien tiep nhan
    NgayTiepNhan       DATETIME NOT NULL DEFAULT GETDATE(),
    NoiDungLoi         NVARCHAR(1000) NOT NULL,
    TrangThai          NVARCHAR(20) NOT NULL DEFAULT N'ChoXuLy',
    MucDo              NVARCHAR(20) NOT NULL DEFAULT N'TrungBinh',
    HanDuKien          DATE NULL,
    NgayHoanThanh      DATETIME NULL,
    LyDoHuyTuChoi      NVARCHAR(500) NULL,
    CONSTRAINT FK_Phieu_ThietBi   FOREIGN KEY (MaThietBi)          REFERENCES ThietBi(MaThietBi),
    CONSTRAINT FK_Phieu_KhachHang FOREIGN KEY (MaKhachHang)        REFERENCES KhachHang(MaKhachHang),
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
    TrangThaiCu        NVARCHAR(20) NULL,               -- NULL khi tao phieu moi
    TrangThaiMoi       NVARCHAR(20) NOT NULL,
    ThoiGian           DATETIME NOT NULL DEFAULT GETDATE(),
    GhiChu             NVARCHAR(500) NULL,
    CONSTRAINT FK_LichSu_Phieu    FOREIGN KEY (MaPhieu)            REFERENCES PhieuSuaChua(MaPhieu),
    CONSTRAINT FK_LichSu_TaiKhoan FOREIGN KEY (MaTaiKhoanThucHien) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_LichSu_Cu  CHECK (TrangThaiCu IS NULL OR TrangThaiCu IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi')),
    CONSTRAINT CK_LichSu_Moi CHECK (TrangThaiMoi IN (N'ChoXuLy', N'DangXuLy', N'HoanThanh', N'Huy', N'TuChoi'))
);

CREATE TABLE ChiTietSuaChua (
    MaChiTiet       INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieu         INT NOT NULL,
    MaKyThuatVien   INT NOT NULL,
    MaLinhKien      INT NULL,                           -- NULL neu chi tinh cong chan doan/sua
    SoLuong         INT NOT NULL DEFAULT 1,
    DonGia          DECIMAL(18,2) NOT NULL DEFAULT 0,
    ThanhTien       DECIMAL(18,2) NOT NULL DEFAULT 0,   -- Controller gan = SoLuong * DonGia, khong cho nhap tay
    KetQuaChanDoan  NVARCHAR(1000) NULL,
    GhiChu          NVARCHAR(500) NULL,
    CONSTRAINT FK_CT_Phieu    FOREIGN KEY (MaPhieu)       REFERENCES PhieuSuaChua(MaPhieu),
    CONSTRAINT FK_CT_KTV      FOREIGN KEY (MaKyThuatVien) REFERENCES KyThuatVien(MaKyThuatVien),
    CONSTRAINT FK_CT_LinhKien FOREIGN KEY (MaLinhKien)    REFERENCES LinhKien(MaLinhKien),
    CONSTRAINT CK_CT_SoLuong  CHECK (SoLuong > 0),
    CONSTRAINT CK_CT_DonGia   CHECK (DonGia >= 0),
    CONSTRAINT CK_CT_ThanhTien CHECK (ThanhTien = SoLuong * DonGia)
);

/* =====================================================================
   NANG CAO: TepDinhKem, ThongBao
   ===================================================================== */
CREATE TABLE TepDinhKem (
    MaTepDinhKem     INT IDENTITY(1,1) PRIMARY KEY,
    LoaiDoiTuong     NVARCHAR(20) NOT NULL,
    MaDoiTuong       INT NOT NULL,                      -- tham chieu da hinh theo LoaiDoiTuong, KHONG co FK that
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
    CONSTRAINT FK_TB_Phieu    FOREIGN KEY (MaPhieu)        REFERENCES PhieuSuaChua(MaPhieu),
    CONSTRAINT CK_TB_Kenh      CHECK (Kenh IN (N'TrongHeThong', N'Email')),
    CONSTRAINT CK_TB_TrangThai CHECK (TrangThai IN (N'ChoGui', N'DaGui', N'Loi'))
);
GO

/* =====================================================================
   INDEX HO TRO TIM KIEM / LOC / THONG KE (khop khoi indexes trong schema.dbml)
   ===================================================================== */
CREATE INDEX IX_ThietBi_KhachHang  ON ThietBi(MaKhachHang);
CREATE INDEX IX_ThietBi_Loai       ON ThietBi(MaLoai);
CREATE INDEX IX_ThietBi_TrangThai  ON ThietBi(TrangThai);
CREATE INDEX IX_ThietBi_Loc        ON ThietBi(MaLoai, MaKhachHang, TrangThai);  -- loc ket hop nhieu dieu kien
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
