using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [Display(Name = "Kullanıcı Adı veya E-posta")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;
        
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
