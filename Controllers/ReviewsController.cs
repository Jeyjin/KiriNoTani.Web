using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models;
using System.Security.Claims;

namespace KiriNoTani.Web.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reviews — список одобренных отзывов
        public async Task<IActionResult> Index()
        {
            var approvedReviews = await _context.Reviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(approvedReviews);
        }

// POST: /Reviews/Create — только для авторизованных
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            // 1. Получаем email текущего залогиненного пользователя
            var userEmail = User.Identity?.Name;

            // 2. Ищем пользователя в базе по почте, чтобы забрать его реальное Name
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // 3. Записываем имя: если в профиле есть Name, берем его, иначе fallback на email/логин
            review.Author = dbUser != null && !string.IsNullOrEmpty(dbUser.Name) 
                ? dbUser.Name 
                : (userEmail ?? "Пользователь");

            // Убираем Author из проверки ModelState, так как мы заполнили его в коде
            ModelState.Remove(nameof(Review.Author));

            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                review.IsApproved = false; // Модерация активна
                review.IsFeatured = false;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Спасибо! Ваш отзыв отправлен на модерацию и появится на сайте после проверки администратором.";
                return RedirectToAction(nameof(Index));
            }

            var approvedReviews = await _context.Reviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View("Index", approvedReviews);
        }
    }
}