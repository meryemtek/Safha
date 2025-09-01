using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

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