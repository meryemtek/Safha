using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UI.Services;
using UI.Models.GoogleBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.EntityFramework.Context;
using Entities;
using System.Security.Claims;
using Core;
using System.Linq.Expressions;
using UI.Models;

namespace UI.Controllers
{
    public class BookController : Controller
    {
        private readonly GoogleBooksService _googleBooksService;
        private readonly ICrudService<Book> _bookService;
        private readonly ICrudService<UserBookStatus> _userBookStatusService;
        private readonly ICrudService<Quote> _quoteService;
        private readonly SafhaDbContext _context;
        private readonly IBookStatusService _bookStatusService;

        public BookController(
            GoogleBooksService googleBooksService, 
            ICrudService<Book> bookService, 
            ICrudService<UserBookStatus> userBookStatusService,
            ICrudService<Quote> quoteService,
            SafhaDbContext context,
            IBookStatusService bookStatusService)
        {
            _googleBooksService = googleBooksService;
            _bookService = bookService;
            _userBookStatusService = userBookStatusService;
            _quoteService = quoteService;
            _context = context;
            _bookStatusService = bookStatusService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            try
            {
                // AJAX isteği mi yoksa normal sayfa isteği mi kontrol et
                bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

                if (string.IsNullOrEmpty(query))
                {
                    if (isAjaxRequest)
                    {
                        return Json(new VolumesResponse());
                    }
                    return View(new VolumesResponse());
                }

                int maxResults = 10;
                int startIndex = (page - 1) * maxResults;

                var result = await _googleBooksService.SearchBooksAsync(query, maxResults, startIndex);
                
                ViewBag.Query = query;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalItems / maxResults);
                
                // AJAX isteği ise JSON döndür
                if (isAjaxRequest)
                {
                    return Json(result);
                }
                
                return View(result);
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi ver
                TempData["ErrorMessage"] = $"Kitap arama sırasında hata oluştu: {ex.Message}";
                Console.WriteLine($"BookController Search hatası: {ex.Message}");
                
                // AJAX isteği ise JSON hata döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { error = ex.Message });
                }
                
                // Hata durumunda boş sonuç döndür
                ViewBag.Query = query;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = 0;
                
                return View(new VolumesResponse());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Index", "Home");
                }

                var book = await _googleBooksService.GetBookByIdAsync(id);
                
