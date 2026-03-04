using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hoc_Lieu_Va_Review.Models;

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
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.NguoiDungs
                .OrderByDescending(n => n.NgayDangKy) // Ai mới đăng ký xếp lên đầu
                .ToListAsync();
            return View(danhSach);
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