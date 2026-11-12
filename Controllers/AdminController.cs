using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models;

namespace KiriNoTani.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ДАШБОРД И БРОНИРОВАНИЯ (Доступно: Admin, Manager)
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            ViewBag.TotalServices = await _context.Services.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.CountAsync();

            return View(bookings);
        }

        // Изменение статуса брони (Доступно: Admin, Manager)
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Удаление брони (Только Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 2. КАЛЕНДАРЬ БРОНИРОВАНИЙ (Доступно: Admin, Manager)
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthBookings = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => b.CheckInDate >= startDate && b.CheckInDate <= endDate)
                .ToListAsync();

            ViewBag.CurrentDate = startDate;
            return View(monthBookings);
        }

        // 3. УПРАВЛЕНИЕ УСЛУГАМИ (Только Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SaveService(Service service)
        {
            if (ModelState.IsValid)
            {
                if (service.Id == 0)
                {
                    _context.Services.Add(service);
                }
                else
                {
                    _context.Services.Update(service);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Services));
            }
            return View("Services", await _context.Services.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Services));
        }

        // 4. ПРОСМОТР И МОДЕРАЦИЯ ОТЗЫВОВ
        // Просмотр отзывов (Доступно: Admin, Manager)
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // Вывод отзывов на Главную (Доступно: Admin, Manager)
        [HttpPost]
        public async Task<IActionResult> ToggleFeaturedReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsFeatured = !review.IsFeatured;
                if (review.IsFeatured)
                {
                    review.IsApproved = true; // При выводе на главную авто-одобряем
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reviews));
        }

        // Одобрение отзывов (Только Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reviews));
        }

        // Удаление отзывов (Только Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reviews));
        }

        // 5. УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ И РОЛЯМИ (Только Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Role = role;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }
    }
}