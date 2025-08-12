using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly SafhaDbContext _context;

        public HomeController(SafhaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Kullanıcı bilgilerini alıyoruz
            string username = "Ziyaretçi";
            string fullName = "";
            
            if (User.Identity.IsAuthenticated)
            {
                username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Kullanıcı";
                fullName = User.FindFirst("FullName")?.Value ?? "";
            }
            
            // Test verisi ekleyelim
            if (!_context.Users.Any())
            {
                var user = new User
                {
                    Username = "testuser",
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    PasswordHash = "test123"
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                var book = new Book
                {
                    Title = "Test Kitap",
                    Author = "Test Yazar",
                    Description = "Bu bir test kitabıdır",
                    UserId = user.Id
                };
                _context.Books.Add(book);
                _context.SaveChanges();
            }

            var books = _context.Books.Include(b => b.User).ToList();
            
            // ViewBag ile kullanıcı bilgilerini görünüme aktarıyoruz
            ViewBag.Username = username;
            ViewBag.FullName = fullName;
            
            return View(books);
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
    }
}