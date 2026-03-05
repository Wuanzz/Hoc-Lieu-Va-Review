using Microsoft.AspNetCore.Mvc;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
    public class BaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaoCaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiBaoCao(int TaiLieuID, string LyDo)
        {
            var userIdClaim = User.FindFirst("UserId");

            // Nếu có nhập lý do và đã đăng nhập
            if (userIdClaim != null && !string.IsNullOrWhiteSpace(LyDo))
            {
                var baoCao = new BaoCao
                {
                    TaiLieuID = TaiLieuID,
                    NguoiDungID = int.Parse(userIdClaim.Value),
                    LyDo = LyDo,
                    NgayBaoCao = DateTime.Now,
                    TrangThaiXuLy = "ChoXuLy" // Trạng thái chờ Giảng viên duyệt
                };

                _context.BaoCaos.Add(baoCao);
                await _context.SaveChangesAsync();

                // TempData giúp gửi một thông báo nhỏ sang trang sau khi redirect
                TempData["ThongBaoBaoCao"] = "Cảm ơn bạn! Báo cáo đã được gửi đến Giảng viên để xử lý.";
            }

            // Gửi xong thì load lại đúng trang chi tiết tài liệu đó
            return RedirectToAction("Details", "TaiLieu", new { id = TaiLieuID });
        }
    }
}