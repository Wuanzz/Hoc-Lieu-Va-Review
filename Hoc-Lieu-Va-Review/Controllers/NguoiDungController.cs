using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Controllers
{
    // Chỉ ai có VaiTro = "Admin" mới được gọi các hàm trong Controller này
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NguoiDungController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách toàn bộ người dùng
        public async Task<IActionResult> Index(string timKiem, int page = 1)
        {
            int pageSize = 8; // Danh sách tài khoản thường dài

            var query = _context.NguoiDungs.AsQueryable();

            // LỌC TÌM KIẾM THEO TÊN HOẶC EMAIL
            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(nd => nd.HoTen.Contains(timKiem) || nd.Email.Contains(timKiem));
                ViewBag.TuKhoa = timKiem; // Giữ lại từ khóa
            }

            // THUẬT TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Cắt dữ liệu theo trang
            var danhSachNguoiDung = await query
                .OrderByDescending(nd => nd.NgayDangKy) // Xếp tài khoản mới đăng ký lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(danhSachNguoiDung);
        }

        // [GET] Mở form chỉnh sửa Quyền và Trạng thái
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        // [POST] Lưu lại các chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NguoiDungID,VaiTro,TrangThai")] NguoiDung inputData)
        {
            // Tìm user gốc trong DB
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound();

            // Admin chỉ được phép sửa 2 cột này, không được đổi Tên, Email hay Mật khẩu của người ta
            nguoiDung.VaiTro = inputData.VaiTro;
            nguoiDung.TrangThai = inputData.TrangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}