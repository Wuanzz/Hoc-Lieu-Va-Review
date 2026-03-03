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
    }
}