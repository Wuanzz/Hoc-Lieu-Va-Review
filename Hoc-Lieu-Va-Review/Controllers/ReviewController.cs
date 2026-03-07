using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Services; // Bổ sung thư viện Services để gọi AI

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService; // Khai báo trợ lý AI

        // Tiêm AI vào Controller
        public ReviewController(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        // Hiển thị danh sách các bài Review
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách review, include thêm thông tin Môn học và Người đăng để hiển thị
            var reviews = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                // CHỈ LẤY CÁC BÀI REVIEW HỢP LỆ (Đã được AI hoặc Giảng viên duyệt)
                .Where(r => r.TrangThaiDuyet == "HopLe" || r.TrangThaiDuyet == "DaDuyet")
                .OrderByDescending(r => r.NgayDang)
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

                // MỜI TRỢ LÝ AI VÀO DUYỆT BÀI REVIEW
                string ketQuaDuyet = await _geminiService.KiemDuyetVanBan(review.NoiDung);
                review.TrangThaiDuyet = ketQuaDuyet;

                _context.Add(review);
                await _context.SaveChangesAsync();

                // Gửi thông báo bằng TempData để hiện popup xanh/đỏ bên ngoài giao diện
                if (ketQuaDuyet == "TuChoi")
                {
                    TempData["ThongBaoReview"] = "❌ Bài đánh giá chứa nội dung vi phạm và đã bị AI chặn!";
                }
                else if (ketQuaDuyet == "ChoDuyet")
                {
                    TempData["ThongBaoReview"] = "⏳ Bài đánh giá có từ ngữ lạ, đang chờ Giảng viên duyệt.";
                }
                else
                {
                    TempData["ThongBaoReview"] = "✅ Đăng bài đánh giá thành công!";
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan", review.HocPhanID);
            return View(review);
        }
    }
}