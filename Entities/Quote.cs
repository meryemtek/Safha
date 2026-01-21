using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class Quote : IEntity, ITrackable
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Author { get; set; }
        
        [MaxLength(200)]
        public string? Source { get; set; }
        
        public int PageNumber { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
       
        public int UserId { get; set; }
        public int BookId { get; set; }
       
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
        public ICollection<QuoteLike> Likes { get; set; } = new List<QuoteLike>();
    }
}
