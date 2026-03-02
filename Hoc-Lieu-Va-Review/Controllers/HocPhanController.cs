using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hoc_Lieu_Va_Review.Controllers
{
    public class HocPhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HocPhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Học Phần
        public async Task<IActionResult> Index(string timKiem, int page = 1)
        {
            int pageSize = 8; // Quản lý Học phần dữ liệu thường nhiều hơn

            // Lấy danh sách Học phần (kèm theo thông tin Ngành nếu có)
            var query = _context.HocPhans.Include(h => h.Nganh).AsQueryable();

            // LỌC TÌM KIẾM THEO TÊN HỌC PHẦN
            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(h => h.TenHocPhan.Contains(timKiem));
                ViewBag.TuKhoa = timKiem; // Giữ lại từ khóa trên ô tìm kiếm
            }

            // THUẬT TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Cắt dữ liệu theo trang
            var danhSachHocPhan = await query
                .OrderBy(h => h.HocPhanID) // Sắp xếp theo ID
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(danhSachHocPhan);
        }
    }
}
