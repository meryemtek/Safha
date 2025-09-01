using System.ComponentModel.DataAnnotations;
using Entities;

namespace UI.Models
{
    public class AddQuoteViewModel
    {
        [Required(ErrorMessage = "Alıntı metni zorunludur")]
        [Display(Name = "Alıntı")]
        [StringLength(1000, ErrorMessage = "Alıntı en fazla 1000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Yazar")]
        [StringLength(100, ErrorMessage = "Yazar adı en fazla 100 karakter olabilir")]
        public string? Author { get; set; }
        
        [Display(Name = "Kaynak")]
        [StringLength(200, ErrorMessage = "Kaynak en fazla 200 karakter olabilir")]
        public string? Source { get; set; }
        
        [Display(Name = "Sayfa Numarası")]
        [Range(1, 9999, ErrorMessage = "Geçerli bir sayfa numarası giriniz")]
        public int PageNumber { get; set; }
        
        [Display(Name = "Notlar")]
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notes { get; set; }
        
        [Required(ErrorMessage = "Kitap seçimi zorunludur")]
        [Display(Name = "Kitap")]
        public int BookId { get; set; }
        
        public List<Book> AvailableBooks { get; set; } = new List<Book>();
    }
}
