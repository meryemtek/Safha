using System.Collections.Generic;
using Entities;
using System;
using System.Linq;

namespace UI.Models
{
    public class BookLibraryViewModel
    {
        public List<Book> Books { get; set; } = new List<Book>();
        public List<UserBookStatus> BookStatuses { get; set; } = new List<UserBookStatus>();
        public int? StatusFilter { get; set; }
        public bool IsCurrentUser { get; set; } = true;
        
        // BookStatusSummary'den alınan istatistik bilgileri
        public int TotalBooks { get; set; }
        public int CurrentlyReadingCount { get; set; }
        public int WantToReadCount { get; set; }
        public int ReadCount { get; set; }
        
        // Kitap durumuna göre filtreleme yardımcı metodları (Profile ile aynı yöntem)
        public List<Book> GetBooksWithStatus(ReadingStatus status)
        {
            try
            {
                var statusBookIds = BookStatuses
                    .Where(s => s.Status == status && s.IsActive)
                    .Select(s => s.BookId)
                    .ToList();
                    
                return Books.Where(b => b.IsActive && statusBookIds.Contains(b.Id)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetBooksWithStatus hatası: {ex.Message}");
                return new List<Book>();
            }
        }
        
        public List<Book> GetWantToReadBooks() => GetBooksWithStatus(ReadingStatus.WantToRead);
        public List<Book> GetCurrentlyReadingBooks() => GetBooksWithStatus(ReadingStatus.CurrentlyReading);
        public List<Book> GetReadBooks() => GetBooksWithStatus(ReadingStatus.Read);
        
        // Kitabın durumunu alma (Profile ile aynı yöntem)
        public ReadingStatus? GetBookStatus(int bookId)
        {
            try
            {
                var status = BookStatuses.FirstOrDefault(s => s.BookId == bookId && s.IsActive);
                return status?.Status;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetBookStatus hatası: {ex.Message}");
                return null;
            }
        }
        
        // Durumu olmayan kitapları alma (Profile ile aynı yöntem)
        public List<Book> GetBooksWithoutStatus()
        {
            try
            {
                var statusBookIds = BookStatuses.Where(s => s.IsActive).Select(s => s.BookId).ToList();
                return Books.Where(b => b.IsActive && !statusBookIds.Contains(b.Id)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetBooksWithoutStatus hatası: {ex.Message}");
                return new List<Book>();
            }
        }
    }
}

