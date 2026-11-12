using System.ComponentModel.DataAnnotations;

namespace KiriNoTani.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите ваше имя")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Придумайте пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}