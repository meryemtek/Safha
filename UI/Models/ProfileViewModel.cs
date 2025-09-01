using Entities;

namespace UI.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPhoto { get; set; }
        public string? Bio { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int TargetBookCount { get; set; }
        public int ReadBookCount { get; set; }
        public int CurrentlyReadingCount { get; set; }
        public int WantToReadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
        public List<UserBookStatus> BookStatuses { get; set; } = new List<UserBookStatus>();
        public bool IsFollowing { get; set; } = false;
        public bool IsOwnProfile { get; set; } = false;
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayName => !string.IsNullOrEmpty(Username) ? Username : FullName;
        public int ReadingProgress => TargetBookCount > 0 ? (ReadBookCount * 100) / TargetBookCount : 0;
        public string MemberSince => CreatedAt.ToString("MMMM yyyy");
        
        // Kitap durumlarına göre kitapları filtreleme (BookLibrary ile aynı yöntem)
        public IEnumerable<Book> GetCurrentlyReadingBooks()
        {
            try
            {
                var currentlyReadingBookIds = BookStatuses
                    .Where(s => s.Status == ReadingStatus.CurrentlyReading && s.IsActive)
                    .Select(s => s.BookId);
                    
                return Books.Where(b => b.IsActive && currentlyReadingBookIds.Contains(b.Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentlyReadingBooks hatası: {ex.Message}");
                return new List<Book>();
            }
        }
        
        public IEnumerable<Book> GetWantToReadBooks()
        {
            try
            {
                var wantToReadBookIds = BookStatuses
                    .Where(s => s.Status == ReadingStatus.WantToRead && s.IsActive)
                    .Select(s => s.BookId);
                    
                return Books.Where(b => b.IsActive && wantToReadBookIds.Contains(b.Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetWantToReadBooks hatası: {ex.Message}");
                return new List<Book>();
            }
        }
        
        public IEnumerable<Book> GetReadBooks()
        {
            try
            {
                var readBookIds = BookStatuses
                    .Where(s => s.Status == ReadingStatus.Read && s.IsActive)
                    .Select(s => s.BookId);
                    
                return Books.Where(b => b.IsActive && readBookIds.Contains(b.Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetReadBooks hatası: {ex.Message}");
                return new List<Book>();
            }
        }
        
        // Kitabın durumunu almak için yardımcı metod
        public ReadingStatus? GetBookStatus(int bookId)
        {
            return BookStatuses
                .FirstOrDefault(s => s.BookId == bookId && s.IsActive)
                ?.Status;
        }
    }
}
