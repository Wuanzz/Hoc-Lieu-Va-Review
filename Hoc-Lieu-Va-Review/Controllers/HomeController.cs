using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoc_Lieu_Va_Review.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DbContext vào để kết nối Database
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 6 Tài liệu mới nhất (CHỈ LẤY TÀI LIỆU HỢP LỆ)
            var taiLieuMoi = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Where(t => t.TrangThaiDuyet == "HopLe")
                .OrderByDescending(t => t.NgayUpload)
                .Take(6) // Chỉ lấy 6 cái mới nhất cho đỡ chật trang
                .ToListAsync();

            // Lấy 4 Review mới nhất
            var reviewMoi = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                .OrderByDescending(r => r.NgayDang)
                .Take(4) // Lấy 4 bài
                .ToListAsync();

            // Gửi sang View
            ViewBag.TaiLieuMoi = taiLieuMoi;
            ViewBag.ReviewMoi = reviewMoi;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
