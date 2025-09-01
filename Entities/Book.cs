using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class Book : IEntity, ITrackable, IUserOwned
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? ISBN { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public int? PublicationYear { get; set; }
        
        [MaxLength(100)]
        public string? Publisher { get; set; }
        
        public int? Pages { get; set; }
        
        [MaxLength(50)]
        public string? Genre { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        [MaxLength(500)]
        public string? CoverImage { get; set; }

        public bool IsAvailable { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public int? UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserBookStatus> UserBookStatuses { get; set; } = new List<UserBookStatus>();
    }
}