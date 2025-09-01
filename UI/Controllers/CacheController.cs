using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Services;

namespace UI.Controllers
{
    [Authorize]
    public class CacheController : Controller
    {
        private readonly GoogleBooksService _googleBooksService;

        public CacheController(GoogleBooksService googleBooksService)
        {
            _googleBooksService = googleBooksService;
        }

      
        public IActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult ClearAllCache()
        {
            try
            {
                _googleBooksService.ClearCache();
                TempData["SuccessMessage"] = "Tüm cache başarıyla temizlendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cache temizleme hatası: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Eski cache dosyalarını temizle
        /// </summary>
        [HttpPost]
        public IActionResult CleanupExpiredCache()
        {
            try
            {
                _googleBooksService.CleanupExpiredCache();
                TempData["SuccessMessage"] = "Eski cache dosyaları başarıyla temizlendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cache temizleme hatası: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Belirli bir arama sorgusu için cache'i temizle
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearSearchCache(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["ErrorMessage"] = "Arama sorgusu boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _googleBooksService.ClearSearchCacheAsync(query);
                TempData["SuccessMessage"] = $"'{query}' araması için cache başarıyla temizlendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cache temizleme hatası: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Cache istatistiklerini getir
        /// </summary>
        [HttpGet]
        public IActionResult GetCacheStats()
        {
            try
            {
                var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "googlebooks");
                var stats = new
                {
                    TotalFiles = 0,
                    SearchCacheFiles = 0,
                    BookCacheFiles = 0,
                    TotalSize = "0 KB",
                    LastCleanup = DateTime.Now
                };

                if (Directory.Exists(cacheDir))
                {
                    var files = Directory.GetFiles(cacheDir, "*.json");
                    var searchFiles = files.Count(f => f.Contains("search_"));
                    var bookFiles = files.Count(f => f.Contains("book_"));
                    
                    long totalSize = 0;
                    foreach (var file in files)
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        totalSize += fileInfo.Length;
                    }

                    stats = new
                    {
                        TotalFiles = files.Length,
                        SearchCacheFiles = searchFiles,
                        BookCacheFiles = bookFiles,
                        TotalSize = $"{totalSize / 1024} KB",
                        LastCleanup = DateTime.Now
                    };
                }

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// URL log istatistiklerini getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUrlLogStats()
        {
            try
            {
                var stats = await _googleBooksService.GetUrlLogStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir URL'nin cache durumunu kontrol et
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckUrlCacheStatus(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Json(new { Error = "URL boş olamaz" });
            }

            try
            {
                var status = await _googleBooksService.GetUrlCacheStatusAsync(url);
                return Json(status);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// URL log dosyasını görüntüle
        /// </summary>
        [HttpGet]
        public IActionResult ViewUrlLog()
        {
            try
            {
                var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "googlebooks");
                var urlLogFile = Path.Combine(cacheDir, "url_log.json");

                if (!System.IO.File.Exists(urlLogFile))
                {
                    return Json(new { Error = "URL log dosyası bulunamadı" });
                }

                var json = System.IO.File.ReadAllText(urlLogFile);
                var urlLog = System.Text.Json.JsonSerializer.Deserialize<object>(json);
                
                return Json(urlLog);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }
    }
}
