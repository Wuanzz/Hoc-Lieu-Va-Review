using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize] // Ai đăng nhập rồi cũng được vào đây
    public class HoSoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoSoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy ID của người đang đăng nhập
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            // Lấy thông tin cá nhân
            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            // Lấy danh sách Tài liệu MÀ NGƯỜI NÀY ĐÃ ĐĂNG
            var taiLieuCuaToi = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Where(t => t.NguoiDungID == userId)
                .OrderByDescending(t => t.NgayUpload)
                .ToListAsync();

            // Lấy danh sách Review MÀ NGƯỜI NÀY ĐÃ VIẾT
            var reviewCuaToi = await _context.Reviews
                .Include(r => r.HocPhan)
                .Where(r => r.NguoiDungID == userId)
                .OrderByDescending(r => r.NgayDang)
                .ToListAsync();

            ViewBag.TaiLieuCuaToi = taiLieuCuaToi;
            ViewBag.ReviewCuaToi = reviewCuaToi;

            return View(nguoiDung);
        }
    }
}