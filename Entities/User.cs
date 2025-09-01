using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class User : IEntity, ITrackable
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        // Profil bilgileri
        [MaxLength(500)]
        public string? ProfilePicture { get; set; }
        
        [MaxLength(500)]
        public string? CoverPhoto { get; set; }
        
        [MaxLength(500)]
        public string? Bio { get; set; }
        
        public int FollowerCount { get; set; } = 0;
        
        public int FollowingCount { get; set; } = 0;
        
        public int TargetBookCount { get; set; } = 0;
        
        public int ReadBookCount { get; set; } = 0;
        
        public string Role { get; set; } = "User";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public ICollection<Follow> Following { get; set; } = new List<Follow>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserBookStatus> UserBookStatuses { get; set; } = new List<UserBookStatus>();
    }
}