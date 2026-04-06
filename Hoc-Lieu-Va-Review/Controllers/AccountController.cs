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
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Khoa", new { area = "Admin" });
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
                var user = _context.NguoiDungs.FirstOrDefault(u =>
                    u.Email == model.Email &&
                    u.MatKhau == model.MatKhau &&
                    u.TrangThai == "HoatDong"); 

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.VaiTro),
                        new Claim("UserId", user.NguoiDungID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // Nếu là Admin hoặc Giảng viên (tùy theo tên VaiTro trong DB của cậu)
                    if (user.VaiTro == "Admin" || user.VaiTro == "GiangVien")
                    {
                        // Chuyển hướng vào trang quản lý Khoa bên trong Area Admin
                        return RedirectToAction("Index", "Khoa", new { area = "Admin" });
                    }

                    // Nếu là Sinh viên thì về trang chủ bình thường
                    return RedirectToAction("Index", "Home");
                }

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