using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;

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

        // MỞ FORM ĐỔI MẬT KHẨU 
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        // XỬ LÝ ĐỔI MẬT KHẨU 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            // Lấy ID người dùng đang đăng nhập
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            // KIỂM TRA LOGIC BẢO MẬT
            if (nguoiDung.MatKhau != matKhauCu)
            {
                ViewBag.Loi = " Mật khẩu cũ không chính xác!";
                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Loi = " Mật khẩu xác nhận không khớp với mật khẩu mới!";
                return View();
            }

            if (matKhauMoi.Length < 6)
            {
                ViewBag.Loi = " Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View();
            }

            // Nếu qua hết các bài test thì lưu mật khẩu mới
            nguoiDung.MatKhau = matKhauMoi;
            await _context.SaveChangesAsync();

            // Gửi thông báo thành công ra ngoài
            TempData["ThongBaoDoiMatKhau"] = "Đổi mật khẩu thành công! Lần đăng nhập sau hãy dùng mật khẩu mới nhé.";

            // Đổi xong thì đá về lại trang Hồ Sơ chính
            return RedirectToAction(nameof(Index));
        }


        // CẬP NHẬT ẢNH ĐẠI DIỆN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatAnhDaiDien(IFormFile fileDaiDien)
        {
            if (fileDaiDien == null || fileDaiDien.Length == 0)
            {
                TempData["LoiHoSo"] = "Vui lòng chọn một ảnh trước khi tải lên!";
                return RedirectToAction(nameof(Index));
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null) return NotFound();

            // 1. Tạo thư mục chứa ảnh nếu chưa có
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 2. Đổi tên file để không bị trùng lặp (dùng Guid)
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileDaiDien.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Copy file vào thư mục wwwroot
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileDaiDien.CopyToAsync(fileStream);
            }

            // 4. Lưu đường dẫn vào Database
            nguoiDung.AnhDaiDien = "/uploads/avatars/" + uniqueFileName;
            await _context.SaveChangesAsync();

            TempData["ThongBaoHoSo"] = "Cập nhật ảnh đại diện thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}