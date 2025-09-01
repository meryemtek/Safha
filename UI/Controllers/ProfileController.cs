using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using UI.Models;
using UI.Services;
using UI.Models.GoogleBooks;
using Core;
using System.Security.Claims;

namespace UI.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly SafhaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly GoogleBooksService _googleBooksService;
        private readonly ICrudService<Book> _bookService;
        private readonly IBookStatusService _bookStatusService;
        private readonly FollowService _followService;

        public ProfileController(
            SafhaDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            GoogleBooksService googleBooksService,
            ICrudService<Book> bookService,
            IBookStatusService bookStatusService,
            FollowService followService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _googleBooksService = googleBooksService;
            _bookService = bookService;
            _bookStatusService = bookStatusService;
            _followService = followService;
        }

        // GET: /Profile/Index
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users
                .Include(u => u.Followers.Where(f => f.IsActive))
                .Include(u => u.Following.Where(f => f.IsActive))
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }
            
            // Kitap durumlarını ve kitapları ortak servisten getir
            var bookStatusSummary = await _bookStatusService.GetUserBookStatusSummaryAsync(userId);

            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfilePicture = user.ProfilePicture,
                CoverPhoto = user.CoverPhoto,
                Bio = user.Bio,
                FollowerCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                TargetBookCount = user.TargetBookCount,
                ReadBookCount = bookStatusSummary.ReadCount,
                CurrentlyReadingCount = bookStatusSummary.CurrentlyReadingCount,
                WantToReadCount = bookStatusSummary.WantToReadCount,
                CreatedAt = user.CreatedAt,
                Books = bookStatusSummary.Books,
                BookStatuses = bookStatusSummary.BookStatuses
            };

            return View(profileViewModel);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var editViewModel = new ProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Bio = user.Bio,
                TargetBookCount = user.TargetBookCount
            };

            return View(editViewModel);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Profil resmi yükleme
                if (model.ProfilePictureFile != null)
                {
                    var profilePicturePath = await SaveFile(model.ProfilePictureFile, "profile-pictures");
                    user.ProfilePicture = profilePicturePath;
                }

                // Kapak fotoğrafı yükleme
                if (model.CoverPhotoFile != null)
                {
                    var coverPhotoPath = await SaveFile(model.CoverPhotoFile, "cover-photos");
                    user.CoverPhoto = coverPhotoPath;
                }

                // Diğer bilgileri güncelle
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Bio = model.Bio;
                user.TargetBookCount = model.TargetBookCount;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profil başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Profile/View/{id}
        [AllowAnonymous]
        public async Task<IActionResult> View(int id)
        {
            var user = await _context.Users
                .Include(u => u.Followers.Where(f => f.IsActive))
                .Include(u => u.Following.Where(f => f.IsActive))
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isFollowing = false;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var currentUserIdInt = int.Parse(currentUserId);
                isFollowing = await _context.Follows
                    .AnyAsync(f => f.FollowerId == currentUserIdInt && f.FollowingId == id && f.IsActive);
            }
            
            // Kitap durumlarını ve kitapları ortak servisten getir
            var bookStatusSummary = await _bookStatusService.GetUserBookStatusSummaryAsync(id);

            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfilePicture = user.ProfilePicture,
                CoverPhoto = user.CoverPhoto,
                Bio = user.Bio,
                FollowerCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                TargetBookCount = user.TargetBookCount,
                ReadBookCount = bookStatusSummary.ReadCount,
                CurrentlyReadingCount = bookStatusSummary.CurrentlyReadingCount,
                WantToReadCount = bookStatusSummary.WantToReadCount,
                CreatedAt = user.CreatedAt,
                Books = bookStatusSummary.Books,
                BookStatuses = bookStatusSummary.BookStatuses,
                IsFollowing = isFollowing
            };

            return View("Index", profileViewModel);
        }



        // POST: /Profile/UploadPhoto
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile file, string photoType)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });
                }

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Geçerli bir dosya seçilmedi" });
                }

                string folderName = photoType == "profile" ? "profile-pictures" : "cover-photos";
                var photoPath = await SaveFile(file, folderName);

                if (string.IsNullOrEmpty(photoPath))
                {
                    return Json(new { success = false, message = "Fotoğraf yüklenemedi" });
                }

                if (photoType == "profile")
                {
                    user.ProfilePicture = photoPath;
                }
                else if (photoType == "cover")
                {
                    user.CoverPhoto = photoPath;
                }

                user.UpdatedAt = DateTime.UtcNow;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Fotoğraf başarıyla yüklendi", 
                    photoUrl = photoPath 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Fotoğraf yükleme hatası: " + ex.Message });
            }
        }

        // GET: /Profile/SearchBooks - Kitap arama sayfası
        public IActionResult SearchBooks()
        {
            return View();
        }

        // POST: /Profile/SearchBooks - API'den kitap arama
        [HttpPost]
        public async Task<IActionResult> SearchBooks(string query, int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Json(new { success = false, message = "Arama terimi gerekli" });
                }

                int maxResults = 10;
                int startIndex = (page - 1) * maxResults;

                var result = await _googleBooksService.SearchBooksAsync(query, maxResults, startIndex);
                
                return Json(new { 
                    success = true, 
                    books = result.Items,
                    totalItems = result.TotalItems,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)result.TotalItems / maxResults)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Arama sırasında hata oluştu: {ex.Message}" });
            }
        }

        // POST: /Profile/AddBookFromAPI - API'den kitap ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBookFromAPI(string googleBookId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }

                // Google Books API'den kitap detaylarını al
                var googleBook = await _googleBooksService.GetBookByIdAsync(googleBookId);
                if (googleBook == null)
                {
                    return Json(new { success = false, message = "Kitap bulunamadı" });
                }

                // Kitap zaten eklenmiş mi kontrol et
                var existingBook = await _context.Books
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                            b.Title == googleBook.VolumeInfo.Title && 
                                            b.Author == (googleBook.VolumeInfo.Authors != null && googleBook.VolumeInfo.Authors.Any() ? googleBook.VolumeInfo.Authors.First() : "Bilinmeyen Yazar") &&
                                            b.IsActive);

                if (existingBook != null)
                {
                    return Json(new { success = false, message = "Bu kitap zaten kütüphanenizde mevcut" });
                }

                // Yeni kitap oluştur
                var book = new Book
                {
                    Title = googleBook.VolumeInfo.Title?.Length > 200 ? googleBook.VolumeInfo.Title.Substring(0, 200) : googleBook.VolumeInfo.Title,
                    Author = (googleBook.VolumeInfo.Authors?.FirstOrDefault() ?? "Bilinmeyen Yazar").Length > 100 ? 
                             (googleBook.VolumeInfo.Authors?.FirstOrDefault() ?? "Bilinmeyen Yazar").Substring(0, 100) : 
                             (googleBook.VolumeInfo.Authors?.FirstOrDefault() ?? "Bilinmeyen Yazar"),
                    Description = googleBook.VolumeInfo.Description?.Length > 500 ? googleBook.VolumeInfo.Description.Substring(0, 500) : googleBook.VolumeInfo.Description,
                    ISBN = googleBook.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier?.Length > 50 ? 
                           googleBook.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier?.Substring(0, 50) : 
                           googleBook.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier,
                    PublicationYear = googleBook.VolumeInfo.GetPublishedYear(),
                    Publisher = googleBook.VolumeInfo.Publisher?.Length > 100 ? googleBook.VolumeInfo.Publisher.Substring(0, 100) : googleBook.VolumeInfo.Publisher,
                    Pages = googleBook.VolumeInfo.PageCount,
                    Genre = googleBook.VolumeInfo.Categories?.FirstOrDefault()?.Length > 50 ? 
                            googleBook.VolumeInfo.Categories?.FirstOrDefault()?.Substring(0, 50) : 
                            googleBook.VolumeInfo.Categories?.FirstOrDefault(),
                    Language = googleBook.VolumeInfo.Language?.Length > 20 ? googleBook.VolumeInfo.Language?.Substring(0, 20) : googleBook.VolumeInfo.Language?.ToUpper(),
                    CoverImage = SafeImageUrl(googleBook.VolumeInfo.ImageLinks?.Thumbnail),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsAvailable = true
                };

                await _bookService.CreateAsync(book);

                // Kitap eklendikten sonra varsayılan olarak "Okuyacaklarım" durumunda UserBookStatus oluştur
                var defaultBookStatus = new UserBookStatus
                {
                    BookId = book.Id,
                    UserId = userId,
                    Status = ReadingStatus.WantToRead, // Varsayılan olarak "Okuyacaklarım"
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Notes = null // Notes alanını boş bırakarak karakter sınırı aşılmasını önle
                };

                _context.UserBookStatuses.Add(defaultBookStatus);
                await _context.SaveChangesAsync();

                // Kullanıcının okunan kitap sayısını güncelle
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }

                return Json(new { 
                    success = true, 
                    message = "Kitap başarıyla kütüphanenize eklendi ve 'Okuyacaklarım' listesine eklendi!",
                    bookId = book.Id,
                    bookStatus = (int)ReadingStatus.WantToRead,
                    statusText = "Okuyacaklarım",
                    statusIcon = "📚"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kitap eklenirken hata oluştu: {ex.Message}" });
            }
        }

        // GET: /Profile/GetBookStatusCounts - AJAX endpoint for getting current book status counts
        [HttpGet]
        public async Task<IActionResult> GetBookStatusCounts()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }

                var statusCounts = await _bookStatusService.GetStatusCountsAsync(userId);
                
                return Json(new { 
                    success = true,
                    currentlyReadingCount = statusCounts[ReadingStatus.CurrentlyReading],
                    wantToReadCount = statusCounts[ReadingStatus.WantToRead],
                    readCount = statusCounts[ReadingStatus.Read]
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
        
        // GET: /Profile/AddQuote - Alıntı ekleme sayfası
        public async Task<IActionResult> AddQuote()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            // Kullanıcının kitaplarını getir
            var userBooks = await _context.Books
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
            
            var viewModel = new AddQuoteViewModel
            {
                AvailableBooks = userBooks
            };
            
            return View(viewModel);
        }
        
        // POST: /Profile/AddQuote - Alıntı ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuote(AddQuoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    if (userId == 0)
                    {
                        return RedirectToAction("Login", "Auth");
                    }
                    
                    // Kitabın kullanıcıya ait olup olmadığını kontrol et
                    var book = await _context.Books
                        .FirstOrDefaultAsync(b => b.Id == model.BookId && b.UserId == userId && b.IsActive);
                    
                    if (book == null)
                    {
                        ModelState.AddModelError("BookId", "Geçersiz kitap seçimi.");
                        model.AvailableBooks = await _context.Books
                            .Where(b => b.UserId == userId && b.IsActive)
                            .OrderBy(b => b.Title)
                            .ToListAsync();
                        return View(model);
                    }
                    
                    // Yeni alıntı oluştur
                    var quote = new Quote
                    {
                        Content = model.Content,
                        Author = model.Author,
                        Source = model.Source,
                        PageNumber = model.PageNumber,
                        Notes = model.Notes,
                        BookId = model.BookId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    
                    _context.Quotes.Add(quote);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Alıntı başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Alıntı eklenirken hata oluştu: {ex.Message}");
                }
            }
            
            // Hata durumunda mevcut kitapları tekrar yükle
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            model.AvailableBooks = await _context.Books
                .Where(b => b.UserId == currentUserId && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
            
            return View(model);
        }

        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + folderName + "/" + uniqueFileName;
        }
        // URL uzunluğunu kontrol eden yardımcı metot
        private string SafeImageUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
                
            // URL 450 karakterden uzunsa kısalt (güvenli bir sınır için)
            if (url.Length > 450)
            {
                Console.WriteLine($"UYARI: Kitap kapak resmi URL'si kısaltıldı. Orijinal uzunluk: {url.Length}");
                return url.Substring(0, 450);
            }
            
            return url;
        }

        // POST: /Profile/Follow
        [HttpPost]
        public async Task<IActionResult> Follow(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
            }

            if (currentUserId == userId)
            {
                return Json(new { success = false, message = "Kendinizi takip edemezsiniz." });
            }

            var success = await _followService.FollowUserAsync(currentUserId, userId);
            if (success)
            {
                var followerCount = await _followService.GetFollowerCountAsync(userId);
                var followingCount = await _followService.GetFollowingCountAsync(currentUserId);
                
                return Json(new { 
                    success = true, 
                    message = "Kullanıcı takip edildi.",
                    followerCount = followerCount,
                    followingCount = followingCount
                });
            }

            return Json(new { success = false, message = "Kullanıcı zaten takip ediliyor." });
        }

        // POST: /Profile/Unfollow
        [HttpPost]
        public async Task<IActionResult> Unfollow(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
            }

            var success = await _followService.UnfollowUserAsync(currentUserId, userId);
            if (success)
            {
                var followerCount = await _followService.GetFollowerCountAsync(userId);
                var followingCount = await _followService.GetFollowingCountAsync(currentUserId);
                
                return Json(new { 
                    success = true, 
                    message = "Kullanıcı takipten çıkarıldı.",
                    followerCount = followerCount,
                    followingCount = followingCount
                });
            }

            return Json(new { success = false, message = "Kullanıcı zaten takip edilmiyor." });
        }

        // GET: /Profile/Followers
        public async Task<IActionResult> Followers(int userId)
        {
            var followers = await _followService.GetFollowersAsync(userId);
            return View(followers);
        }

        // GET: /Profile/Following
        public async Task<IActionResult> Following(int userId)
        {
            var following = await _followService.GetFollowingAsync(userId);
            return View(following);
        }

        // GET: /Profile/GetFollowStatus
        [HttpGet]
        public async Task<IActionResult> GetFollowStatus(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
            }

            var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
            return Json(new { success = true, isFollowing = isFollowing });
        }
    }
}
