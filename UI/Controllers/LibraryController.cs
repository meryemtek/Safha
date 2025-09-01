using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Core;
using Entities;
using UI.Models;
using Microsoft.EntityFrameworkCore;

namespace UI.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ICrudService<Book> _bookService;
        private readonly ICrudService<UserBookStatus> _bookStatusService;

        public LibraryController(ICrudService<Book> bookService, ICrudService<UserBookStatus> bookStatusService)
        {
            _bookService = bookService;
            _bookStatusService = bookStatusService;
        }

        public async Task<IActionResult> Index(string searchQuery = "")
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new MyLibraryViewModel
            {
                SearchQuery = searchQuery
            };

            // Kullanıcının tüm kitap durumlarını getir
            Expression<Func<UserBookStatus, bool>> statusFilter = s => s.UserId == userId && s.IsActive;
            var userBookStatuses = await _bookStatusService.GetAllAsync(statusFilter, "Book");

            // Arama sorgusu varsa filtrele
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                userBookStatuses = userBookStatuses.Where(s => 
                    s.Book.Title.ToLower().Contains(searchQuery) || 
                    s.Book.Author.ToLower().Contains(searchQuery) ||
                    (s.Book.Genre != null && s.Book.Genre.ToLower().Contains(searchQuery)) ||
                    (s.Book.Description != null && s.Book.Description.ToLower().Contains(searchQuery))
                ).ToList();
            }

            // Duruma göre kategorize et
            foreach (var status in userBookStatuses)
            {
                var bookWithStatus = new BookWithStatus(status.Book, status);
                
                switch (status.Status)
                {
                    case ReadingStatus.WantToRead:
                        viewModel.WantToReadBooks.Add(bookWithStatus);
                        break;
                    case ReadingStatus.CurrentlyReading:
                        viewModel.CurrentlyReadingBooks.Add(bookWithStatus);
                        break;
                    case ReadingStatus.Read:
                        viewModel.ReadBooks.Add(bookWithStatus);
                        break;
                }
            }

            // Kitapları ekleme/güncelleme tarihine göre sırala
            viewModel.WantToReadBooks = viewModel.WantToReadBooks.OrderByDescending(b => b.Status.CreatedAt).ToList();
            viewModel.CurrentlyReadingBooks = viewModel.CurrentlyReadingBooks.OrderByDescending(b => b.Status.UpdatedAt ?? b.Status.CreatedAt).ToList();
            viewModel.ReadBooks = viewModel.ReadBooks.OrderByDescending(b => b.Status.FinishedReadingDate ?? b.Status.UpdatedAt ?? b.Status.CreatedAt).ToList();

            return View("MyLibrary", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToLibrary(int bookId, ReadingStatus status)
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
                    return RedirectToAction("Index");
                }

                // Kitap zaten kitaplıkta mı kontrol et
                Expression<Func<UserBookStatus, bool>> existingFilter = s => s.BookId == bookId && s.UserId == userId && s.IsActive;
                var existingStatus = await _bookStatusService.GetAsync(existingFilter);

                if (existingStatus != null)
                {
                    // Önceki durumu kaydet
                    var previousStatus = existingStatus.Status;
                    
                    // Mevcut durumu güncelle
                    existingStatus.Status = status;
                    existingStatus.UpdatedAt = DateTime.UtcNow;
                    
                    if (status == ReadingStatus.CurrentlyReading && !existingStatus.StartedReadingDate.HasValue)
                    {
                        existingStatus.StartedReadingDate = DateTime.UtcNow;
                    }
                    else if (status == ReadingStatus.Read && !existingStatus.FinishedReadingDate.HasValue)
                    {
                        existingStatus.FinishedReadingDate = DateTime.UtcNow;
                    }

                    await _bookStatusService.UpdateAsync(existingStatus);
                    
                    // Okunan kitap sayısını güncelle
                    await UpdateUserReadBookCount(userId, previousStatus, status);
                    
                    TempData["SuccessMessage"] = "Kitap durumu güncellendi.";
                }
                else
                {
                    // Yeni durum oluştur
                    var newStatus = new UserBookStatus
                    {
                        BookId = bookId,
                        UserId = userId,
                        Status = status,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    if (status == ReadingStatus.CurrentlyReading)
                    {
                        newStatus.StartedReadingDate = DateTime.UtcNow;
                        newStatus.CurrentPage = 1;
                    }
                    else if (status == ReadingStatus.Read)
                    {
                        newStatus.FinishedReadingDate = DateTime.UtcNow;
                    }

                    await _bookStatusService.CreateAsync(newStatus);
                    
                    // Okunan kitap sayısını güncelle
                    await UpdateUserReadBookCount(userId, ReadingStatus.WantToRead, status);
                    
                    TempData["SuccessMessage"] = "Kitap kitaplığınıza eklendi.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap eklenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int bookId, ReadingStatus status)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Kitap durumunu bul
                Expression<Func<UserBookStatus, bool>> filter = s => s.BookId == bookId && s.UserId == userId && s.IsActive;
                var bookStatus = await _bookStatusService.GetAsync(filter);

                if (bookStatus == null)
                {
                    TempData["ErrorMessage"] = "Kitap durumu bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Durumu güncelle
                bookStatus.Status = status;
                bookStatus.UpdatedAt = DateTime.UtcNow;

                if (status == ReadingStatus.CurrentlyReading && !bookStatus.StartedReadingDate.HasValue)
                {
                    bookStatus.StartedReadingDate = DateTime.UtcNow;
                    bookStatus.CurrentPage = bookStatus.CurrentPage ?? 1;
                }
                else if (status == ReadingStatus.Read && !bookStatus.FinishedReadingDate.HasValue)
                {
                    bookStatus.FinishedReadingDate = DateTime.UtcNow;
                }

                await _bookStatusService.UpdateAsync(bookStatus);
                TempData["SuccessMessage"] = "Kitap durumu güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap durumu güncellenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int bookId, int currentPage, string notes)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Kitap durumunu bul
                Expression<Func<UserBookStatus, bool>> filter = s => s.BookId == bookId && s.UserId == userId && s.IsActive;
                var bookStatus = await _bookStatusService.GetAsync(filter);

                if (bookStatus == null)
                {
                    TempData["ErrorMessage"] = "Kitap durumu bulunamadı.";
                    return RedirectToAction("Index");
                }

                // İlerlemeyi güncelle
                bookStatus.CurrentPage = currentPage;
                bookStatus.Notes = notes;
                bookStatus.UpdatedAt = DateTime.UtcNow;

                // Kitabın toplam sayfa sayısını kontrol et
                var book = await _bookService.GetByIdAsync(bookId);
                if (book != null && book.Pages.HasValue && currentPage >= book.Pages.Value)
                {
                    bookStatus.Status = ReadingStatus.Read;
                    bookStatus.FinishedReadingDate = DateTime.UtcNow;
                    TempData["SuccessMessage"] = "Tebrikler! Kitabı bitirdiniz.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Okuma ilerlemeniz güncellendi.";
                }

                await _bookStatusService.UpdateAsync(bookStatus);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İlerleme güncellenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromLibrary(int bookId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Kitap durumunu bul
                Expression<Func<UserBookStatus, bool>> filter = s => s.BookId == bookId && s.UserId == userId && s.IsActive;
                var bookStatus = await _bookStatusService.GetAsync(filter);

                if (bookStatus == null)
                {
                    TempData["ErrorMessage"] = "Kitap durumu bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Soft delete
                bookStatus.IsActive = false;
                bookStatus.UpdatedAt = DateTime.UtcNow;

                await _bookStatusService.UpdateAsync(bookStatus);
                TempData["SuccessMessage"] = "Kitap kitaplığınızdan kaldırıldı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap kaldırılırken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Kullanıcının okunan kitap sayısını günceller
        /// </summary>
        private async Task UpdateUserReadBookCount(int userId, ReadingStatus previousStatus, ReadingStatus newStatus)
        {
            try
            {
                // Kullanıcıyı bul
                var user = await _bookService.GetContext().Set<User>().FindAsync(userId);
                if (user == null) return;

                var readCountChange = 0;

                // Önceki durumdan çıkar
                if (previousStatus == ReadingStatus.Read)
                {
                    readCountChange--;
                }

                // Yeni duruma ekle
                if (newStatus == ReadingStatus.Read)
                {
                    readCountChange++;
                }

                // Okunan kitap sayısını güncelle
                if (readCountChange != 0)
                {
                    user.ReadBookCount = Math.Max(0, user.ReadBookCount + readCountChange);
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    var context = _bookService.GetContext();
                    context.Entry(user).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir
                Console.WriteLine($"Okunan kitap sayısı güncellenirken hata: {ex.Message}");
            }
        }
    }
}
