using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using UI.Models.GoogleBooks;

namespace UI.Services
{
    public class GoogleBooksService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://www.googleapis.com/books/v1";
        private readonly GoogleBooksCacheService _cacheService;

        public GoogleBooksService(HttpClient httpClient, IConfiguration configuration, GoogleBooksCacheService cacheService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleBooks:ApiKey"] ?? string.Empty;
            _cacheService = cacheService;
        }

        public async Task<VolumesResponse> SearchBooksAsync(string query, int maxResults = 10, int startIndex = 0)
        {
            try
            {
                // Önce cache'den kontrol et
                var cachedResults = await _cacheService.GetCachedSearchResultsAsync(query);
                if (cachedResults != null)
                {
                    return cachedResults;
                }

                // Cache'de yoksa API'den al
                var url = $"{_baseUrl}/volumes?q={Uri.EscapeDataString(query)}&maxResults={maxResults}&startIndex={startIndex}&key={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Google Books API HTTP Hatası: {response.StatusCode} - {errorContent}");
                    
                    // API hatası durumunda cache'den eski sonuçları döndür
                    if (cachedResults != null)
                    {
                        return cachedResults;
                    }
                    
                    throw new Exception($"Google Books API hatası: {response.StatusCode} - {errorContent}");
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<VolumesResponse>(content);
                
                if (result != null)
                {
                    // Sonuçları cache'e kaydet (URL ile birlikte)
                    await _cacheService.SaveSearchResultsAsync(query, result, url);
                    return result;
                }
                else
                {
                    throw new Exception("API yanıtı deserialize edilemedi");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Books API hatası: {ex.Message}");
                
                // Cache'den eski sonuçları döndürmeye çalış
                try
                {
                    var cachedResults = await _cacheService.GetCachedSearchResultsAsync(query);
                    if (cachedResults != null)
                    {
                        return cachedResults;
                    }
                }
                catch (Exception cacheEx)
                {
                    Console.WriteLine($"Cache erişim hatası: {cacheEx.Message}");
                }
                
                // Hata durumunda boş sonuç döndür
                return new VolumesResponse();
            }
        }

        public async Task<Volume> GetBookByIdAsync(string id)
        {
            try
            {
                // Önce cache'den kontrol et
                var cachedBook = await _cacheService.GetCachedBookDetailsAsync(id);
                if (cachedBook != null)
                {
                    return cachedBook;
                }

                // Cache'de yoksa API'den al
                var url = $"{_baseUrl}/volumes/{id}?key={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Google Books API HTTP Hatası (Kitap ID: {id}): {response.StatusCode} - {errorContent}");
                    
                    // API hatası durumunda cache'den eski kitap bilgilerini döndür
                    if (cachedBook != null)
                    {
                        return cachedBook;
                    }
                    
                    throw new Exception($"Google Books API hatası: {response.StatusCode} - {errorContent}");
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Volume>(content);
                
                if (result != null)
                {
                    // Kitap bilgilerini cache'e kaydet (URL ile birlikte)
                    await _cacheService.SaveBookDetailsAsync(id, result, url);
                    return result;
                }
                else
                {
                    throw new Exception("API yanıtı deserialize edilemedi");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Books API hatası (Kitap ID: {id}): {ex.Message}");
                
                // Cache'den eski kitap bilgilerini döndürmeye çalış
                try
                {
                    var cachedBook = await _cacheService.GetCachedBookDetailsAsync(id);
                    if (cachedBook != null)
                    {
                        return cachedBook;
                    }
                }
                catch (Exception cacheEx)
                {
                    Console.WriteLine($"Cache erişim hatası: {cacheEx.Message}");
                }
                
                // Hata durumunda boş sonuç döndür
                return new Volume();
            }
        }

        /// <summary>
        /// URL log istatistiklerini getir
        /// </summary>
        public async Task<UrlLogStats> GetUrlLogStatsAsync()
        {
            return await _cacheService.GetUrlLogStatsAsync();
        }

        /// <summary>
        /// Belirli bir URL'nin cache durumunu kontrol et
        /// </summary>
        public async Task<UrlCacheStatus> GetUrlCacheStatusAsync(string url)
        {
            return await _cacheService.GetUrlCacheStatusAsync(url);
        }

        /// <summary>
        /// Cache'i temizle
        /// </summary>
        public void ClearCache()
        {
            _cacheService.ClearCache();
        }

        /// <summary>
        /// Eski cache dosyalarını temizle
        /// </summary>
        public void CleanupExpiredCache()
        {
            _cacheService.CleanupExpiredCache();
        }

        /// <summary>
        /// Belirli bir arama sorgusu için cache'i temizle
        /// </summary>
        public Task ClearSearchCacheAsync(string query)
        {
            try
            {
                var fileName = _cacheService.GetType().GetMethod("GetSafeFileName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .Invoke(_cacheService, new object[] { $"search_{query}" }) as string;
                
                if (!string.IsNullOrEmpty(fileName))
                {
                    var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "googlebooks");
                    var filePath = Path.Combine(cacheDir, fileName);
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Arama cache temizleme hatası: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
