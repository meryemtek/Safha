using System;

namespace UI.Models
{
    public class QuoteViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Source { get; set; }
        public int? PageNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string? BookCoverImage { get; set; }
    }
}



