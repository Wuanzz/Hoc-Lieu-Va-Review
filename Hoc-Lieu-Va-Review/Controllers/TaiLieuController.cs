using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Cần cái này để lấy ID người dùng đăng nhập
using Hoc_Lieu_Va_Review.Models;
using Hoc_Lieu_Va_Review.Services;

namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
    public class TaiLieuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dùng để truy cập thư mục wwwroot
        private readonly GeminiService _geminiService; // Dùng để gọi API Gemini

        public TaiLieuController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, GeminiService geminiService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _geminiService = geminiService;
        }

        // Hiển thị danh sách Tài Liệu (CÓ TÌM KIẾM VÀ BỘ LỌC)
        public async Task<IActionResult> Index(string timKiem, int? locHocPhan)
        {
            // Bắt đầu với câu truy vấn cơ bản (Chỉ lấy tài liệu Hợp Lệ)
            var query = _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Where(t => t.TrangThaiDuyet == "HopLe")
                .AsQueryable();

            // LỌC 1: Nếu người dùng có gõ chữ vào ô tìm kiếm
            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(t => t.TenTaiLieu.Contains(timKiem));
                ViewBag.TuKhoa = timKiem; // Giữ lại từ khóa trên ô nhập để user đỡ bỡ ngỡ
            }

            // LỌC 2: Nếu người dùng chọn một môn học cụ thể từ Dropdown
            if (locHocPhan.HasValue && locHocPhan.Value > 0)
            {
                query = query.Where(t => t.HocPhanID == locHocPhan.Value);
            }

            // Truyền danh sách Môn học sang View để vẽ cái Dropdown (thanh chọn)
            ViewBag.DanhSachHocPhan = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan", locHocPhan);

            // Sắp xếp mới nhất lên đầu và chốt lấy dữ liệu
            var danhSachTaiLieu = await query.OrderByDescending(t => t.NgayUpload).ToListAsync();

            return View(danhSachTaiLieu);
        }

        // [GET] Hiển thị form Upload File
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan");
            return View();
        }

        // [POST] Xử lý việc lưu File lên server và lưu thông tin vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenTaiLieu,LoaiTaiLieu,HocPhanID")] TaiLieu taiLieu, IFormFile fileUpload)
        {
            // Bỏ qua validate các trường sẽ được code tự động gán giá trị
            ModelState.Remove("DuongDanFile");
            ModelState.Remove("NguoiDung");
            ModelState.Remove("HocPhan");
            ModelState.Remove("TrangThaiDuyet");

            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng có thực sự chọn file chưa
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    // Tạo thư mục "uploads" trong wwwroot nếu chưa tồn tại
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Đổi tên file để tránh việc upload 2 file trùng tên bị ghi đè (dùng Guid)
                    string fileExtension = Path.GetExtension(fileUpload.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Copy file vật lý vào thư mục uploads
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(fileStream);
                    }

                    // Cập nhật nốt các thông tin còn thiếu vào Model trước khi lưu DB
                    taiLieu.DuongDanFile = "/uploads/" + uniqueFileName;
                    taiLieu.KichThuoc = Math.Round((double)fileUpload.Length / (1024 * 1024), 2); // Đổi byte sang MB

                    // Lấy ID của người đang đăng nhập (từ Claims lúc Login)
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim != null)
                    {
                        taiLieu.NguoiDungID = int.Parse(userIdClaim.Value);
                    }

                    taiLieu.NgayUpload = DateTime.Now;
                    taiLieu.TrangThaiDuyet = "ChoDuyet"; // Sinh viên up lên thì cứ cho vào trạng thái chờ Admin duyệt
                    taiLieu.LuotTai = 0;

                    // Lưu vào Database
                    _context.Add(taiLieu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("DuongDanFile", "Vui lòng chọn một file để upload.");
                }
            }

            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan", taiLieu.HocPhanID);
            return View(taiLieu);
        }

        // Hàm xử lý việc tải file và đếm lượt tải
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu == null) return NotFound("Không tìm thấy thông tin tài liệu trong Database.");

            // CÁCH MỚI: Tách lấy đúng cái tên file ở đuôi, rồi tự ghép nối lại cực kỳ an toàn
            string fileName = Path.GetFileName(taiLieu.DuongDanFile);
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Tớ thêm cái filePath vào thông báo lỗi để nếu có sai, cậu sẽ nhìn thấy ngay nó đang tìm ở đâu
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"File vật lý không tồn tại. Máy chủ đang tìm tại: {filePath}");
            }

            taiLieu.LuotTai += 1;
            _context.Update(taiLieu);
            await _context.SaveChangesAsync();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            string extension = Path.GetExtension(filePath);
            string downloadName = taiLieu.TenTaiLieu + extension;

            return File(fileBytes, "application/octet-stream", downloadName);
        }

        // [GET] Hiển thị chi tiết tài liệu và danh sách bình luận
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiLieu = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Include(t => t.BinhLuans)
                    .ThenInclude(b => b.NguoiDung)
                .FirstOrDefaultAsync(m => m.TaiLieuID == id);

            if (taiLieu == null) return NotFound();

            // Chỉ lấy những bình luận qua ải kiểm duyệt
            taiLieu.BinhLuans = taiLieu.BinhLuans
                .Where(b => b.TrangThaiDuyet == "HopLe" || b.TrangThaiDuyet == "DaDuyet")
                .OrderByDescending(b => b.NgayDang).ToList();

            return View(taiLieu);
        }

        // [POST] Xử lý khi người dùng bấm "Gửi bình luận"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int TaiLieuID, string NoiDung)
        {
            if (string.IsNullOrWhiteSpace(NoiDung)) return RedirectToAction(nameof(Details), new { id = TaiLieuID });

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                // ĐEM NỘI DUNG ĐI HỎI TRỢ LÝ AI
                string ketQuaDuyet = await _geminiService.KiemDuyetVanBan(NoiDung);

                var binhLuan = new BinhLuan
                {
                    TaiLieuID = TaiLieuID,
                    NoiDung = NoiDung,
                    NgayDang = DateTime.Now,
                    TrangThaiDuyet = ketQuaDuyet, // Gán kết quả của AI trực tiếp vào Database!
                    NguoiDungID = int.Parse(userIdClaim.Value)
                };

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();

                // Gửi thông báo cho Sinh viên biết AI đang làm việc
                if (ketQuaDuyet == "TuChoi")
                {
                    TempData["ThongBaoBaoCao"] = "❌ Bình luận của bạn chứa từ ngữ vi phạm và đã bị AI tự động chặn hiển thị!";
                }
                else if (ketQuaDuyet == "ChoDuyet")
                {
                    TempData["ThongBaoBaoCao"] = "⏳ Bình luận của bạn có từ ngữ lạ, đang được đưa vào danh sách chờ Giảng viên duyệt.";
                }
            }

            return RedirectToAction(nameof(Details), new { id = TaiLieuID });
        }
    }
}