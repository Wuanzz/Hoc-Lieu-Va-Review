using Hoc_Lieu_Va_Review.Models;
using Hoc_Lieu_Va_Review.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hoc_Lieu_Va_Review.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu người dùng đã đăng nhập rồi thì phân luồng về đúng vị trí
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Khoa", new { area = "Admin" });
                }
                else if (User.IsInRole("GiangVien"))
                {
                    return RedirectToAction("Index", "KiemDuyet", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng trong database
                var user = _context.NguoiDungs.FirstOrDefault(u =>
                    u.Email == model.Email &&
                    u.MatKhau == model.MatKhau &&
                    u.TrangThai == "HoatDong"); // Đảm bảo tài khoản chưa bị khóa

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        // Tạo danh sách các "chứng minh thư" (Claims) cho người dùng này
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.VaiTro), // Phân quyền Admin/GiangVien/SinhVien
                        new Claim("UserId", user.NguoiDungID.ToString())
                    };

                    // Khởi tạo Identity và Principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Ghi Cookie đăng nhập vào trình duyệt
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // --- PHÂN LUỒNG ĐIỀU HƯỚNG THEO VAI TRÒ ---
                    if (user.VaiTro == "Admin")
                    {
                        // Admin vào thẳng trang Quản lý Khoa
                        return RedirectToAction("Index", "Khoa", new { area = "Admin" });
                    }
                    else if (user.VaiTro == "GiangVien")
                    {
                        // Giảng viên vào thẳng khu vực Kiểm duyệt của họ
                        return RedirectToAction("Index", "KiemDuyet", new { area = "Admin" });
                    }

                    // Nếu là Sinh viên thì về trang chủ public bình thường
                    return RedirectToAction("Index", "Home");
                }

                // Nếu không tìm thấy hoặc sai mật khẩu
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa.");
            }

            return View(model);
        }

        // Action để Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}