using System.Text.Json;
using UI.Models.GoogleBooks;

namespace UI.Services
{
    public class GoogleBooksCacheService
    {
        private readonly string _cacheDirectory;
        private readonly string _urlLogFile;
        private readonly JsonSerializerOptions _jsonOptions;

        public GoogleBooksCacheService()
        {
            _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "root", "cache", "googlebooks");
            _urlLogFile = Path.Combine(_cacheDirectory, "url_log.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Cache dizinini oluştur
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }

            // URL log dosyasını oluştur
            InitializeUrlLog();
        }

        private void InitializeUrlLog()
        {
            try
            {
                if (!File.Exists(_urlLogFile))
                {
                    var initialLog = new UrlLogData
                    {
                        CreatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                        UrlEntries = new List<UrlLogEntry>()
                    };

                    var json = JsonSerializer.Serialize(initialLog, _jsonOptions);
                    File.WriteAllText(_urlLogFile, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL log başlatma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// URL'yi log'a kaydet
        /// </summary>
        public async Task LogUrlAsync(string url, string type, string query = "", string bookId = "")
        {
            try
            {
                var urlLog = await LoadUrlLogAsync();
                if (urlLog == null) return;

                var entry = new UrlLogEntry
                {
                    Url = url,
                    Type = type,
                    Query = query,
                    BookId = bookId,
                    Timestamp = DateTime.UtcNow,
                    CacheFileName = GetCacheFileName(type, query, bookId)
                };

                // Aynı URL varsa güncelle, yoksa ekle
                var existingEntry = urlLog.UrlEntries.FirstOrDefault(e => e.Url == url);
                if (existingEntry != null)
                {
                    existingEntry.Timestamp = DateTime.UtcNow;
                    existingEntry.Query = query;
                    existingEntry.BookId = bookId;
                    existingEntry.CacheFileName = entry.CacheFileName;
                }
                else
                {
                    urlLog.UrlEntries.Add(entry);
                }

                urlLog.LastUpdated = DateTime.UtcNow;
                urlLog.TotalRequests = urlLog.UrlEntries.Count;

                await SaveUrlLogAsync(urlLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL log kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// URL log'unu yükle
        /// </summary>
        private async Task<UrlLogData?> LoadUrlLogAsync()
        {
            try
            {
                if (!File.Exists(_urlLogFile))
                    return null;

                var json = await File.ReadAllTextAsync(_urlLogFile);
                return JsonSerializer.Deserialize<UrlLogData>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// URL log'unu kaydet
        /// </summary>
        private async Task SaveUrlLogAsync(UrlLogData urlLog)
        {
            try
            {
                var json = JsonSerializer.Serialize(urlLog, _jsonOptions);
                await File.WriteAllTextAsync(_urlLogFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL log kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Cache dosya adını oluştur
        /// </summary>
        private string GetCacheFileName(string type, string query, string bookId)
        {
            if (type == "search")
                return GetSafeFileName($"search_{query}");
            else if (type == "book")
                return GetSafeFileName($"book_{bookId}");
            
            return string.Empty;
        }

        /// <summary>
        /// Kitap arama sonuçlarını cache'e kaydet
        /// </summary>
        public async Task SaveSearchResultsAsync(string query, VolumesResponse results, string url)
        {
            try
            {
                var fileName = GetSafeFileName($"search_{query}");
                var filePath = Path.Combine(_cacheDirectory, fileName);

                var cacheData = new SearchCacheData
                {
                    Query = query,
                    CachedAt = DateTime.UtcNow,
                    Results = results,
                    RequestUrl = url
                };

                var json = JsonSerializer.Serialize(cacheData, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                // URL'yi log'a kaydet
                await LogUrlAsync(url, "search", query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tekil kitap bilgisini cache'e kaydet
        /// </summary>
        public async Task SaveBookDetailsAsync(string bookId, Volume book, string url)
        {
            try
            {
                var fileName = GetSafeFileName($"book_{bookId}");
                var filePath = Path.Combine(_cacheDirectory, fileName);

                var cacheData = new BookCacheData
                {
                    BookId = bookId,
                    CachedAt = DateTime.UtcNow,
                    Book = book,
                    RequestUrl = url
                };

                var json = JsonSerializer.Serialize(cacheData, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                // URL'yi log'a kaydet
                await LogUrlAsync(url, "book", "", bookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitap cache kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// URL log istatistiklerini getir
        /// </summary>
        public async Task<UrlLogStats> GetUrlLogStatsAsync()
        {
            try
            {
                var urlLog = await LoadUrlLogAsync();
                if (urlLog == null)
                    return new UrlLogStats();

                var now = DateTime.UtcNow;
                var last24Hours = now.AddHours(-24);
                var last7Days = now.AddDays(-7);

                var stats = new UrlLogStats
                {
                    TotalRequests = urlLog.UrlEntries.Count,
                    SearchRequests = urlLog.UrlEntries.Count(e => e.Type == "search"),
                    BookRequests = urlLog.UrlEntries.Count(e => e.Type == "book"),
                    Last24Hours = urlLog.UrlEntries.Count(e => e.Timestamp >= last24Hours),
                    Last7Days = urlLog.UrlEntries.Count(e => e.Timestamp >= last7Days),
                    LastUpdated = urlLog.LastUpdated,
                    MostPopularQueries = urlLog.UrlEntries
                        .Where(e => e.Type == "search" && !string.IsNullOrEmpty(e.Query))
                        .GroupBy(e => e.Query)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new PopularQuery { Query = g.Key, Count = g.Count() })
                        .ToList()
                };

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL log istatistik hatası: {ex.Message}");
                return new UrlLogStats();
            }
        }

        /// <summary>
        /// Belirli bir URL'nin cache durumunu kontrol et
        /// </summary>
        public async Task<UrlCacheStatus> GetUrlCacheStatusAsync(string url)
        {
            try
            {
                var urlLog = await LoadUrlLogAsync();
                if (urlLog == null)
                    return new UrlCacheStatus { IsCached = false };

                var entry = urlLog.UrlEntries.FirstOrDefault(e => e.Url == url);
                if (entry == null)
                    return new UrlCacheStatus { IsCached = false };

                var cacheFile = Path.Combine(_cacheDirectory, entry.CacheFileName);
                var isExpired = false;

                if (entry.Type == "search")
                {
                    isExpired = entry.Timestamp.AddHours(24) < DateTime.UtcNow;
                }
                else if (entry.Type == "book")
                {
                    isExpired = entry.Timestamp.AddDays(7) < DateTime.UtcNow;
                }

                return new UrlCacheStatus
                {
                    IsCached = File.Exists(cacheFile) && !isExpired,
                    Type = entry.Type,
                    Query = entry.Query,
                    BookId = entry.BookId,
                    CachedAt = entry.Timestamp,
                    IsExpired = isExpired,
                    CacheFileName = entry.CacheFileName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL cache durum kontrol hatası: {ex.Message}");
                return new UrlCacheStatus { IsCached = false };
            }
        }

        /// <summary>
        /// Cache'den arama sonuçlarını getir
        /// </summary>
        public async Task<VolumesResponse?> GetCachedSearchResultsAsync(string query)
        {
            try
            {
                var fileName = GetSafeFileName($"search_{query}");
                var filePath = Path.Combine(_cacheDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var cacheData = JsonSerializer.Deserialize<SearchCacheData>(json, _jsonOptions);

                // Cache süresi kontrolü (24 saat)
                if (cacheData?.CachedAt.AddHours(24) < DateTime.UtcNow)
                {
                    File.Delete(filePath);
                    return null;
                }

                return cacheData?.Results;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cache'den kitap detaylarını getir
        /// </summary>
        public async Task<Volume?> GetCachedBookDetailsAsync(string bookId)
        {
            try
            {
                var fileName = GetSafeFileName($"book_{bookId}");
                var filePath = Path.Combine(_cacheDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var cacheData = JsonSerializer.Deserialize<BookCacheData>(json, _jsonOptions);

                // Cache süresi kontrolü (7 gün)
                if (cacheData?.CachedAt.AddDays(7) < DateTime.UtcNow)
                {
                    File.Delete(filePath);
                    return null;
                }

                return cacheData?.Book;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cache'i temizle
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    var files = Directory.GetFiles(_cacheDirectory, "*.json");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                }

                // URL log'u da temizle
                InitializeUrlLog();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Eski cache dosyalarını temizle
        /// </summary>
        public void CleanupExpiredCache()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                    return;

                var files = Directory.GetFiles(_cacheDirectory, "*.json");
                var now = DateTime.UtcNow;

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var isExpired = false;

                        // Dosya türüne göre süre kontrolü
                        if (file.Contains("search_"))
                        {
                            var searchData = JsonSerializer.Deserialize<SearchCacheData>(json, _jsonOptions);
                            isExpired = searchData?.CachedAt.AddHours(24) < now;
                        }
                        else if (file.Contains("book_"))
                        {
                            var bookData = JsonSerializer.Deserialize<BookCacheData>(json, _jsonOptions);
                            isExpired = bookData?.CachedAt.AddDays(7) < now;
                        }

                        if (isExpired)
                        {
                            File.Delete(file);
                        }
                    }
                    catch
                    {
                        // Bozuk dosyayı sil
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache temizleme hatası: {ex.Message}");
            }
        }

        private string GetSafeFileName(string baseName)
        {
            // Dosya adındaki geçersiz karakterleri temizle
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = invalidChars.Aggregate(baseName, (current, invalidChar) => current.Replace(invalidChar, '_'));
            
            // Çok uzun dosya adlarını kısalt
            if (safeName.Length > 100)
            {
                safeName = safeName.Substring(0, 100);
            }

            return $"{safeName}.json";
        }
    }

    // Cache veri modelleri
    public class SearchCacheData
    {
        public string Query { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
        public VolumesResponse Results { get; set; } = new();
        public string RequestUrl { get; set; } = string.Empty;
    }

    public class BookCacheData
    {
        public string BookId { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
        public Volume Book { get; set; } = new();
        public string RequestUrl { get; set; } = string.Empty;
    }

    // URL log modelleri
    public class UrlLogData
    {
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TotalRequests { get; set; }
        public List<UrlLogEntry> UrlEntries { get; set; } = new();
    }

    public class UrlLogEntry
    {
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "search" veya "book"
        public string Query { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CacheFileName { get; set; } = string.Empty;
    }

    public class UrlLogStats
    {
        public int TotalRequests { get; set; }
        public int SearchRequests { get; set; }
        public int BookRequests { get; set; }
        public int Last24Hours { get; set; }
        public int Last7Days { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PopularQuery> MostPopularQueries { get; set; } = new();
    }

    public class PopularQuery
    {
        public string Query { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class UrlCacheStatus
    {
        public bool IsCached { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
        public bool IsExpired { get; set; }
        public string CacheFileName { get; set; } = string.Empty;
    }
}
