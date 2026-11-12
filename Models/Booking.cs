using System.ComponentModel.DataAnnotations;

namespace KiriNoTani.Web.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите услугу")]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Введите ваше имя")]
        [Display(Name = "Имя гостя")]
        public string GuestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату заезда")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата заезда")]
        public DateTime CheckInDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Выберите время заезда")]
        [Display(Name = "Время заезда")]
        public string TimeSlot { get; set; } = "14:00";

        [Range(1, 10, ErrorMessage = "Количество гостей от 1 до 10")]
        [Display(Name = "Количество гостей")]
        public int GuestsCount { get; set; } = 2;

        [Display(Name = "Дополнительные услуги")]
        public string AdditionalServices { get; set; } = string.Empty;

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Новое";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}