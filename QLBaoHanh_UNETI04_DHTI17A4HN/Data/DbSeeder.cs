using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Data
{
    // Dữ liệu mẫu theo mục 16 của đề:
    // 5 loại thiết bị, 15 thiết bị, 30 khách hàng, 2 admin, 3 nhân viên,
    // 45 phiếu sửa chữa (nhiều trạng thái), 18 chi tiết sửa chữa.
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Đã có dữ liệu thì không seed lại
            if (db.TaiKhoans.Any()) return;

            // ===== 1. Tài khoản: 2 Admin, 3 NhanVien, 3 KhachHang (1 tài khoản bị khóa) =====
            // Mật khẩu mẫu là 123456 (chưa băm). Nếu Module 1 làm băm mật khẩu thì sửa lại chỗ này.
            var taiKhoans = new List<TaiKhoan>
            {
                TaoTaiKhoan("admin", "Quản trị viên", "Admin"),
                TaoTaiKhoan("admin2", "Quản trị viên 2", "Admin"),
                TaoTaiKhoan("nhanvien1", "Nguyễn Văn Kỹ", "NhanVien"),
                TaoTaiKhoan("nhanvien2", "Trần Thị Thuật", "NhanVien"),
                TaoTaiKhoan("nhanvien3", "Lê Minh Sửa", "NhanVien"),
                TaoTaiKhoan("khachhang1", "Phạm Hoàng An", "KhachHang"),
                TaoTaiKhoan("khachhang2", "Vũ Thị Bình", "KhachHang"),
                TaoTaiKhoan("khachhang3", "Hoàng Văn Cường", "KhachHang", trangThai: false), // tài khoản bị khóa
            };
            db.TaiKhoans.AddRange(taiKhoans);
            db.SaveChanges();

            var nhanViens = taiKhoans.Where(t => t.VaiTro == "NhanVien").ToList();
            var tkKhachHangs = taiKhoans.Where(t => t.VaiTro == "KhachHang").ToList();

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
                    LoaiKhachHang = doanhNghiep ? "Doanh nghiệp" : "Cá nhân",
                    MaSoThue = doanhNghiep ? "0100" + i.ToString("D6") : null,
                    TrangThai = i != 30,   // khách thứ 30 ngừng hoạt động
                });
            }
            db.KhachHangs.AddRange(khachHangs);
            db.SaveChanges();

            // ===== 4. Thiết bị (15): lẫn lộn còn/hết hạn bảo hành và nhiều trạng thái =====
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
                    TrangThai = i % 5 == 0 ? "Ngừng sử dụng" : i % 3 == 0 ? "Đang sửa chữa" : "Khả dụng",
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

            // ===== 6. Phiếu sửa chữa (45), chia đều 4 trạng thái =====
            var trangThaiPhieu = new[] { "Chờ xử lý", "Đang xử lý", "Hoàn thành", "Hủy" };
            var mucDo = new[] { "Thấp", "Trung bình", "Cao" };
            var noiDungLoi = new[]
            {
                "Không lên nguồn",
                "Màn hình bị vỡ hoặc sọc",
                "Pin chai, sạc không vào",
                "Máy chạy chậm, tự khởi động lại",
                "Không kết nối được mạng",
            };
            var phieus = new List<PhieuSuaChua>();
            for (int i = 1; i <= 45; i++)
            {
                var thietBi = thietBis[(i - 1) % thietBis.Count];
                var trangThai = trangThaiPhieu[i % 4];
                var ngayTiepNhan = DateTime.Today.AddDays(-i * 4);
                phieus.Add(new PhieuSuaChua
                {
                    MaThietBi = thietBi.MaThietBi,
                    MaKhachHang = thietBi.MaKhachHang,   // thiết bị phải thuộc đúng khách hàng tạo phiếu
                    NgayTiepNhan = ngayTiepNhan,
                    NoiDungLoi = noiDungLoi[i % noiDungLoi.Length],
                    TrangThai = trangThai,
                    MucDo = mucDo[i % mucDo.Length],
                    HanDuKien = ngayTiepNhan.AddDays(7),
                    NgayHoanThanh = trangThai == "Hoàn thành" ? (DateTime?)ngayTiepNhan.AddDays(5) : null,
                });
            }
            db.PhieuSuaChuas.AddRange(phieus);
            db.SaveChanges();

            // ===== 7. Chi tiết sửa chữa (18): chỉ phiếu đang xử lý hoặc hoàn thành mới có =====
            var phieuCoChiTiet = phieus
                .Where(p => p.TrangThai == "Đang xử lý" || p.TrangThai == "Hoàn thành")
                .Take(18)
                .ToList();

            var chiTiets = new List<ChiTietSuaChua>();
            for (int j = 0; j < phieuCoChiTiet.Count; j++)
            {
                LinhKien? linhKien = j % 6 == 5 ? null : linhKiens[j % linhKiens.Count];
                int soLuong = 1 + j % 3;
                decimal donGia = linhKien?.DonGia ?? 150_000m;   // không dùng linh kiện thì tính tiền công
                chiTiets.Add(new ChiTietSuaChua
                {
                    MaPhieu = phieuCoChiTiet[j].MaPhieu,
                    MaKyThuatVien = nhanViens[j % nhanViens.Count].MaTaiKhoan,
                    MaLinhKien = linhKien?.MaLinhKien,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    ThanhTien = soLuong * donGia,
                    KetQuaChanDoan = linhKien == null ? "Vệ sinh, cập nhật phần mềm" : $"Thay {linhKien.TenLinhKien}",
                });
            }
            db.ChiTietSuaChuas.AddRange(chiTiets);
            db.SaveChanges();
        }

        private static TaiKhoan TaoTaiKhoan(string tenDangNhap, string hoTen, string vaiTro, bool trangThai = true)
        {
            return new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = "123456",
                HoTen = hoTen,
                Email = $"{tenDangNhap}@baohanh.vn",
                VaiTro = vaiTro,
                TrangThai = trangThai,
            };
        }
    }
}
