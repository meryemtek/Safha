using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using System.Security.Claims;

namespace UI.Services
{
    public interface IBookStatusService
    {
        Task<BookStatusSummary> GetUserBookStatusSummaryAsync(int userId);
        Task<List<Book>> GetUserBooksAsync(int userId);
        Task<List<UserBookStatus>> GetUserBookStatusesAsync(int userId);
        Task<Dictionary<ReadingStatus, int>> GetStatusCountsAsync(int userId);
    }

    public class BookStatusService : IBookStatusService
    {
        private readonly SafhaDbContext _context;

        public BookStatusService(SafhaDbContext context)
        {
            _context = context;
        }

        public async Task<BookStatusSummary> GetUserBookStatusSummaryAsync(int userId)
        {
            try
            {
                // Önce tüm aktif kitap durumlarını getir
                var bookStatuses = await _context.UserBookStatuses
                    .Where(s => s.UserId == userId && s.IsActive)
                    .ToListAsync();

                // Aktif kitap durumları olan kitapların ID'lerini al
                var activeBookIds = bookStatuses.Select(s => s.BookId).Distinct().ToList();
                
                // Aktif kitapları getir
                var books = await _context.Books
                    .Where(b => b.UserId == userId && b.IsActive && activeBookIds.Contains(b.Id))
                    .ToListAsync();

                // Durum sayılarını hesapla
                var currentlyReadingCount = bookStatuses.Count(s => s.Status == ReadingStatus.CurrentlyReading);
                var wantToReadCount = bookStatuses.Count(s => s.Status == ReadingStatus.WantToRead);
                var readCount = bookStatuses.Count(s => s.Status == ReadingStatus.Read);

                var summary = new BookStatusSummary
                {
                    TotalBooks = books.Count,
                    CurrentlyReadingCount = currentlyReadingCount,
                    WantToReadCount = wantToReadCount,
                    ReadCount = readCount,
                    Books = books,
                    BookStatuses = bookStatuses
                };

                return summary;
            }
            catch (Exception ex)
            {
                // Hata durumunda boş summary döndür
                Console.WriteLine($"GetUserBookStatusSummaryAsync hatası: {ex.Message}");
                return new BookStatusSummary
                {
                    TotalBooks = 0,
                    CurrentlyReadingCount = 0,
                    WantToReadCount = 0,
                    ReadCount = 0,
                    Books = new List<Book>(),
                    BookStatuses = new List<UserBookStatus>()
                };
            }
        }

        public async Task<List<Book>> GetUserBooksAsync(int userId)
        {
            return await _context.Books
                .Where(b => b.UserId == userId && b.IsActive)
                .ToListAsync();
        }

        public async Task<List<UserBookStatus>> GetUserBookStatusesAsync(int userId)
        {
            return await _context.UserBookStatuses
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();
        }

        public async Task<Dictionary<ReadingStatus, int>> GetStatusCountsAsync(int userId)
        {
            var bookStatuses = await _context.UserBookStatuses
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            return new Dictionary<ReadingStatus, int>
            {
                { ReadingStatus.CurrentlyReading, bookStatuses.Count(s => s.Status == ReadingStatus.CurrentlyReading) },
                { ReadingStatus.WantToRead, bookStatuses.Count(s => s.Status == ReadingStatus.WantToRead) },
                { ReadingStatus.Read, bookStatuses.Count(s => s.Status == ReadingStatus.Read) }
            };
        }
    }

    public class BookStatusSummary
    {
        public int TotalBooks { get; set; }
        public int CurrentlyReadingCount { get; set; }
        public int WantToReadCount { get; set; }
        public int ReadCount { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
        public List<UserBookStatus> BookStatuses { get; set; } = new List<UserBookStatus>();
    }
}


