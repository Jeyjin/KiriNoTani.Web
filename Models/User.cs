using System.ComponentModel.DataAnnotations;

namespace KiriNoTani.Web.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Роль")]
        public string Role { get; set; } = "User"; // "User" или "Admin"

        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}