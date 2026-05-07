using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Cần cái này để lấy ID người dùng đăng nhập
using Azure.Storage.Blobs;
using Hoc_Lieu_Va_Review.Models;
using Hoc_Lieu_Va_Review.Services;


namespace Hoc_Lieu_Va_Review.Controllers
{
    [Authorize]
    public class TaiLieuController : Controller
    {
        private readonly ApplicationDbContext _context;
        // private readonly IWebHostEnvironment _webHostEnvironment; // Dùng để truy cập thư mục wwwroot
        private readonly GeminiService _geminiService; // Dùng để gọi API Gemini
        private readonly IConfiguration _configuration;

        public TaiLieuController(ApplicationDbContext context /*IWebHostEnvironment webHostEnvironment*/, GeminiService geminiService, IConfiguration configuration)
        {
            _context = context;
            _geminiService = geminiService;
            _configuration = configuration;
        }

        // Hiển thị danh sách Tài Liệu (CÓ TÌM KIẾM VÀ BỘ LỌC)
        public async Task<IActionResult> Index(string timKiem, int? locHocPhan, int page = 1)
        {
            int pageSize = 8; // Đặt số lượng tài liệu hiển thị trên 1 trang (có thể đổi thành 10, 15 tùy ý)

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
            ViewBag.LocHocPhan = locHocPhan; // Giữ lại ID môn học để chuyển trang không bị mất lọc

            // THUẬT TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync(); // Đếm tổng số kết quả
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Tính tổng số trang làm tròn lên

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Sắp xếp mới nhất lên đầu và chốt lấy dữ liệu bằng Skip & Take
            var danhSachTaiLieu = await query
                .OrderByDescending(t => t.NgayUpload)
                .Skip((page - 1) * pageSize) // Bỏ qua các bài của trang trước
                .Take(pageSize)              // Lấy đúng số lượng của trang hiện tại
                .ToListAsync();

            return View(danhSachTaiLieu);
        }

        // [GET] Hiển thị form Upload File
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.DanhSachKhoa = new SelectList(_context.Khoas, "KhoaID", "TenKhoa");
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
                    // 1. Lấy chuỗi kết nối Storage từ cấu hình (Key Vault)
                    string storageConnString = _configuration["Storage--ConnectionString"] ?? _configuration["Storage:ConnectionString"];

                    // 2. Kết nối tới Blob Container
                    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("tailieu-uploads");

                    // 3. Tạo tên file độc nhất để chống ghi đè
                    string fileExtension = Path.GetExtension(fileUpload.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // 4. Upload stream dữ liệu thẳng lên Azure Blob
                    using (var stream = fileUpload.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    // 5. Cập nhật Model với đường dẫn tuyệt đối của Azure Blob
                    taiLieu.DuongDanFile = blobClient.Uri.ToString();
                    taiLieu.KichThuoc = Math.Round((double)fileUpload.Length / (1024 * 1024), 2); // Đổi byte sang MB

                    // Lấy ID của người đang đăng nhập
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim != null)
                    {
                        taiLieu.NguoiDungID = int.Parse(userIdClaim.Value);
                    }

                    taiLieu.NgayUpload = DateTime.Now;
                    taiLieu.TrangThaiDuyet = "ChoDuyet";
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

        // Dropdown liên ho giữa Khoa -> Ngành -> Học Phần
        [HttpGet]
        public IActionResult GetNganhByKhoa(int khoaId)
        {
            var nganhs = _context.Nganhs
                .Where(n => n.KhoaID == khoaId)
                .Select(n => new { value = n.NganhID, text = n.TenNganh })
                .ToList();
            return Json(nganhs);
        }

        [HttpGet]
        public IActionResult GetHocPhanByNganh(int nganhId)
        {
            var hocPhans = _context.HocPhans
                .Where(h => h.NganhID == nganhId)
                .Select(h => new { value = h.HocPhanID, text = h.TenHocPhan })
                .ToList();
            return Json(hocPhans);
        }

        // Hàm xử lý việc tải file và đếm lượt tải
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu == null) return NotFound("Không tìm thấy thông tin tài liệu trong Database.");

            // Tăng lượt tải lên 1
            taiLieu.LuotTai += 1;
            _context.Update(taiLieu);
            await _context.SaveChangesAsync();

            // Chuyển hướng người dùng thẳng tới link của Azure Blob Storage để tải file
            return Redirect(taiLieu.DuongDanFile);
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