using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;

namespace KiriNoTani.Web.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }
        
        // GET: /Services/SeedData
        public async Task<IActionResult> SeedData()
        {
            if (!_context.Services.Any())
            {
                _context.Services.AddRange(
                    new Models.Service { Title = "Японская баня (Фуросэ)", Description = "Традиционное прогревание в кедровых купелях под открытым небом с целевыми травами.", Price = 3500 },
                    new Models.Service { Title = "Аренда домика у воды", Description = "Уютные домики в минималистичном стиле с панорамными окнами и видом на туманную долину.", Price = 7000 },
                    new Models.Service { Title = "Чайная церемония", Description = "Медитативный процесс дегустации элитных сортов японского чая в уединенном павильоне.", Price = 2000 }
                );
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        // GET: /Services/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
    }
    
}