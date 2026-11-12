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
            // Автоматически берем имя авторизованного пользователя
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Гость";
            review.Author = userName;

            // Убираем обязательные поля из проверки ModelState, если форма их не шлет с фронта
            ModelState.Remove(nameof(Review.Author));

            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                review.IsApproved = false; // ВЕРНУЛИ МОДЕРАЦИЮ: отзыв ждет одобрения админа
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