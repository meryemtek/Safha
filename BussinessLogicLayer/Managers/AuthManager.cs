using BussinessLogicLayer.Interfaceses;
using DataAccessLayer.EntityFramework.Context;
using DataTransferObject.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogicLayer.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly SafhaDbContext _context;



        public AuthManager(SafhaDbContext context)
        {
            _context = context;
        }

        public bool RememberMe { get; private set; }

        public async Task<UserModel> Login(string username, string password, bool RememberMe)
        {
            var hashedPassword = HashPassword(password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == username || u.Username == username) && u.Password == hashedPassword && u.IsActive);

            ClaimsIdentity claimsIdentity = null;
            AuthenticationProperties authProperties = null;

            if (user != null)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role ?? "User")
                    };

                claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                authProperties = new AuthenticationProperties
                {
                    IsPersistent = RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };
            }

            UserModel model = new UserModel();
            model.authProperties = authProperties;
            model.ClaismIdentity = claimsIdentity;
            return model;
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

