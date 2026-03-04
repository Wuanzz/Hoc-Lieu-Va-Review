using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
namespace Hoc_Lieu_Va_Review.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách các bài Review
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách review, include thêm thông tin Môn học và Người đăng để hiển thị
            var reviews = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                .OrderByDescending(r => r.NgayDang) // Sắp xếp bài mới nhất lên đầu
                .ToListAsync();
            return View(reviews);
        }

        // [GET] Hiển thị form Viết Review
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan");
            return View();
        }

        // [POST] Lưu Review vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HocPhanID,NoiDung,SoSao")] Review review)
        {
            // Bỏ qua validate các trường gán tự động để không bị lỗi "bấm nút im re"
            ModelState.Remove("NguoiDung");
            ModelState.Remove("HocPhan");
            ModelState.Remove("TrangThaiDuyet");

            if (ModelState.IsValid)
            {
                // Lấy ID người dùng đang đăng nhập
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null)
                {
                    review.NguoiDungID = int.Parse(userIdClaim.Value);
                }

                review.NgayDang = DateTime.Now;

                // Tạm thời để trạng thái "DaDuyet" (Đã duyệt) để test hiển thị luôn cho dễ nhé
                review.TrangThaiDuyet = "DaDuyet";

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan", review.HocPhanID);
            return View(review);
        }
    }
}
