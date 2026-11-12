using System.ComponentModel.DataAnnotations;

namespace KiriNoTani.Web.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название услуги")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Краткое описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Подробное описание")]
        public string FullDescription { get; set; } = string.Empty;

        [Display(Name = "Цена (₽)")]
        public decimal Price { get; set; }

        [Display(Name = "Изображение (URL)")]
        public string? ImageUrl { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}