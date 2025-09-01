using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class Review : IEntity, ITrackable
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [MaxLength(500)]
        public string? Title { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Foreign keys
        public int UserId { get; set; }
        public int BookId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}