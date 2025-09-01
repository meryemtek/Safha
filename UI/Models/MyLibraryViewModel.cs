using Entities;
using System.Collections.Generic;

namespace UI.Models
{
    public class MyLibraryViewModel
    {
        public List<BookWithStatus> ReadBooks { get; set; } = new List<BookWithStatus>();
        public List<BookWithStatus> CurrentlyReadingBooks { get; set; } = new List<BookWithStatus>();
        public List<BookWithStatus> WantToReadBooks { get; set; } = new List<BookWithStatus>();
        public string SearchQuery { get; set; } = string.Empty;
    }

    public class BookWithStatus
    {
        public Book Book { get; set; }
        public UserBookStatus Status { get; set; }
        
        public BookWithStatus(Book book, UserBookStatus status)
        {
            Book = book;
            Status = status;
        }
    }
}
