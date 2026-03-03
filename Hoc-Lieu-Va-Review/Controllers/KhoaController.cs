using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hoc_Lieu_Va_Review.Models;

namespace Hoc_Lieu_Va_Review_Demooo.Controllers
{
    // Yêu cầu phải đăng nhập mới được vào trang này
    [Authorize]
    public class KhoaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhoaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách Khoa
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách Khoa từ Database
            var danhSachKhoa = await _context.Khoas.ToListAsync();
            return View(danhSachKhoa);
        }
    }
}