using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;
namespace Hoc_Lieu_Va_Review.Controllers
{
    public class NganhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NganhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Ngành
        public async Task<IActionResult> Index(string timKiem, int page = 1)
        {
            int pageSize = 5; // Cứ để 5 dòng 1 trang cho giao diện gọn gàng

            // Lấy danh sách Ngành kèm theo thông tin của Khoa
            var query = _context.Nganhs.Include(n => n.Khoa).AsQueryable();

            // LỌC TÌM KIẾM THEO TÊN NGÀNH
            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(n => n.TenNganh.Contains(timKiem));
                ViewBag.TuKhoa = timKiem; // Giữ lại từ khóa trên ô tìm kiếm
            }

            // THUẬT TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Cắt dữ liệu theo trang
            var danhSachNganh = await query
                .OrderBy(n => n.NganhID) // Sắp xếp theo ID
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(danhSachNganh);
        }
        // [GET] Hiển thị form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            // Tạo một danh sách thả xuống (SelectList) chứa các Khoa để chọn
            // "KhoaID" là giá trị ẩn (value), "TenKhoa" là chữ hiển thị ra ngoài (text)
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa");
            return View();
        }

        // [POST] Lưu Ngành mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenNganh,MoTa,KhoaID")] Nganh nganh)
        {
            // Yêu cầu hệ thống không bắt lỗi thiếu dữ liệu của object Khoa
            ModelState.Remove("Khoa");

            if (ModelState.IsValid)
            {
                _context.Add(nganh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Nếu lỗi, phải nạp lại danh sách Khoa cho dropdown kẻo bị lỗi giao diện
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", nganh.KhoaID);
            return View(nganh);
        }
    }
}
