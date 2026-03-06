using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hoc_Lieu_Va_Review.Models;

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
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
            // Lấy danh sách tài liệu mới tải lên chờ duyệt
            var taiLieuChoDuyet = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Where(t => t.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(t => t.NgayUpload)
                .ToListAsync();

            // Lấy danh sách các báo cáo đang chờ xử lý
            var danhSachBaoCao = await _context.BaoCaos
                .Include(b => b.TaiLieu)       // Kéo theo thông tin tài liệu bị báo cáo
                .Include(b => b.NguoiDung)     // Kéo theo thông tin sinh viên gửi báo cáo
                .Where(b => b.TrangThaiXuLy == "ChoXuLy")
                .OrderBy(b => b.NgayBaoCao)
                .ToListAsync();

            ViewBag.TaiLieuChoDuyet = taiLieuChoDuyet;
            ViewBag.DanhSachBaoCao = danhSachBaoCao; // Gửi sang View

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
    }
}