                // Kullanıcı giriş yapmışsa, kitabın kitaplıktaki durumunu kontrol et
                if (User.Identity.IsAuthenticated)
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    if (userId > 0)
                    {
                        // Kitabın veritabanında olup olmadığını kontrol et
                        string isbn = book.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier ?? "";
                        if (!string.IsNullOrEmpty(isbn))
                        {
                            Expression<Func<Book, bool>> bookFilter = b => b.ISBN == isbn && b.IsActive;
                            var dbBook = await _bookService.GetAsync(bookFilter);
                            
                            if (dbBook != null)
                            {
                                ViewBag.DbBookId = dbBook.Id;
                                
                                // Kitabın kitaplıktaki durumunu kontrol et
                                Expression<Func<UserBookStatus, bool>> statusFilter = s => s.BookId == dbBook.Id && s.UserId == userId && s.IsActive;
                                var userBookStatus = await _userBookStatusService.GetAsync(statusFilter);
                                
                                if (userBookStatus != null)
                                {
                                    ViewBag.LibraryStatus = (int)userBookStatus.Status;
                                }
                            }
                        }
                    }
                }
                
                // AJAX isteği ise JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(book);
                }
                
                return View(book);
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi ver
                TempData["ErrorMessage"] = $"Kitap detayları alınırken hata oluştu: {ex.Message}";
                Console.WriteLine($"BookController Detail hatası: {ex.Message}");
                
                // AJAX isteği ise JSON hata döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { error = ex.Message });
                }
                
                // Hata durumunda ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }
        }
        


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReadingStatus(int bookId, int readingStatus)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Kitabın var olup olmadığını kontrol et
                var book = await _bookService.GetByIdAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Kitap bulunamadı.";
                    return RedirectToAction("MyBooks");
                }
                
                // Kapak resmi URL'sini kontrol et ve gerekirse kısalt
                if (!string.IsNullOrEmpty(book.CoverImage) && book.CoverImage.Length > 500)
                {
                    book.CoverImage = SafeImageUrl(book.CoverImage);
                    await _bookService.UpdateAsync(book);
                }
                
                // Diğer alanları da kontrol et ve gerekirse kısalt
                if (!string.IsNullOrEmpty(book.Title) && book.Title.Length > 250)
                {
                    book.Title = book.Title.Substring(0, 250);
                    await _bookService.UpdateAsync(book);
                }
                
                if (!string.IsNullOrEmpty(book.Author) && book.Author.Length > 250)
                {
                    book.Author = book.Author.Substring(0, 250);
                    await _bookService.UpdateAsync(book);
                }
                
                if (!string.IsNullOrEmpty(book.Description) && book.Description.Length > 2000)
                {
                    book.Description = book.Description.Substring(0, 2000);
                    await _bookService.UpdateAsync(book);
                }

                // Kitabı kitaplığa ekle veya durumunu güncelle
                await _userBookStatusService.GetContext().Database.BeginTransactionAsync();
                
                try {
                    // Önce aktif olmayan bir durum var mı kontrol et
                    Expression<Func<UserBookStatus, bool>> anyStatusFilter = s => s.BookId == bookId && s.UserId == userId;
                    var anyStatus = await _userBookStatusService.GetAsync(anyStatusFilter);
                    
                    if (anyStatus != null && !anyStatus.IsActive)
                    {
                        // Varolan ama aktif olmayan durumu aktifleştir
                        anyStatus.IsActive = true;
                        anyStatus.Status = (ReadingStatus)readingStatus;
                        anyStatus.UpdatedAt = DateTime.UtcNow;
                        
                        // Notes alanını kontrol et
                        if (!string.IsNullOrEmpty(anyStatus.Notes) && anyStatus.Notes.Length > 500)
                        {
                            anyStatus.Notes = anyStatus.Notes.Substring(0, 500);
                        }
                        
                        if (readingStatus == (int)ReadingStatus.CurrentlyReading && !anyStatus.StartedReadingDate.HasValue)
                        {
                            anyStatus.StartedReadingDate = DateTime.UtcNow;
                            anyStatus.CurrentPage = 1;
                        }
                        else if (readingStatus == (int)ReadingStatus.Read && !anyStatus.FinishedReadingDate.HasValue)
                        {
                            anyStatus.FinishedReadingDate = DateTime.UtcNow;
                            
                            // Eğer başlama tarihi yoksa, bitirme tarihinden önce başladığını varsay
                            if (!anyStatus.StartedReadingDate.HasValue)
                            {
                                anyStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                            }
                        }
                        
                        await _userBookStatusService.UpdateAsync(anyStatus);
                        await _userBookStatusService.GetContext().Database.CommitTransactionAsync();
                        
                        TempData["SuccessMessage"] = "Kitap durumu güncellendi.";
                        
                        // AJAX isteği ise JSON döndür
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            // Kitap bilgilerini al
                            var bookDetails = await _bookService.GetByIdAsync(bookId);
                            
                            return Json(new { 
                                success = true, 
                                message = TempData["SuccessMessage"],
                                bookId = bookId,
                                book = new {
                                    id = bookDetails.Id,
                                    title = bookDetails.Title,
                                    author = bookDetails.Author,
                                    coverImage = string.IsNullOrEmpty(bookDetails.CoverImage) ? "/image/default-book-cover.jpg" : SafeImageUrl(bookDetails.CoverImage),
                                    description = bookDetails.Description,
                                    publicationYear = bookDetails.PublicationYear,
                                    pages = bookDetails.Pages,
                                    genre = bookDetails.Genre
                                },
                                status = (int)anyStatus.Status,
                                statusText = GetStatusText(anyStatus.Status),
                                statusIcon = GetStatusIcon(anyStatus.Status)
                            });
                        }
                        
                        return RedirectToAction("MyBooks");
                    }
                    
                    // Kitap zaten kitaplıkta mı kontrol et
                    Expression<Func<UserBookStatus, bool>> existingFilter = s => s.BookId == bookId && s.UserId == userId && s.IsActive;
                    var existingStatus = await _userBookStatusService.GetAsync(existingFilter);

                    if (existingStatus != null)
                    {
                        // Önceki durumu kaydet
                        var previousStatus = existingStatus.Status;
                        
                        // Mevcut durumu güncelle
                        existingStatus.Status = (ReadingStatus)readingStatus;
                        existingStatus.UpdatedAt = DateTime.UtcNow;
                        
                        // Notes alanını kontrol et
                        if (!string.IsNullOrEmpty(existingStatus.Notes) && existingStatus.Notes.Length > 500)
                        {
                            existingStatus.Notes = existingStatus.Notes.Substring(0, 500);
                        }
                        
                        // Notes alanını kontrol et
                        if (!string.IsNullOrEmpty(existingStatus.Notes) && existingStatus.Notes.Length > 500)
                        {
                            existingStatus.Notes = existingStatus.Notes.Substring(0, 500);
                        }
                        
                        // Okuma durumuna göre tarihleri güncelle
                        if (readingStatus == (int)ReadingStatus.CurrentlyReading && !existingStatus.StartedReadingDate.HasValue)
                        {
                            existingStatus.StartedReadingDate = DateTime.UtcNow;
                            existingStatus.CurrentPage = existingStatus.CurrentPage ?? 1;
                        }
                        else if (readingStatus == (int)ReadingStatus.Read && !existingStatus.FinishedReadingDate.HasValue)
                        {
                            existingStatus.FinishedReadingDate = DateTime.UtcNow;
                            
                            // Eğer başlama tarihi yoksa, bitirme tarihinden önce başladığını varsay
                            if (!existingStatus.StartedReadingDate.HasValue)
                            {
                                existingStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                            }
                        }
                        else if (readingStatus == (int)ReadingStatus.WantToRead)
                        {
                            // Eğer "Okuyacaklarım" durumuna geri döndüyse, başlama ve bitirme tarihlerini sıfırla
                            if (previousStatus == ReadingStatus.CurrentlyReading || previousStatus == ReadingStatus.Read)
                            {
                                existingStatus.StartedReadingDate = null;
                                existingStatus.FinishedReadingDate = null;
                                existingStatus.CurrentPage = null;
                            }
                        }

                        await _userBookStatusService.UpdateAsync(existingStatus);
                        
                        TempData["SuccessMessage"] = "Kitap durumu güncellendi.";
                    }
                    else
                    {
                        // Yeni durum oluştur
                        var newStatus = new UserBookStatus
                        {
                            BookId = bookId,
                            UserId = userId,
                            Status = (ReadingStatus)readingStatus,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            Notes = null // Notes alanını boş bırakarak karakter sınırı aşılmasını önle
                        };

                        // Okuma durumuna göre tarihleri ayarla
                        if (readingStatus == (int)ReadingStatus.CurrentlyReading)
                        {
                            newStatus.StartedReadingDate = DateTime.UtcNow;
                            newStatus.CurrentPage = 1;
                        }
                        else if (readingStatus == (int)ReadingStatus.Read)
                        {
                            // Okudum durumunda hem başlama hem de bitirme tarihlerini ayarla
                            newStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                            newStatus.FinishedReadingDate = DateTime.UtcNow;
                        }

                        await _userBookStatusService.CreateAsync(newStatus);
                        
                        TempData["SuccessMessage"] = "Kitap kitaplığınıza eklendi.";
                    }
                    
                    await _userBookStatusService.GetContext().Database.CommitTransactionAsync();
                    
                    // AJAX isteği ise JSON döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        // Güncel kitaplık verilerini getir
                        var updatedBook = await _bookService.GetByIdAsync(bookId);
                        var updatedStatus = await _userBookStatusService.GetAsync(s => s.BookId == bookId && s.UserId == userId && s.IsActive);
                        
                        return Json(new { 
                            success = true, 
                            message = TempData["SuccessMessage"],
                            bookId = bookId,
                            book = new {
                                id = updatedBook.Id,
                                title = updatedBook.Title,
                                author = updatedBook.Author,
                                coverImage = string.IsNullOrEmpty(updatedBook.CoverImage) ? "/image/default-book-cover.jpg" : SafeImageUrl(updatedBook.CoverImage),
                                description = updatedBook.Description,
                                publicationYear = updatedBook.PublicationYear,
                                pages = updatedBook.Pages,
                                genre = updatedBook.Genre
                            },
                            status = (int)(updatedStatus?.Status ?? 0),
                            statusText = GetStatusText(updatedStatus?.Status),
                            statusIcon = GetStatusIcon(updatedStatus?.Status)
                        });
                    }
                    
                    return RedirectToAction("MyBooks");
                }
                catch (Exception ex)
                {
                    await _userBookStatusService.GetContext().Database.RollbackTransactionAsync();
                    // Orijinal stack trace'i korumak için throw; kullan
                    throw;
                }
            }
            catch (Exception ex)
            {
                // İç hatayı da göster
                string errorMessage = $"Kitap durumu güncellenirken hata oluştu: {ex.Message}";
                
                // İç hata varsa onu da ekle
                if (ex.InnerException != null)
                {
                    errorMessage += $" İç hata: {ex.InnerException.Message}";
                }
                
                // Hata detaylarını konsola yaz
                Console.WriteLine($"UpdateReadingStatus Hatası: {errorMessage}");
                Console.WriteLine($"Hata Stack Trace: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = errorMessage;
                
                // AJAX isteği ise JSON hata döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { error = errorMessage });
                }
                
                return RedirectToAction("MyBooks");
            }
        }
        
        // Durum metni ve simgesi için yardımcı metotlar
        private string GetStatusText(ReadingStatus? status)
        {
            if (!status.HasValue)
                return "";
                
            return status switch
            {
                ReadingStatus.WantToRead => "Okuyacaklarım",
                ReadingStatus.CurrentlyReading => "Okuyorum",
                ReadingStatus.Read => "Okuduklarım",
                _ => ""
            };
        }
        
        private string GetStatusIcon(ReadingStatus? status)
        {
            if (!status.HasValue)
                return "📚";
                
            return status switch
            {
                ReadingStatus.WantToRead => "📚",
                ReadingStatus.CurrentlyReading => "📖",
                ReadingStatus.Read => "✅",
                _ => "📚"
            };
        }

        // ========== TEMEL CRUD İŞLEMLERİ ==========

        // CREATE - Kitap Ekleme
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? coverImageFile)
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

                    // Kapak resmi yükleme
                    if (coverImageFile != null && coverImageFile.Length > 0)
                    {
                        var coverImagePath = await SaveCoverImage(coverImageFile);
                        book.CoverImage = coverImagePath;
                    }

                    // Kitap bilgilerini ayarla
                    book.UserId = userId;
                    book.CreatedAt = DateTime.UtcNow;
                    book.UpdatedAt = DateTime.UtcNow;
                    book.IsActive = true;
                    book.IsAvailable = true;

                    await _bookService.CreateAsync(book);

                    TempData["SuccessMessage"] = "Kitap başarıyla eklendi!";
                    return RedirectToAction(nameof(MyBooks));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Kitap eklenirken hata oluştu: {ex.Message}");
                }
            }

            return View(book);
        }

        // Kapak resmi kaydetme metodu
        private async Task<string> SaveCoverImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            // Dosya boyutu kontrolü (5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("Dosya boyutu 5MB'dan büyük olamaz.");

            // Dosya türü kontrolü
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Sadece JPG, PNG ve GIF formatında resim yükleyebilirsiniz.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "book-covers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/book-covers/" + uniqueFileName;
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

        // READ - Kullanıcının Kitapları
        [Authorize]
        public async Task<IActionResult> MyBooks(int? statusFilter = null)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kitap durumlarını ve kitapları ortak servisten getir
            var bookStatusSummary = await _bookStatusService.GetUserBookStatusSummaryAsync(currentUserId);
            
            // StatusFilter'a göre kitapları filtrele
            List<Book> filteredBooks;
            
            if (statusFilter.HasValue)
            {
                // Belirli bir durumdaki kitapları filtrele
                var statusBookIds = bookStatusSummary.BookStatuses
                    .Where(s => s.Status == (ReadingStatus)statusFilter.Value && s.IsActive)
                    .Select(s => s.BookId)
                    .ToList();
                
                filteredBooks = bookStatusSummary.Books
                    .Where(b => b.IsActive && statusBookIds.Contains(b.Id))
                    .ToList();
            }
            else
            {
                // Tüm kitapları getir
                filteredBooks = bookStatusSummary.Books
                    .Where(b => b.IsActive)
                    .ToList();
            }
            
            // Kitap ve durum bilgilerini birleştir
            var model = new BookLibraryViewModel
            {
                Books = filteredBooks,
                BookStatuses = bookStatusSummary.BookStatuses,
                StatusFilter = statusFilter,
                IsCurrentUser = true, // Kendi kitaplarını görüntülüyor
                
                // İstatistik bilgilerini BookStatusSummary'den al
                CurrentlyReadingCount = bookStatusSummary.CurrentlyReadingCount,
                WantToReadCount = bookStatusSummary.WantToReadCount,
                ReadCount = bookStatusSummary.ReadCount,
                TotalBooks = bookStatusSummary.TotalBooks
            };

            return View(model);
        }

        // UPDATE - Kitap Düzenleme
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            Expression<Func<Book, bool>> filter = b => b.Id == id && b.UserId == userId && b.IsActive;
            var book = await _bookService.GetAsync(filter);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    Expression<Func<Book, bool>> filter = b => b.Id == id && b.UserId == userId && b.IsActive;
                    var existingBook = await _bookService.GetAsync(filter);

                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Description = book.Description;
                    existingBook.ISBN = book.ISBN;
                    existingBook.PublicationYear = book.PublicationYear;
                    existingBook.Genre = book.Genre;
                    existingBook.Pages = book.Pages;
                    existingBook.Language = book.Language;
                    existingBook.Publisher = book.Publisher;

                    await _bookService.UpdateAsync(existingBook);

                    TempData["SuccessMessage"] = "Kitap başarıyla güncellendi!";
                    return RedirectToAction(nameof(MyBooks));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Kitap güncellenirken hata oluştu: {ex.Message}");
                }
            }

            return View(book);
        }

        // DELETE - Kitap Silme
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                Expression<Func<Book, bool>> filter = b => b.Id == id && b.UserId == userId && b.IsActive;
                var book = await _bookService.GetAsync(filter);

                if (book == null)
                {
                    TempData["ErrorMessage"] = "Kitap bulunamadı veya silme yetkiniz yok.";
                    return RedirectToAction(nameof(MyBooks));
                }

                // Soft delete - sadece IsActive'i false yap
                book.IsActive = false;
                book.UpdatedAt = DateTime.UtcNow;
                
                await _bookService.UpdateAsync(book);

                TempData["SuccessMessage"] = "Kitap başarıyla silindi!";
                
                // AJAX isteği ise JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Kitap başarıyla silindi!" });
                }
                
                return RedirectToAction(nameof(MyBooks));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap silinirken hata oluştu: {ex.Message}";
                
                // AJAX isteği ise JSON hata döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { error = ex.Message });
                }
                
                return RedirectToAction(nameof(MyBooks));
            }
        }

        // GET: /Book/GetLibraryStats - AJAX endpoint for getting current library statistics
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLibraryStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }

                var bookStatusSummary = await _bookStatusService.GetUserBookStatusSummaryAsync(userId);
                
                return Json(new { 
                    success = true,
                    totalBooks = bookStatusSummary.TotalBooks,
                    currentlyReadingCount = bookStatusSummary.CurrentlyReadingCount,
                    wantToReadCount = bookStatusSummary.WantToReadCount,
                    readCount = bookStatusSummary.ReadCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // GET: /Book/GetBookDetails/{id} - AJAX endpoint for getting book details
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookDetails(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }

                var book = await _bookService.GetByIdAsync(id);
                if (book == null || book.UserId != userId)
                {
                    return Json(new { success = false, message = "Kitap bulunamadı" });
                }

                return Json(new { 
                    success = true, 
                    book = new {
                        id = book.Id,
                        title = book.Title,
                        author = book.Author,
                        coverImage = string.IsNullOrEmpty(book.CoverImage) ? "/image/default-book-cover.jpg" : SafeImageUrl(book.CoverImage),
                        description = book.Description,
                        publicationYear = book.PublicationYear,
                        pages = book.Pages,
                        genre = book.Genre
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // READ - Kitap Detayı (Veritabanından)
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookService.GetByIdAsync(id);

            if (book == null || !book.IsActive)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Book/AddToLibrary - Kitabı kütüphaneye ekle ve durumunu belirle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToLibrary(string googleBookId, int readingStatus)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Google Books API'den kitap detaylarını al
                var googleBook = await _googleBooksService.GetBookByIdAsync(googleBookId);
                if (googleBook == null)
                {
                    TempData["ErrorMessage"] = "Kitap bulunamadı.";
                    return RedirectToAction("Search");
                }

                // Kitap zaten eklenmiş mi kontrol et
                var existingBook = await _context.Books
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                            b.Title == googleBook.VolumeInfo.Title && 
                                            b.Author == (googleBook.VolumeInfo.Authors != null && googleBook.VolumeInfo.Authors.Any() ? googleBook.VolumeInfo.Authors.First() : "Bilinmeyen Yazar") &&
                                            b.IsActive);

                if (existingBook != null)
                {
                    // Kitap zaten varsa, sadece durumunu güncelle
                    var existingStatus = await _context.UserBookStatuses
                        .FirstOrDefaultAsync(s => s.BookId == existingBook.Id && s.UserId == userId && s.IsActive);
                    
                    if (existingStatus != null)
                    {
                        // Mevcut durumu güncelle
                        existingStatus.Status = (ReadingStatus)readingStatus;
                        existingStatus.UpdatedAt = DateTime.UtcNow;
                        
                        // Notes alanını kontrol et
                        if (!string.IsNullOrEmpty(existingStatus.Notes) && existingStatus.Notes.Length > 500)
                        {
                            existingStatus.Notes = existingStatus.Notes.Substring(0, 500);
                        }
                        
                        // Okuma durumuna göre tarihleri güncelle
                        if (readingStatus == (int)ReadingStatus.CurrentlyReading && !existingStatus.StartedReadingDate.HasValue)
                        {
                            existingStatus.StartedReadingDate = DateTime.UtcNow;
                            existingStatus.CurrentPage = existingStatus.CurrentPage ?? 1;
                        }
                        else if (readingStatus == (int)ReadingStatus.Read && !existingStatus.FinishedReadingDate.HasValue)
                        {
                            existingStatus.FinishedReadingDate = DateTime.UtcNow;
                            
                            // Eğer başlama tarihi yoksa, bitirme tarihinden önce başladığını varsay
                            if (!existingStatus.StartedReadingDate.HasValue)
                            {
                                existingStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                            }
                        }
                        else if (readingStatus == (int)ReadingStatus.WantToRead)
                        {
                            // Eğer "Okuyacaklarım" durumuna geri döndüyse, başlama ve bitirme tarihlerini sıfırla
                            existingStatus.StartedReadingDate = null;
                            existingStatus.FinishedReadingDate = null;
                            existingStatus.CurrentPage = null;
                        }
                        
                        _context.Update(existingStatus);
                    }
                    else
                    {
                        // Yeni durum oluştur
                        var newStatus = new UserBookStatus
                        {
                            BookId = existingBook.Id,
                            UserId = userId,
                            Status = (ReadingStatus)readingStatus,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        
                        // Okuma durumuna göre tarihleri ayarla
                        if (readingStatus == (int)ReadingStatus.CurrentlyReading)
                        {
                            newStatus.StartedReadingDate = DateTime.UtcNow;
                            newStatus.CurrentPage = 1;
                        }
                        else if (readingStatus == (int)ReadingStatus.Read)
                        {
                            // Okudum durumunda hem başlama hem de bitirme tarihlerini ayarla
                            newStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                            newStatus.FinishedReadingDate = DateTime.UtcNow;
                        }
                        
                        _context.UserBookStatuses.Add(newStatus);
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kitap durumu başarıyla güncellendi.";
                }
                else
                {
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
                    
                    // Kitap durumu oluştur
                    var bookStatus = new UserBookStatus
                    {
                        BookId = book.Id,
                        UserId = userId,
                        Status = (ReadingStatus)readingStatus,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        Notes = null // Notes alanını boş bırakarak karakter sınırı aşılmasını önle
                    };
                    
                    // Okuma durumuna göre tarihleri ayarla
                    if (readingStatus == (int)ReadingStatus.CurrentlyReading)
                    {
                        bookStatus.StartedReadingDate = DateTime.UtcNow;
                        bookStatus.CurrentPage = 1;
                    }
                    else if (readingStatus == (int)ReadingStatus.Read)
                    {
                        // Okudum durumunda hem başlama hem de bitirme tarihlerini ayarla
                        bookStatus.StartedReadingDate = DateTime.UtcNow.AddDays(-7); // Varsayılan olarak 1 hafta önce başlamış kabul et
                        bookStatus.FinishedReadingDate = DateTime.UtcNow;
                    }
                    
                    _context.UserBookStatuses.Add(bookStatus);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Kitap başarıyla kütüphanenize eklendi!";
                }
                
                return RedirectToAction("MyBooks");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap eklenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Search");
            }
        }
        
        // POST: /Book/AddBookFromAPI - API'den kitap ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
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
        
        // ========== ALINTI İŞLEMLERİ ==========
        
        // GET: /Book/GetQuotes/{bookId} - Kitap için alıntıları getir
        [HttpGet]
        public async Task<IActionResult> GetQuotes(int bookId)
        {
            try
            {
                var quotes = await _context.Quotes
                    .Include(q => q.User)
                    .Where(q => q.BookId == bookId && q.IsActive)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();
                
                var quoteViewModels = quotes.Select(q => new
                {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    source = q.Source,
                    pageNumber = q.PageNumber,
                    notes = q.Notes,
                    createdAt = q.CreatedAt.ToString("dd MMM yyyy"),
                    userName = q.User?.FirstName + " " + q.User?.LastName,
                    userProfilePicture = string.IsNullOrEmpty(q.User?.ProfilePicture) ? "/image/default-avatar.jpg" : q.User.ProfilePicture,
                    canDelete = User.Identity.IsAuthenticated && 
                               int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") == q.UserId
                });
                
                return Json(new { success = true, quotes = quoteViewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Alıntılar alınırken hata oluştu: {ex.Message}" });
            }
        }
        
        // POST: /Book/AddQuote - Alıntı ekle
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuote(AddQuoteViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validasyon hatası", errors = errors });
                }
                
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Kullanıcı kimliği bulunamadı" });
                }
                
                // Kitabın var olup olmadığını kontrol et
                var book = await _bookService.GetByIdAsync(model.BookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Kitap bulunamadı" });
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
                
                await _quoteService.CreateAsync(quote);
                
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
                
                return Json(new { 
                    success = true, 
                    message = "Alıntı başarıyla eklendi!", 
                    quote = quoteViewModel 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Alıntı eklenirken hata oluştu: {ex.Message}" });
            }
        }
        
        // DELETE: /Book/DeleteQuote/{id} - Alıntı sil
        [HttpPost]
        [Authorize]
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
                var quote = await _quoteService.GetByIdAsync(id);
                if (quote == null)
                {
                    return Json(new { success = false, message = "Alıntı bulunamadı" });
                }
                
                if (quote.UserId != userId)
                {
                    return Json(new { success = false, message = "Bu alıntıyı silme yetkiniz yok" });
                }
                
                // Soft delete - sadece IsActive'i false yap
                quote.IsActive = false;
                quote.UpdatedAt = DateTime.UtcNow;
                
                await _quoteService.UpdateAsync(quote);
                
                return Json(new { success = true, message = "Alıntı başarıyla silindi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Alıntı silinirken hata oluştu: {ex.Message}" });
            }
        }
        
        // GET: /Book/GetUserQuotes/{userId} - Kullanıcının alıntılarını getir
        [HttpGet]
        public async Task<IActionResult> GetUserQuotes(int userId)
        {
            try
            {
                var quotes = await _context.Quotes
                    .Include(q => q.Book)
                    .Where(q => q.UserId == userId && q.IsActive)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();
                
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
                    canDelete = User.Identity.IsAuthenticated && 
                               int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") == userId
                });
                
                return Json(new { success = true, quotes = quoteViewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kullanıcı alıntıları alınırken hata oluştu: {ex.Message}" });
            }
        }
    }
}