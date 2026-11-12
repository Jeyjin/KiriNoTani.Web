using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models;

namespace KiriNoTani.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Единственный и правильный метод Index
        public async Task<IActionResult> Index()
        {
            ViewBag.FeaturedServices = await _context.Services.Take(3).ToListAsync();
            ViewBag.FeaturedReviews = await _context.Reviews.Where(r => r.IsFeatured).Take(3).ToListAsync();
            return View();
        }

        // Страница «О базе»
        public IActionResult About()
        {
            return View();
        }

        // Страница «Контакты»
        public IActionResult Contacts()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}