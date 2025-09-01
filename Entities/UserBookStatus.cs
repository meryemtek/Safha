using System;
using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class UserBookStatus : IEntity, ITrackable, IUserOwned
    {
        public int Id { get; set; }
        
        [Required]
        public int BookId { get; set; }
        
        [Required]
        public int? UserId { get; set; }
        
        [Required]
        public ReadingStatus Status { get; set; }
        
        public DateTime? StartedReadingDate { get; set; }
        
        public DateTime? FinishedReadingDate { get; set; }
        
        public int? CurrentPage { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Book? Book { get; set; }
        public User? User { get; set; }
    }
    
    public enum ReadingStatus
    {
        WantToRead = 1,
        CurrentlyReading = 2,
        Read = 3
    }
}
