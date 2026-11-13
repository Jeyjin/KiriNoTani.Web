using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models;

namespace KiriNoTani.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Пользователь с таким Email уже существует");
                    return View(model);
                }

                // Первого зарегистрированного сделаем админом для удобства проверки
                string userRole = !_context.Users.Any() ? "Admin" : "User";

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password, 
                    Role = userRole
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await AuthenticateUser(user);
                return RedirectToAction("Profile");
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    await AuthenticateUser(user);
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", "Неверный Email или пароль");
            }
            return View(model);
        }

// GET: /Account/Profile (Личный кабинет)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Получаем ID текущего авторизованного пользователя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Загружаем бронирования строго по привязанному UserId (с подгрузкой услуг)
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => b.UserId == currentUserId)
                .ToListAsync();

            ViewBag.UserBookings = bookings;

            return View(user);
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task AuthenticateUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }
    }
}