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

        // СОЗДАНИЕ НОВОЙ УСЛУГИ
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service service, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                service.ImageUrl = await SaveImageAsync(imageFile);
            }

            ModelState.Remove(nameof(service.ImageUrl));

            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Services));
            }
            return View("Services", await _context.Services.ToListAsync());
        }

        // ОТКРЫТИЕ СТРАНИЦЫ РЕДАКТИРОВАНИЯ
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View(service);
        }

        // СОХРАНЕНИЕ ИЗМЕНЕНИЙ
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service service, IFormFile? imageFile)
        {
            if (id != service.Id) return NotFound();

            var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (existingService == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                service.ImageUrl = await SaveImageAsync(imageFile);
            }
            else
            {
                service.ImageUrl = existingService.ImageUrl; // Сохраняем старую картинку
            }

            ModelState.Remove(nameof(service.ImageUrl));

            if (ModelState.IsValid)
            {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Services));
            }
            return View(service);
        }

        // УДАЛЕНИЕ УСЛУГИ (Оставлен только один экземпляр метода)
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

        // Вспомогательный метод для сохранения картинки на сервер
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/services");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/services/" + uniqueFileName;
        }

        // 4. ПРОСМОТР И МОДЕРАЦИЯ ОТЗЫВОВ
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeaturedReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsFeatured = !review.IsFeatured;
                if (review.IsFeatured)
                {
                    review.IsApproved = true;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reviews));
        }

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