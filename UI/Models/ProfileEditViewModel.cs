using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UI.Models
{
    public class ProfileEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Telefon Numarası")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir")]
        public string? Address { get; set; }
        
        [Display(Name = "Hakkımda")]
        [StringLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir")]
        public string? Bio { get; set; }
        
        [Display(Name = "Hedef Kitap Sayısı")]
        [Range(0, 1000, ErrorMessage = "Hedef kitap sayısı 0-1000 arasında olmalıdır")]
        public int TargetBookCount { get; set; }
        
        [Display(Name = "Profil Resmi")]
        public IFormFile? ProfilePictureFile { get; set; }
        
        [Display(Name = "Kapak Fotoğrafı")]
        public IFormFile? CoverPhotoFile { get; set; }
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
}
