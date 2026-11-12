using System.ComponentModel.DataAnnotations;

namespace KiriNoTani.Web.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите отзыв")]
        [Display(Name = "Текст отзыва")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5)]
        [Display(Name = "Оценка")]
        public int Rating { get; set; } = 5;

        // Одобрен ли отзыв админом (для показа в общем списке отзывов)
        public bool IsApproved { get; set; } = false;

        // Позывать ли в блоке «Избранные» на Главной странице
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}