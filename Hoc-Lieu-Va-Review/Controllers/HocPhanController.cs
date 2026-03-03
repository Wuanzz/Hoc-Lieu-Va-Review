using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hoc_Lieu_Va_Review.Controllers
{
    public class HocPhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HocPhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Học Phần
        public async Task<IActionResult> Index(string timKiem, int page = 1)
        {
            int pageSize = 8; // Quản lý Học phần dữ liệu thường nhiều hơn

            // Lấy danh sách Học phần (kèm theo thông tin Ngành nếu có)
            var query = _context.HocPhans.Include(h => h.Nganh).AsQueryable();

            // LỌC TÌM KIẾM THEO TÊN HỌC PHẦN
            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(h => h.TenHocPhan.Contains(timKiem));
                ViewBag.TuKhoa = timKiem; // Giữ lại từ khóa trên ô tìm kiếm
            }

            // THUẬT TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Cắt dữ liệu theo trang
            var danhSachHocPhan = await query
                .OrderBy(h => h.HocPhanID) // Sắp xếp theo ID
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(danhSachHocPhan);

        }
        // [GET] Hiển thị form Thêm mới Học Phần
        [HttpGet]
        public IActionResult Create()
        {
            // Tạo Dropdown List chọn Khoa
            ViewData["KhoaList"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa");
            return View();
        }

        // Hàm này trả về dữ liệu JSON cho AJAX gọi ngầm
        [HttpGet]
        public async Task<JsonResult> GetNganhByKhoa(int khoaId)
        {
            var nganhs = await _context.Nganhs
            .Where(n => n.KhoaID == khoaId)
            .Select(n => new { value = n.NganhID, text = n.TenNganh })
            .ToListAsync();
            return Json(nganhs);
        }

        // [POST] Lưu Học Phần mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenHocPhan,MoTa,NganhID")] HocPhan hocPhan)
        {
            // Bỏ qua lỗi validate object Nganh
            ModelState.Remove("Nganh");

            if (ModelState.IsValid)
            {
                _context.Add(hocPhan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại dropdown
            ViewData["NganhID"] = new SelectList(_context.Nganhs, "NganhID", "TenNganh", hocPhan.NganhID);
            return View(hocPhan);
        }
        // CHỨC NĂNG SỬA (EDIT)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Phải Include Nganh để lấy được KhoaID
            var hocPhan = await _context.HocPhans.Include(h => h.Nganh).FirstOrDefaultAsync(h => h.HocPhanID == id);
            if (hocPhan == null) return NotFound();

            // Lấy ID của Khoa đang chứa Ngành của môn học này
            int currentKhoaId = hocPhan.Nganh.KhoaID;

            // Truyền danh sách Khoa (chọn sẵn Khoa hiện tại)
            ViewData["KhoaList"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", currentKhoaId);
            // Truyền danh sách Ngành thuộc Khoa đó (chọn sẵn Ngành hiện tại)
            ViewData["NganhList"] = new SelectList(_context.Nganhs.Where(n => n.KhoaID == currentKhoaId), "NganhID", "TenNganh", hocPhan.NganhID);

            return View(hocPhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HocPhanID,TenHocPhan,MoTa,NganhID")] HocPhan hocPhan)
        {
            if (id != hocPhan.HocPhanID) return NotFound();

            ModelState.Remove("Nganh");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocPhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HocPhanExists(hocPhan.HocPhanID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, xử lý lại Dropdown (cơ bản)
            var nganh = await _context.Nganhs.FindAsync(hocPhan.NganhID);
            int currentKhoaId = nganh?.KhoaID ?? 0;
            ViewData["KhoaList"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", currentKhoaId);
            ViewData["NganhList"] = new SelectList(_context.Nganhs.Where(n => n.KhoaID == currentKhoaId), "NganhID", "TenNganh", hocPhan.NganhID);
            return View(hocPhan);
        }

        private bool HocPhanExists(int id)
        {
            return _context.HocPhans.Any(e => e.HocPhanID == id);
        }
    }
}
