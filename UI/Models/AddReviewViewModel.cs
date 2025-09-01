using System.ComponentModel.DataAnnotations;
using Entities;

namespace UI.Models
{
    public class AddReviewViewModel
    {
        [Required(ErrorMessage = "İnceleme metni zorunludur")]
        [Display(Name = "İnceleme")]
        [StringLength(2000, ErrorMessage = "İnceleme en fazla 2000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Başlık")]
        [StringLength(500, ErrorMessage = "Başlık en fazla 500 karakter olabilir")]
        public string? Title { get; set; }
        
        [Required(ErrorMessage = "Puan zorunludur")]
        [Display(Name = "Puan")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; } = 5;
        
        [Required(ErrorMessage = "Kitap seçimi zorunludur")]
        [Display(Name = "Kitap")]
        public int BookId { get; set; }
        
        public List<Book> AvailableBooks { get; set; } = new List<Book>();
    }
}
