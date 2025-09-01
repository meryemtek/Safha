using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using DataAccessLayer.EntityFramework.Context;
using UI.Services;
using Core;
using Entities;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Cache servislerini ekle
builder.Services.AddSingleton<GoogleBooksCacheService>();
builder.Services.AddScoped<GoogleBooksService>();

// Add DbContext
builder.Services.AddDbContext<SafhaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SafhaDb")));

// Generic repository ve servis kayıtları
builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));
builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudServiceBase<>));

// UserBookStatus servisini özel olarak kayıt et
builder.Services.AddScoped<ICrudService<UserBookStatus>, CrudServiceBase<UserBookStatus>>();

// BookStatusService'i kayıt et
builder.Services.AddScoped<IBookStatusService, BookStatusService>();

// FollowService'i kayıt et
builder.Services.AddScoped<FollowService>();

// DbContext'i generic olarak da kayıt et
builder.Services.AddScoped<DbContext>(provider => provider.GetService<SafhaDbContext>());

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "SAFHA.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SafhaDbContext>();
    await SeedTestData(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "library",
    pattern: "Library/{action=Index}/{id?}",
    defaults: new { controller = "Library" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Seed test data method
async Task SeedTestData(SafhaDbContext context)
{
    if (!context.Users.Any())
    {
        var hashedPassword = HashPassword("test123");
        var user = new User
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = hashedPassword,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var book = new Book
        {
            Title = "Test Kitap",
            Author = "Test Yazar",
            Description = "Bu bir test kitabıdır",
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Books.Add(book);
        await context.SaveChangesAsync();
    }
}

string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}