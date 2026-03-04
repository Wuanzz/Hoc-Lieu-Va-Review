using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hoc_Lieu_Va_Review.Models;

namespace Hoc_Lieu_Va_Review.Controllers
{
    // Yêu cầu phải đăng nhập mới được vào trang này
    [Authorize]
    public class KhoaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhoaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách Khoa
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách Khoa từ Database
            var danhSachKhoa = await _context.Khoas.ToListAsync();
            return View(danhSachKhoa);
        }

        // Action [GET] để hiển thị Form nhập liệu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action [POST] để nhận dữ liệu từ Form và lưu vào Database
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo request
        public async Task<IActionResult> Create([Bind("TenKhoa,MoTa")] Khoa khoa)
        {
            if (ModelState.IsValid)
            {
                // Thêm dữ liệu vào bộ nhớ đệm
                _context.Add(khoa);
                // Lưu chính thức xuống SQL Server
                await _context.SaveChangesAsync();

                // Lưu xong thì quay về trang danh sách (Index)
                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu không hợp lệ (ví dụ bỏ trống tên Khoa), thì hiển thị lại Form kèm báo lỗi
            return View(khoa);
        }
        // Action [GET] hiển thị Form Sửa với dữ liệu cũ
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Tìm khoa trong DB dựa vào ID
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null) return NotFound();

            return View(khoa);
        }

        // Action [POST] nhận dữ liệu đã sửa và lưu lại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KhoaID,TenKhoa,MoTa")] Khoa khoa)
        {
            if (id != khoa.KhoaID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khoa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaExists(khoa.KhoaID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khoa);
        }

        // Hàm hỗ trợ kiểm tra xem Khoa có tồn tại không
        private bool KhoaExists(int id)
        {
            return _context.Khoas.Any(e => e.KhoaID == id);
        }

        // Action [GET] hiển thị trang xác nhận Xóa
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var khoa = await _context.Khoas.FirstOrDefaultAsync(m => m.KhoaID == id);
            if (khoa == null) return NotFound();

            return View(khoa);
        }

        // Action [POST] thực hiện việc xóa khỏi Database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa != null)
            {
                _context.Khoas.Remove(khoa);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}