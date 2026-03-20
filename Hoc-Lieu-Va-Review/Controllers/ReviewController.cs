using Hoc_Lieu_Va_Review.Models;
using Hoc_Lieu_Va_Review.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService; // 1. Khai báo trợ lý AI

        // 2. Tiêm AI vào Controller
        public ReviewController(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        // 1. Hiển thị danh sách các bài Review
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                // 3. CHỈ LẤY CÁC BÀI REVIEW HỢP LỆ (Đã được AI hoặc Giảng viên duyệt)
                .Where(r => r.TrangThaiDuyet == "HopLe" || r.TrangThaiDuyet == "DaDuyet")
                .OrderByDescending(r => r.NgayDang)
                .ToListAsync();
            return View(reviews);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HocPhanID,NoiDung,SoSao")] Review review)
        {
            ModelState.Remove("NguoiDung");
            ModelState.Remove("HocPhan");
            ModelState.Remove("TrangThaiDuyet");

            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null)
                {
                    review.NguoiDungID = int.Parse(userIdClaim.Value);
                }

                review.NgayDang = DateTime.Now;

                // 4. MỜI TRỢ LÝ AI VÀO DUYỆT BÀI REVIEW
                string ketQuaDuyet = await _geminiService.KiemDuyetVanBan(review.NoiDung);
                review.TrangThaiDuyet = ketQuaDuyet;

                _context.Add(review);
                await _context.SaveChangesAsync();

                // 5. Gửi thông báo bằng TempData để hiện popup xanh/đỏ bên ngoài giao diện
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

        // HÀM MỞ TRANG CHI TIẾT REVIEW 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Lấy bài Review cùng thông tin Môn học và Người viết
            var review = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                .FirstOrDefaultAsync(m => m.ReviewID == id);

            if (review == null) return NotFound();

            // Lấy danh sách bình luận CỦA BÀI REVIEW NÀY (Chỉ lấy bình luận hợp lệ)
            // Lưu ý: Cột ReviewID trong bảng BinhLuan phải cho phép null (int?) nhé
            var danhSachBinhLuan = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Where(b => b.ReviewID == id && (b.TrangThaiDuyet == "HopLe" || b.TrangThaiDuyet == "DaDuyet"))
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            ViewBag.DanhSachBinhLuan = danhSachBinhLuan;

            return View(review);
        }

        // HÀM XỬ LÝ GỬI BÌNH LUẬN (CÓ AI KIỂM DUYỆT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bổ sung thêm tham số ParentID
        public async Task<IActionResult> AddComment(int ReviewID, string NoiDung, int? ParentID)
        {
            if (string.IsNullOrWhiteSpace(NoiDung)) return RedirectToAction(nameof(Details), new { id = ReviewID });

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                // Vẫn qua AI duyệt như bình thường
                string ketQuaDuyet = await _geminiService.KiemDuyetVanBan(NoiDung);

                var binhLuan = new BinhLuan
                {
                    ReviewID = ReviewID,
                    NoiDung = NoiDung,
                    NgayDang = DateTime.Now,
                    TrangThaiDuyet = ketQuaDuyet,
                    NguoiDungID = int.Parse(userIdClaim.Value),
                    ParentID = ParentID // Gắn ID của bình luận cha vào đây
                };

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();

                if (ketQuaDuyet == "TuChoi")
                {
                    TempData["ThongBaoBinhLuan"] = "❌ Bình luận của bạn chứa từ ngữ vi phạm và đã bị AI tự động chặn!";
                }
                else if (ketQuaDuyet == "ChoDuyet")
                {
                    TempData["ThongBaoBinhLuan"] = "⏳ Bình luận có từ ngữ lạ, đang chờ Giảng viên duyệt.";
                }
            }

            return RedirectToAction(nameof(Details), new { id = ReviewID });
        }
    }
}