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
            // Nếu người dùng đã đăng nhập rồi thì đá về trang chủ, không cho vào trang Login nữa
            if (User.Identity.IsAuthenticated)
            {
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
                    // Tạo danh sách các "chứng minh thư" (Claims) cho người dùng này
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.VaiTro), // Quan trọng để phân quyền Admin/SinhVien sau này
                        new Claim("UserId", user.NguoiDungID.ToString())
                    };

                    // Khởi tạo Identity và Principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Ghi Cookie đăng nhập vào trình duyệt
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // Đăng nhập thành công thì chuyển hướng về trang chủ
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