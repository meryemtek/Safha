using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using UI.Models;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly SafhaDbContext _context;

        public HomeController(SafhaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcı bilgilerini alıyoruz
            string username = "Ziyaretçi";
            string fullName = "";
            
            if (User.Identity.IsAuthenticated)
            {
                username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Kullanıcı";
                fullName = User.FindFirst("FullName")?.Value ?? "";
            }
            
            // Son eklenen kitapları al
            var recentBooks = await _context.Books
                .Include(b => b.User)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Take(6)
                .ToListAsync();
            
            // Son eklenen alıntıları al (kullanıcı bilgileri ile birlikte)
            var recentQuotes = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Book)
                .Where(q => q.IsActive && q.BookId > 0) // Sadece kitap alıntıları
                .OrderByDescending(q => q.CreatedAt)
                .Take(8)
                .Select(q => new QuoteViewModel
                {
                    Id = q.Id,
                    Content = q.Content,
                    Author = q.Author,
                    Source = q.Source,
                    PageNumber = q.PageNumber,
                    Notes = q.Notes,
                    CreatedAt = q.CreatedAt,
                    UserId = q.User.Id,
                    UserName = q.User.FirstName + " " + q.User.LastName,
                    UserProfilePicture = q.User.ProfilePicture,
                    BookTitle = q.Book.Title,
                    BookAuthor = q.Book.Author,
                    BookCoverImage = q.Book.CoverImage
                })
                .ToListAsync();
            
            // Son aktiviteleri al (kitaplık durumu değişiklikleri)
            var recentActivities = await _context.UserBookStatuses
                .Include(ubs => ubs.User)
                .Include(ubs => ubs.Book)
                .Where(ubs => ubs.IsActive)
                .OrderByDescending(ubs => ubs.UpdatedAt)
                .Take(5)
                .Select(ubs => new ActivityViewModel
                {
                    UserName = ubs.User.FirstName + " " + ubs.User.LastName,
                    BookTitle = ubs.Book.Title,
                    Status = ubs.Status,
                    UpdatedAt = ubs.UpdatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();
            
            // ViewBag ile kullanıcı bilgilerini görünüme aktarıyoruz
            ViewBag.Username = username;
            ViewBag.FullName = fullName;
            ViewBag.RecentBooks = recentBooks;
            ViewBag.RecentQuotes = recentQuotes;
            ViewBag.RecentActivities = recentActivities;
            
            return View(recentBooks);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}