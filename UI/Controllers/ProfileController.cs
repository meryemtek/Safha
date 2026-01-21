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
                BookStatuses = bookStatusSummary.BookStatuses,
                IsOwnProfile = true
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
                TargetBookCount = user.TargetBookCount,
                CurrentProfilePicture = user.ProfilePicture,
                CurrentCoverPhoto = user.CoverPhoto
            };

            return View(editViewModel);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
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

            // Model validasyonu
            if (!ModelState.IsValid)
            {
                // Mevcut fotoğrafları koru
                model.CurrentProfilePicture = user.ProfilePicture;
                model.CurrentCoverPhoto = user.CoverPhoto;
                return View(model);
            }

            try
            {
                // Profil resmi yükleme
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    // Dosya tipi kontrolü
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.ProfilePictureFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Sadece JPG, PNG veya GIF formatında resim yükleyebilirsiniz.");
                        model.CurrentProfilePicture = user.ProfilePicture;
                        model.CurrentCoverPhoto = user.CoverPhoto;
                        return View(model);
                    }

                    // Dosya boyutu kontrolü (5MB)
                    if (model.ProfilePictureFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                        model.CurrentProfilePicture = user.ProfilePicture;
                        model.CurrentCoverPhoto = user.CoverPhoto;
                        return View(model);
                    }

                    var profilePicturePath = await SaveFile(model.ProfilePictureFile, "profile-pictures");
                    if (!string.IsNullOrEmpty(profilePicturePath))
                    {
                        user.ProfilePicture = profilePicturePath;
                    }
                }

                // Kapak fotoğrafı yükleme
                if (model.CoverPhotoFile != null && model.CoverPhotoFile.Length > 0)
                {
                    // Dosya tipi kontrolü
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.CoverPhotoFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("CoverPhotoFile", "Sadece JPG, PNG veya GIF formatında resim yükleyebilirsiniz.");
                        model.CurrentProfilePicture = user.ProfilePicture;
                        model.CurrentCoverPhoto = user.CoverPhoto;
                        return View(model);
                    }

                    // Dosya boyutu kontrolü (5MB)
                    if (model.CoverPhotoFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("CoverPhotoFile", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                        model.CurrentProfilePicture = user.ProfilePicture;
                        model.CurrentCoverPhoto = user.CoverPhoto;
                        return View(model);
                    }

                    var coverPhotoPath = await SaveFile(model.CoverPhotoFile, "cover-photos");
                    if (!string.IsNullOrEmpty(coverPhotoPath))
                    {
                        user.CoverPhoto = coverPhotoPath;
                    }
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
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Profil güncellenirken bir hata oluştu: {ex.Message}");
                model.CurrentProfilePicture = user.ProfilePicture;
                model.CurrentCoverPhoto = user.CoverPhoto;
                return View(model);
            }
        }

        // GET: /Profile/View/{id}
        [AllowAnonymous]
        public async Task<IActionResult> View(int id)
        {
            Console.WriteLine($"Profile/View çağrıldı. Görüntülenecek kullanıcı ID: {id}");
            
            var user = await _context.Users
                .Include(u => u.Followers.Where(f => f.IsActive))
                .Include(u => u.Following.Where(f => f.IsActive))
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                Console.WriteLine($"Kullanıcı bulunamadı. ID: {id}");
                return NotFound();
            }
            
            Console.WriteLine($"Kullanıcı bulundu: {user.Username} (ID: {user.Id})");

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
                IsFollowing = isFollowing,
                IsOwnProfile = !string.IsNullOrEmpty(currentUserId) && int.Parse(currentUserId) == id
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
            
            // Kullanıcının sadece "okuduğu" ve "okuyor" olduğu kitapları getir
            var userBookStatuses = await _context.UserBookStatuses
                .Where(ubs => ubs.UserId == userId && ubs.IsActive && 
                             (ubs.Status == ReadingStatus.Read || ubs.Status == ReadingStatus.CurrentlyReading))
                .Include(ubs => ubs.Book)
                .Where(ubs => ubs.Book != null && ubs.Book.IsActive)
                .OrderBy(ubs => ubs.Book.Title)
                .ToListAsync();
            
            var availableBooks = userBookStatuses.Select(ubs => ubs.Book).ToList();
            
            var viewModel = new AddQuoteViewModel
            {
                AvailableBooks = availableBooks
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
                    
                    // Kitabın var olup olmadığını ve kullanıcının bu kitabı okuduğunu/okuyor olduğunu kontrol et
                    var userBookStatus = await _context.UserBookStatuses
                        .Where(ubs => ubs.UserId == userId && ubs.BookId == model.BookId && ubs.IsActive &&
                                     (ubs.Status == ReadingStatus.Read || ubs.Status == ReadingStatus.CurrentlyReading))
                        .Include(ubs => ubs.Book)
                        .FirstOrDefaultAsync();
                    
                    if (userBookStatus?.Book == null)
                    {
                        ModelState.AddModelError("BookId", "Bu kitap için alıntı ekleyemezsiniz. Sadece okuduğunuz veya okumakta olduğunuz kitaplardan alıntı yapabilirsiniz.");
                        // Hata durumunda mevcut kitapları tekrar yükle
                        var availableUserBookStatuses = await _context.UserBookStatuses
                            .Where(ubs => ubs.UserId == userId && ubs.IsActive && 
                                         (ubs.Status == ReadingStatus.Read || ubs.Status == ReadingStatus.CurrentlyReading))
                            .Include(ubs => ubs.Book)
                            .Where(ubs => ubs.Book != null && ubs.Book.IsActive)
                            .OrderBy(ubs => ubs.Book.Title)
                            .ToListAsync();
                        
                        model.AvailableBooks = availableUserBookStatuses.Select(ubs => ubs.Book).ToList();
                        return View(model);
                    }
                    
                    var book = userBookStatus.Book;
                    
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
                    
                    Console.WriteLine($"ProfileController'da alıntı eklendi. ID: {quote.Id}");
                    
                    TempData["SuccessMessage"] = "Alıntı başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileController'da alıntı eklenirken hata: {ex.Message}");
                    ModelState.AddModelError("", $"Alıntı eklenirken hata oluştu: {ex.Message}");
                }
            }
            
            // Hata durumunda mevcut kitapları tekrar yükle
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var reloadUserBookStatuses = await _context.UserBookStatuses
                .Where(ubs => ubs.UserId == currentUserId && ubs.IsActive && 
                             (ubs.Status == ReadingStatus.Read || ubs.Status == ReadingStatus.CurrentlyReading))
                .Include(ubs => ubs.Book)
                .Where(ubs => ubs.Book != null && ubs.Book.IsActive)
                .OrderBy(ubs => ubs.Book.Title)
                .ToListAsync();
            
            model.AvailableBooks = reloadUserBookStatuses.Select(ubs => ubs.Book).ToList();
            
            return View(model);
        }
        
        // POST: /Profile/AddQuoteAjax - AJAX ile alıntı ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuoteAjax(AddQuoteViewModel model)
        {
            try
            {
                Console.WriteLine($"AddQuoteAjax çağrıldı. Model valid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    Console.WriteLine($"Model validasyon hataları: {string.Join(", ", errors)}");
                    return Json(new { success = false, message = "Validasyon hatası", errors = errors });
                }
                
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    Console.WriteLine("Kullanıcı kimliği bulunamadı");
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }
                
                Console.WriteLine($"Kullanıcı ID: {userId}, Alıntı içeriği: {model.Content?.Substring(0, Math.Min(model.Content?.Length ?? 0, 50))}...");
                
                // Kitabın var olup olmadığını ve kullanıcının bu kitabı okuduğunu/okuyor olduğunu kontrol et
                var userBookStatus = await _context.UserBookStatuses
                    .Where(ubs => ubs.UserId == userId && ubs.BookId == model.BookId && ubs.IsActive &&
                                 (ubs.Status == ReadingStatus.Read || ubs.Status == ReadingStatus.CurrentlyReading))
                    .Include(ubs => ubs.Book)
                    .FirstOrDefaultAsync();
                
                if (userBookStatus?.Book == null)
                {
                    Console.WriteLine($"Kitap bulunamadı veya kullanıcı bu kitabı okumuyor. BookId: {model.BookId}");
                    return Json(new { success = false, message = "Bu kitap için alıntı ekleyemezsiniz. Sadece okuduğunuz veya okumakta olduğunuz kitaplardan alıntı yapabilirsiniz." });
                }
                
                var book = userBookStatus.Book;
                
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
                
                Console.WriteLine($"ProfileController'da AJAX ile alıntı eklendi. ID: {quote.Id}");
                
                // Kullanıcı bilgilerini al
                var user = await _context.Users.FindAsync(userId);
                
                var quoteViewModel = new
                {
                    id = quote.Id,
                    content = quote.Content,
                    author = quote.Author,
                    source = quote.Source,
                    pageNumber = quote.PageNumber,
                    notes = quote.Notes,
                    createdAt = quote.CreatedAt.ToString("dd MMM yyyy"),
                    userName = user?.FirstName + " " + user?.LastName,
                    userProfilePicture = string.IsNullOrEmpty(user?.ProfilePicture) ? "/image/default-avatar.jpg" : user.ProfilePicture,
                    canDelete = true
                };
                
                var redirectUrl = Url.Action("Index", "Profile");
                Console.WriteLine($"Yönlendirme URL: {redirectUrl}");
                
                return Json(new { 
                    success = true, 
                    message = "Alıntı başarıyla eklendi!", 
                    quote = quoteViewModel,
                    redirectUrl = redirectUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileController'da AJAX alıntı eklenirken hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Alıntı eklenirken hata oluştu: {ex.Message}" });
            }
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
            try
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

                // Takip edilecek kullanıcının var olup olmadığını kontrol et
                var targetUser = await _context.Users.FindAsync(userId);
                if (targetUser == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
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
            catch (Exception ex)
            {
                Console.WriteLine($"Follow hatası: {ex.Message}");
                return Json(new { success = false, message = $"Bir hata oluştu: {ex.Message}" });
            }
        }

        // POST: /Profile/Unfollow
        [HttpPost]
        public async Task<IActionResult> Unfollow(int userId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                {
                    return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
                }

                // Takipten çıkarılacak kullanıcının var olup olmadığını kontrol et
                var targetUser = await _context.Users.FindAsync(userId);
                if (targetUser == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
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
            catch (Exception ex)
            {
                Console.WriteLine($"Unfollow hatası: {ex.Message}");
                return Json(new { success = false, message = $"Bir hata oluştu: {ex.Message}" });
            }
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

        // GET: /Profile/Users - Kullanıcı listesi
        [AllowAnonymous]
        public async Task<IActionResult> Users()
        {
            var currentUserId = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }

            var users = await _context.Users
                .Where(u => u.IsActive && u.Id != currentUserId)
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }
            
            // POST: /Profile/DeleteQuote - Alıntı silme
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteQuote(int id)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    if (userId == 0)
                    {
                        return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                    }
                    
                    // Alıntının var olup olmadığını ve kullanıcının yetkisini kontrol et
                    var quote = await _context.Quotes
                        .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
                    
                    if (quote == null)
                    {
                        return Json(new { success = false, message = "Alıntı bulunamadı" });
                    }
                    
                    // Soft delete - sadece IsActive'i false yap
                    quote.IsActive = false;
                    quote.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(quote);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"ProfileController'da alıntı silindi. ID: {id}");
                    
                    return Json(new { success = true, message = "Alıntı başarıyla silindi!" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileController'da alıntı silinirken hata: {ex.Message}");
                    return Json(new { success = false, message = $"Alıntı silinirken hata oluştu: {ex.Message}" });
                }
            }
            
            // GET: /Profile/GetUserQuotes/{userId} - Kullanıcının alıntılarını getir
        [HttpGet]
        public async Task<IActionResult> GetUserQuotes(int userId)
        {
            try
            {
                Console.WriteLine($"ProfileController GetUserQuotes çağrıldı. UserId: {userId}");
                
                // Önce tüm alıntıları kontrol et (IsActive kontrolü olmadan)
                var allQuotes = await _context.Quotes
                    .Where(q => q.UserId == userId)
                    .ToListAsync();
                
                Console.WriteLine($"Kullanıcının tüm alıntı sayısı (IsActive kontrolü olmadan): {allQuotes.Count}");
                
                // Her alıntıyı detaylı logla
                foreach (var quote in allQuotes)
                {
                    Console.WriteLine($"Alıntı ID: {quote.Id}, İçerik: {quote.Content?.Substring(0, Math.Min(quote.Content?.Length ?? 0, 30))}..., IsActive: {quote.IsActive}, BookId: {quote.BookId}");
                }
                
                // Aktif alıntıları filtrele ve Book bilgilerini yükle (sadece kitap alıntıları)
                var quotes = await _context.Quotes
                    .Include(q => q.Book)
                    .Where(q => q.UserId == userId && q.IsActive && q.BookId > 0)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();
                
                Console.WriteLine($"Aktif alıntı sayısı: {quotes.Count}");
                
                // Aktif alıntıları detaylı logla
                foreach (var quote in quotes)
                {
                    Console.WriteLine($"Aktif Alıntı ID: {quote.Id}, İçerik: {quote.Content?.Substring(0, Math.Min(quote.Content?.Length ?? 0, 30))}..., BookId: {quote.BookId}");
                }
                
                // Alıntıları detaylı logla
                foreach (var quote in quotes)
                {
                    Console.WriteLine($"Alıntı ID: {quote.Id}, İçerik: {quote.Content?.Substring(0, Math.Min(quote.Content?.Length ?? 0, 30))}..., Kitap ID: {quote.BookId}, Kitap: {quote.Book?.Title ?? "Kitap bilgisi yok"}, IsActive: {quote.IsActive}");
                }
                
                var quoteViewModels = quotes.Select(q => new
                {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    source = q.Source,
                    pageNumber = q.PageNumber,
                    notes = q.Notes,
                    createdAt = q.CreatedAt.ToString("dd MMM yyyy"),
                    bookTitle = q.Book?.Title ?? "Bilinmeyen Kitap",
                    bookAuthor = q.Book?.Author ?? "Bilinmeyen Yazar",
                    bookCoverImage = string.IsNullOrEmpty(q.Book?.CoverImage) ? "/image/default-book-cover.jpg" : q.Book.CoverImage,
                    canDelete = User.Identity?.IsAuthenticated == true && 
                               int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") == userId
                }).ToList();
                
                return Json(new { success = true, quotes = quoteViewModels });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileController GetUserQuotes hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Kullanıcı alıntıları alınırken hata oluştu: {ex.Message}" });
            }
        }
    }
}
