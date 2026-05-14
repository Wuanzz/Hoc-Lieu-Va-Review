using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hoc_Lieu_Va_Review.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,GiangVien")] // Cho phép cả Admin và Giảng viên vào xem thống kê
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // LẤY CÁC CON SỐ THỐNG KÊ TỔNG QUAN
            ViewBag.TongSoNguoiDung = await _context.NguoiDungs.CountAsync();
            ViewBag.TongSoTaiLieu = await _context.TaiLieus.CountAsync();
            ViewBag.TongSoReview = await _context.Reviews.CountAsync();

            // Đếm số tài liệu đang chờ duyệt (Quan trọng cho Giảng viên)
            ViewBag.TaiLieuChoDuyet = await _context.TaiLieus.CountAsync(t => t.TrangThaiDuyet == "ChoDuyet");

            // CHUẨN BỊ DỮ LIỆU VẼ BIỂU ĐỒ CỘT (Số lượng tài liệu theo Ngành học)
            var thongKeNganh = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .ThenInclude(h => h.Nganh)
                .Where(t => t.HocPhan != null && t.HocPhan.Nganh != null)
                .GroupBy(t => t.HocPhan.Nganh.TenNganh)
                .Select(g => new { TenNganh = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();

            ViewBag.TenNganhChart = thongKeNganh.Select(x => x.TenNganh).ToList();
            ViewBag.SoLuongChart = thongKeNganh.Select(x => x.SoLuong).ToList();

            // DỮ LIỆU VẼ BIỂU ĐỒ TRÒN 
            var thongKeRole = await _context.NguoiDungs
                .GroupBy(u => u.VaiTro)
                .Select(g => new { VaiTro = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            ViewBag.RolesChart = thongKeRole.Select(x => x.VaiTro).ToList();
            ViewBag.RolesData = thongKeRole.Select(x => x.SoLuong).ToList();

            return View();
        }
    }
}