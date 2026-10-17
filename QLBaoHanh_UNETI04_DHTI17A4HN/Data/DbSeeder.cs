// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Dữ liệu mẫu cho hệ thống (nguồn dữ liệu mẫu duy nhất, xem Doc/THIETKE.md mục 6).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Helpers;
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Data
{
    // Dữ liệu mẫu theo mục 16 của đề:
    // 5 loại thiết bị, 15 thiết bị, 30 khách hàng, 2 admin, 3 nhân viên (kèm 3 hồ sơ kỹ thuật viên),
    // 45 phiếu sửa chữa (đủ 5 trạng thái), 18 chi tiết sửa chữa, kèm lịch sử trạng thái của từng phiếu.
    //
    // Dữ liệu được dựng cho khớp luồng nghiệp vụ trong Doc/THIETKE.md (mục 2.4, 2.5):
    // mỗi thiết bị có 3 phiếu, chỉ phiếu mới nhất mới có thể đang mở (ChoXuLy/DangXuLy), 2 phiếu cũ đều đã đóng.
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Đã có dữ liệu thì không seed lại
            if (db.TaiKhoans.Any()) return;

            // ===== 1. Tài khoản: 2 Admin, 3 NhanVien, 3 KhachHang (1 tài khoản bị khóa) =====
            // Mật khẩu mẫu là 123456, được băm bằng PasswordHasher<TaiKhoan> (MatKhauHelper) trong TaoTaiKhoan.
            var taiKhoans = new List<TaiKhoan>
            {
                TaoTaiKhoan("admin", "Quản trị viên", VaiTroEnum.Admin),
                TaoTaiKhoan("admin2", "Quản trị viên 2", VaiTroEnum.Admin),
                TaoTaiKhoan("nhanvien1", "Nguyễn Văn Kỹ", VaiTroEnum.NhanVien),
                TaoTaiKhoan("nhanvien2", "Trần Thị Thuật", VaiTroEnum.NhanVien),
                TaoTaiKhoan("nhanvien3", "Lê Minh Sửa", VaiTroEnum.NhanVien),
                TaoTaiKhoan("khachhang1", "Phạm Hoàng An", VaiTroEnum.KhachHang),
                TaoTaiKhoan("khachhang2", "Vũ Thị Bình", VaiTroEnum.KhachHang),
                TaoTaiKhoan("khachhang3", "Hoàng Văn Cường", VaiTroEnum.KhachHang, trangThai: false), // tài khoản bị khóa
            };
            db.TaiKhoans.AddRange(taiKhoans);
            db.SaveChanges();

            var nhanViens = taiKhoans.Where(t => t.VaiTro == VaiTroEnum.NhanVien).ToList();
            var tkKhachHangs = taiKhoans.Where(t => t.VaiTro == VaiTroEnum.KhachHang).ToList();

            // ===== 1b. Hồ sơ kỹ thuật viên: mỗi tài khoản NhanVien có 1 hồ sơ =====
            var chuyenMon = new[] { "Điện thoại, máy tính bảng", "Laptop, máy in", "Tivi, thiết bị điện tử" };
            var kyThuatViens = new List<KyThuatVien>();
            for (int k = 0; k < nhanViens.Count; k++)
            {
                kyThuatViens.Add(new KyThuatVien
                {
                    MaTaiKhoan = nhanViens[k].MaTaiKhoan,
                    HoTen = nhanViens[k].HoTen,
                    ChuyenMon = chuyenMon[k % chuyenMon.Length],
                    SoDienThoai = "0901000" + (k + 1).ToString("D3"),
                    Email = nhanViens[k].Email,
                    TrangThai = true,
                });
            }
            db.KyThuatViens.AddRange(kyThuatViens);
            db.SaveChanges();

            // ===== 2. Loại thiết bị (5) =====
            var loais = new List<LoaiThietBi>
            {
                new() { TenLoai = "Điện thoại", HangSanXuat = "Samsung", MoTa = "Điện thoại thông minh", ThoiHanBaoHanhMacDinh = 12, TrangThai = true },
                new() { TenLoai = "Laptop", HangSanXuat = "Dell", MoTa = "Máy tính xách tay", ThoiHanBaoHanhMacDinh = 24, TrangThai = true },
                new() { TenLoai = "Máy tính bảng", HangSanXuat = "Apple", MoTa = "Máy tính bảng", ThoiHanBaoHanhMacDinh = 12, TrangThai = true },
                new() { TenLoai = "Tivi", HangSanXuat = "LG", MoTa = "Tivi thông minh", ThoiHanBaoHanhMacDinh = 24, TrangThai = true },
                new() { TenLoai = "Máy in", HangSanXuat = "Canon", MoTa = "Máy in văn phòng", ThoiHanBaoHanhMacDinh = 12, TrangThai = true },
            };
            db.LoaiThietBis.AddRange(loais);
            db.SaveChanges();

            // ===== 3. Khách hàng (30): 3 khách đầu liên kết tài khoản, mỗi khách thứ 5 là doanh nghiệp =====
            var ho = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ" };
            var ten = new[] { "An", "Bình", "Cường", "Dũng", "Hà", "Hùng", "Lan", "Mai", "Nam", "Phúc" };
            var khachHangs = new List<KhachHang>();
            for (int i = 1; i <= 30; i++)
            {
                bool doanhNghiep = i % 5 == 0;
                khachHangs.Add(new KhachHang
                {
                    MaTaiKhoan = i <= tkKhachHangs.Count ? (int?)tkKhachHangs[i - 1].MaTaiKhoan : null,
                    HoTen = $"{ho[i % ho.Length]} {ten[(i * 3) % ten.Length]}",
                    SoDienThoai = "09" + i.ToString("D8"),
                    Email = $"khachhang{i}@mail.com",
                    DiaChi = $"Số {i} đường Nguyễn Trãi, Hà Nội",
                    CCCD = "001" + i.ToString("D9"),
                    NgayTao = DateTime.Today.AddDays(-i * 7),
                    LoaiKhachHang = doanhNghiep ? LoaiKhachHangEnum.DoanhNghiep : LoaiKhachHangEnum.CaNhan,
                    MaSoThue = doanhNghiep ? "0100" + i.ToString("D6") : null,
                    TrangThai = i != 30,   // khách thứ 30 ngừng hoạt động
                });
            }
            db.KhachHangs.AddRange(khachHangs);
            db.SaveChanges();

            // ===== 4. Thiết bị (15): lẫn lộn còn/hết hạn bảo hành =====
            // Trạng thái thiết bị tạm đặt KhaDung, sẽ cập nhật theo phiếu mới nhất ở bước 6.
            var thietBis = new List<ThietBi>();
            for (int i = 1; i <= 15; i++)
            {
                var loai = loais[(i - 1) % loais.Count];
                var ngayMua = DateTime.Today.AddMonths(-i * 2);
                thietBis.Add(new ThietBi
                {
                    MaLoai = loai.MaLoai,
                    MaKhachHang = khachHangs[i - 1].MaKhachHang,
                    SerialNumber = $"SN{i:D6}",
                    TenThietBi = $"{loai.TenLoai} {loai.HangSanXuat} #{i}",
                    NgayMua = ngayMua,
                    HanBaoHanh = ngayMua.AddMonths(loai.ThoiHanBaoHanhMacDinh),
                    MoTa = $"Thiết bị mẫu số {i}",
                    TrangThai = TrangThaiThietBiEnum.KhaDung,
                    GiaTri = 3_000_000m + i * 1_500_000m,
                });
            }
            db.ThietBis.AddRange(thietBis);
            db.SaveChanges();

            // ===== 5. Linh kiện (8) =====
            var linhKiens = new List<LinhKien>
            {
                new() { TenLinhKien = "Màn hình LCD", DonViTinh = "Cái", DonGia = 1_200_000m, SoLuongTon = 15, TrangThai = true },
                new() { TenLinhKien = "Pin", DonViTinh = "Cái", DonGia = 350_000m, SoLuongTon = 40, TrangThai = true },
                new() { TenLinhKien = "Bàn phím", DonViTinh = "Cái", DonGia = 250_000m, SoLuongTon = 25, TrangThai = true },
                new() { TenLinhKien = "Ổ cứng SSD 256GB", DonViTinh = "Cái", DonGia = 800_000m, SoLuongTon = 20, TrangThai = true },
                new() { TenLinhKien = "RAM 8GB", DonViTinh = "Thanh", DonGia = 600_000m, SoLuongTon = 30, TrangThai = true },
                new() { TenLinhKien = "Bộ sạc", DonViTinh = "Bộ", DonGia = 200_000m, SoLuongTon = 50, TrangThai = true },
                new() { TenLinhKien = "Mainboard", DonViTinh = "Cái", DonGia = 2_500_000m, SoLuongTon = 5, TrangThai = true },
                new() { TenLinhKien = "Quạt tản nhiệt", DonViTinh = "Cái", DonGia = 180_000m, SoLuongTon = 35, TrangThai = true },
            };
            db.LinhKiens.AddRange(linhKiens);
            db.SaveChanges();

            // ===== 6. Phiếu sửa chữa (45) = 15 thiết bị x 3 phiếu =====
            // Vòng 0 (phiếu mới nhất của mỗi thiết bị): đủ 5 trạng thái, mỗi trạng thái 3 phiếu.
            // Vòng 1, 2 (phiếu cũ): đều đã đóng (HoanThanh/Huy/TuChoi) => mỗi thiết bị tối đa 1 phiếu đang mở.
            var trangThaiMoiNhat = new[]
            {
                TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.DangXuLy, TrangThaiPhieuEnum.HoanThanh,
                TrangThaiPhieuEnum.Huy, TrangThaiPhieuEnum.TuChoi,
            };
            var trangThaiDaDong = new[]
            {
                TrangThaiPhieuEnum.HoanThanh, TrangThaiPhieuEnum.Huy, TrangThaiPhieuEnum.TuChoi,
            };
            var mucDo = new[] { MucDoEnum.Thap, MucDoEnum.TrungBinh, MucDoEnum.Cao, MucDoEnum.KhanCap };
            var noiDungLoi = new[]
            {
                "Không lên nguồn",
                "Màn hình bị vỡ hoặc sọc",
                "Pin chai, sạc không vào",
                "Máy chạy chậm, tự khởi động lại",
                "Không kết nối được mạng",
            };
            var phieus = new List<PhieuSuaChua>();
            for (int i = 0; i < 45; i++)
            {
                int idx = i % 15;    // thiết bị thứ idx
                int vong = i / 15;   // 0 = mới nhất, 1-2 = cũ hơn
                var thietBi = thietBis[idx];
                var trangThai = vong == 0 ? trangThaiMoiNhat[idx % 5] : trangThaiDaDong[(idx + vong) % 3];
                var ngayTiepNhan = DateTime.Today.AddDays(-(vong * 60 + idx * 3 + 1));
                // ChoXuLy chưa ai tiếp nhận; Huy là khách tự hủy khi còn chờ => cũng chưa có nhân viên tiếp nhận
                bool daTiepNhan = trangThai != TrangThaiPhieuEnum.ChoXuLy && trangThai != TrangThaiPhieuEnum.Huy;

                phieus.Add(new PhieuSuaChua
                {
                    MaThietBi = thietBi.MaThietBi,
                    MaKhachHang = thietBi.MaKhachHang,   // thiết bị phải thuộc đúng khách hàng tạo phiếu
                    MaTaiKhoanTiepNhan = daTiepNhan ? nhanViens[i % nhanViens.Count].MaTaiKhoan : null,
                    NgayTiepNhan = ngayTiepNhan,
                    NoiDungLoi = noiDungLoi[i % noiDungLoi.Length],
                    TrangThai = trangThai,
                    MucDo = mucDo[i % mucDo.Length],
                    HanDuKien = ngayTiepNhan.AddDays(7),
                    NgayHoanThanh = trangThai == TrangThaiPhieuEnum.HoanThanh ? (DateTime?)ngayTiepNhan.AddDays(5) : null,
                    LyDoHuyTuChoi = trangThai == TrangThaiPhieuEnum.Huy ? "Khách hàng tự hủy phiếu"
                                  : trangThai == TrangThaiPhieuEnum.TuChoi ? "Thiết bị hết bảo hành, không thuộc phạm vi sửa chữa"
                                  : null,
                });
            }
            db.PhieuSuaChuas.AddRange(phieus);
            db.SaveChanges();

            // Trạng thái thiết bị theo phiếu mới nhất (phieus[idx] chính là phiếu vòng 0 của thiết bị idx)
            for (int idx = 0; idx < thietBis.Count; idx++)
            {
                thietBis[idx].TrangThai = phieus[idx].TrangThai switch
                {
                    TrangThaiPhieuEnum.DangXuLy => TrangThaiThietBiEnum.DangSuaChua,
                    TrangThaiPhieuEnum.TuChoi => TrangThaiThietBiEnum.KhongKhaDung,
                    _ => TrangThaiThietBiEnum.KhaDung,
                };
            }
            db.SaveChanges();

            // ===== 7. Lịch sử trạng thái: mỗi phiếu có đủ các bước từ lúc tạo đến trạng thái hiện tại =====
            var khachTheoMa = khachHangs.ToDictionary(k => k.MaKhachHang);
            var lichSus = new List<LichSuTrangThaiPhieu>();

            void ThemLichSu(PhieuSuaChua p, TrangThaiPhieuEnum? cu, TrangThaiPhieuEnum moi, int nguoiThucHien, DateTime luc, string? ghiChu = null)
            {
                lichSus.Add(new LichSuTrangThaiPhieu
                {
                    MaPhieu = p.MaPhieu,
                    MaTaiKhoanThucHien = nguoiThucHien,
                    TrangThaiCu = cu,
                    TrangThaiMoi = moi,
                    ThoiGian = luc,
                    GhiChu = ghiChu,
                });
            }

            foreach (var p in phieus)
            {
                // Người tạo phiếu: tài khoản của khách (nếu có), không thì nhân viên nhập hộ
                int nguoiTao = khachTheoMa[p.MaKhachHang].MaTaiKhoan ?? nhanViens[0].MaTaiKhoan;
                int nhanVien = p.MaTaiKhoanTiepNhan ?? nhanViens[0].MaTaiKhoan;

                ThemLichSu(p, null, TrangThaiPhieuEnum.ChoXuLy, nguoiTao, p.NgayTiepNhan, "Tạo phiếu sửa chữa");
                switch (p.TrangThai)
                {
                    case TrangThaiPhieuEnum.DangXuLy:
                        ThemLichSu(p, TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.DangXuLy, nhanVien, p.NgayTiepNhan.AddHours(2), "Tiếp nhận xử lý");
                        break;
                    case TrangThaiPhieuEnum.HoanThanh:
                        ThemLichSu(p, TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.DangXuLy, nhanVien, p.NgayTiepNhan.AddHours(2), "Tiếp nhận xử lý");
                        ThemLichSu(p, TrangThaiPhieuEnum.DangXuLy, TrangThaiPhieuEnum.HoanThanh, nhanVien, p.NgayHoanThanh!.Value, "Hoàn tất sửa chữa");
                        break;
                    case TrangThaiPhieuEnum.Huy:
                        ThemLichSu(p, TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.Huy, nguoiTao, p.NgayTiepNhan.AddHours(1), p.LyDoHuyTuChoi);
                        break;
                    case TrangThaiPhieuEnum.TuChoi:
                        ThemLichSu(p, TrangThaiPhieuEnum.ChoXuLy, TrangThaiPhieuEnum.TuChoi, nhanVien, p.NgayTiepNhan.AddHours(1), p.LyDoHuyTuChoi);
                        break;
                }
            }
            db.LichSuTrangThaiPhieus.AddRange(lichSus);
            db.SaveChanges();

            // ===== 8. Chi tiết sửa chữa (18): chỉ phiếu đang xử lý hoặc hoàn thành mới có =====
            var phieuCoChiTiet = phieus
                .Where(p => p.TrangThai == TrangThaiPhieuEnum.DangXuLy || p.TrangThai == TrangThaiPhieuEnum.HoanThanh)
                .ToList();   // 16 phiếu, 2 phiếu đầu có thêm 1 dòng tiền công => 18 dòng

            var chiTiets = new List<ChiTietSuaChua>();
            for (int j = 0; j < phieuCoChiTiet.Count; j++)
            {
                var kyThuatVien = kyThuatViens[j % kyThuatViens.Count];
                LinhKien? linhKien = j % 6 == 5 ? null : linhKiens[j % linhKiens.Count];
                int soLuong = 1 + j % 3;
                decimal donGia = linhKien?.DonGia ?? 150_000m;   // không dùng linh kiện thì tính tiền công
                chiTiets.Add(new ChiTietSuaChua
                {
                    MaPhieu = phieuCoChiTiet[j].MaPhieu,
                    MaKyThuatVien = kyThuatVien.MaKyThuatVien,
                    MaLinhKien = linhKien?.MaLinhKien,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    ThanhTien = soLuong * donGia,   // Controller gán = SoLuong * DonGia (THIETKE.md mục 2.8)
                    KetQuaChanDoan = linhKien == null ? "Vệ sinh, cập nhật phần mềm" : $"Thay {linhKien.TenLinhKien}",
                });

                if (j < 2)
                {
                    chiTiets.Add(new ChiTietSuaChua
                    {
                        MaPhieu = phieuCoChiTiet[j].MaPhieu,
                        MaKyThuatVien = kyThuatVien.MaKyThuatVien,
                        MaLinhKien = null,
                        SoLuong = 1,
                        DonGia = 150_000m,
                        ThanhTien = 150_000m,
                        KetQuaChanDoan = "Công sửa chữa",
                    });
                }
            }
            db.ChiTietSuaChuas.AddRange(chiTiets);
            db.SaveChanges();
        }

        private static TaiKhoan TaoTaiKhoan(string tenDangNhap, string hoTen, VaiTroEnum vaiTro, bool trangThai = true)
        {
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                HoTen = hoTen,
                Email = $"{tenDangNhap}@baohanh.vn",
                VaiTro = vaiTro,
                TrangThai = trangThai,
            };
            taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, "123456");
            return taiKhoan;
        }
    }
}
