using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hoc_Lieu_Va_Review.Models;

namespace Hoc_Lieu_Va_Review.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GiangVien")]
    public class KiemDuyetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KiemDuyetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị Dashboard Kiểm duyệt (Gồm cả Tài liệu chờ duyệt & Báo cáo)
        public async Task<IActionResult> Index()
        {
            // Tài liệu chờ duyệt
            var taiLieuChoDuyet = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Where(t => t.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(t => t.NgayUpload)
                .ToListAsync();

            // Báo cáo chờ xử lý
            var danhSachBaoCao = await _context.BaoCaos
                .Include(b => b.TaiLieu)       
                .Include(b => b.NguoiDung)     
                .Where(b => b.TrangThaiXuLy == "ChoXuLy")
                .OrderBy(b => b.NgayBaoCao)
                .ToListAsync();

            // Bình luận bị AI nghi ngờ (ChoDuyet)
            var binhLuanChoDuyet = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Include(b => b.TaiLieu) // Kéo theo tài liệu để Giảng viên biết họ đang bình luận ở đâu
                .Where(b => b.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(b => b.NgayDang)
                .ToListAsync();
            // Bài Review bị AI nghi ngờ (ChoDuyet)
            var reviewChoDuyet = await _context.Reviews
                .Include(r => r.NguoiDung)
                .Include(r => r.HocPhan)
                .Where(r => r.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(r => r.NgayDang)
                .ToListAsync();

            ViewBag.TaiLieuChoDuyet = taiLieuChoDuyet;
            ViewBag.DanhSachBaoCao = danhSachBaoCao;
            ViewBag.BinhLuanChoDuyet = binhLuanChoDuyet;
            ViewBag.ReviewChoDuyet = reviewChoDuyet;
            return View();
        }

        // CÁC HÀM XỬ LÝ TÀI LIỆU CHỜ DUYỆT CŨ 
        [HttpPost]
        public async Task<IActionResult> DuyetTaiLieu(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu != null) { taiLieu.TrangThaiDuyet = "HopLe"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TuChoiTaiLieu(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu != null) { taiLieu.TrangThaiDuyet = "TuChoi"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // CÁC HÀM MỚI: XỬ LÝ BÁO CÁO VI PHẠM 

        [HttpPost]
        public async Task<IActionResult> XoaTaiLieuViPham(int id)
        {
            // Tìm báo cáo và kéo theo thông tin tài liệu của báo cáo đó
            var baoCao = await _context.BaoCaos.Include(b => b.TaiLieu).FirstOrDefaultAsync(b => b.BaoCaoID == id);

            if (baoCao != null && baoCao.TaiLieu != null)
            {
                // Đổi trạng thái tài liệu thành Từ chối (ẩn khỏi cộng đồng)
                baoCao.TaiLieu.TrangThaiDuyet = "TuChoi";

                // Đánh dấu báo cáo đã được giải quyết
                baoCao.TrangThaiXuLy = "DaXuLy";

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BoQuaBaoCao(int id)
        {
            var baoCao = await _context.BaoCaos.FindAsync(id);
            if (baoCao != null)
            {
                // Báo cáo sai, chỉ đánh dấu báo cáo là đã xử lý, giữ nguyên tài liệu
                baoCao.TrangThaiXuLy = "DaXuLy";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // XỬ LÝ BÌNH LUẬN
        [HttpPost]
        public async Task<IActionResult> DuyetBinhLuan(int id)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan != null)
            {
                binhLuan.TrangThaiDuyet = "HopLe"; // Cho phép hiển thị
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TuChoiBinhLuan(int id)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan != null)
            {
                binhLuan.TrangThaiDuyet = "TuChoi"; // Chặn vĩnh viễn
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // XỬ LÝ REVIEW 
        [HttpPost]
        public async Task<IActionResult> DuyetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.TrangThaiDuyet = "HopLe"; // Cho hiển thị ra cộng đồng
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TuChoiReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.TrangThaiDuyet = "TuChoi"; // Chặn vĩnh viễn
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}