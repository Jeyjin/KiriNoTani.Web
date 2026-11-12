using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models;

namespace KiriNoTani.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Booking (Клиентская страница оформления брони)
        [HttpGet]
        public async Task<IActionResult> Index(int? serviceId)
        {
            ViewBag.Services = await _context.Services.ToListAsync();
            
            var booking = new Booking
            {
                CheckInDate = DateTime.Today.AddDays(1),
                GuestsCount = 2
            };

            if (serviceId.HasValue)
            {
                booking.ServiceId = serviceId.Value;
            }

            return View(booking);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking, string[] selectedAddons)
        {
            if (selectedAddons != null && selectedAddons.Length > 0)
            {
                booking.AdditionalServices = string.Join(", ", selectedAddons);
            }
            else
            {
                booking.AdditionalServices = "Без доп. услуг";
            }

            if (ModelState.IsValid)
            {
                booking.Status = "Новое";
                booking.CreatedAt = DateTime.Now;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                // Переходим на страницу успешного подтверждения с ID брони
                return RedirectToAction(nameof(Success), new { id = booking.Id });
            }

            ViewBag.Services = await _context.Services.ToListAsync();
            return View("Index", booking);
        }

        // GET: /Booking/Success/5 (Подтверждение с деталями заказа)
        public async Task<IActionResult> Success(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(booking);
        }
    }
}