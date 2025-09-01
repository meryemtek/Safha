# Google Books Cache Dizini

Bu dizin, Google Books API'den gelen verilerin JSON formatında cache'lenmesi için kullanılır.

## Dizin Yapısı

```
wwwroot/cache/googlebooks/
├── search_[arama_sorgusu].json    # Arama sonuçları cache'i (24 saat geçerli)
├── book_[kitap_id].json           # Kitap detayları cache'i (7 gün geçerli)
├── url_log.json                   # URL log dosyası (tüm API isteklerini kaydeder)
└── README.md                      # Bu dosya
```

## Cache Türleri

### 1. Arama Sonuçları Cache'i
- **Dosya Adı Formatı:** `search_[arama_sorgusu].json`
- **Geçerlilik Süresi:** 24 saat
- **İçerik:** Arama sorgusu, cache tarihi, sonuçlar ve API URL'i
- **Örnek:** `search_harry_potter.json`

### 2. Kitap Detayları Cache'i
- **Dosya Adı Formatı:** `book_[kitap_id].json`
- **Geçerlilik Süresi:** 7 gün
- **İçerik:** Kitap ID'si, cache tarihi, kitap bilgileri ve API URL'i
- **Örnek:** `book_abc123.json`

### 3. URL Log Sistemi
- **Dosya Adı:** `url_log.json`
- **İçerik:** Tüm API isteklerinin detaylı kaydı
- **Özellikler:**
  - Her isteğin URL'i, türü ve zamanı
  - Cache dosya adları ile eşleştirme
  - İstek istatistikleri ve analiz verileri

## URL Log Yapısı

```json
{
  "createdAt": "2024-01-01T00:00:00Z",
  "lastUpdated": "2024-01-01T12:00:00Z",
  "totalRequests": 150,
  "urlEntries": [
    {
      "url": "https://www.googleapis.com/books/v1/volumes?q=harry+potter",
      "type": "search",
      "query": "harry potter",
      "bookId": "",
      "timestamp": "2024-01-01T12:00:00Z",
      "cacheFileName": "search_harry_potter.json"
    }
  ]
}
```

## Cache Avantajları

1. **Performans Artışı:** Aynı arama sorguları için API'ye tekrar istek gönderilmez
2. **API Limit Korunması:** Google Books API limitlerini aşma riski azalır
3. **Hızlı Yanıt:** Cache'den veri çekme, API'den çekmekten çok daha hızlıdır
4. **Offline Erişim:** Cache'lenen veriler internet bağlantısı olmadan da erişilebilir
5. **URL Takibi:** Hangi URL'lerin cache'lendiği ve ne zaman erişildiği bilinir
6. **Analitik Veriler:** Popüler aramalar ve kullanım istatistikleri

## Cache Yönetimi

### Otomatik Temizleme
- Arama cache'leri 24 saat sonra otomatik olarak temizlenir
- Kitap cache'leri 7 gün sonra otomatik olarak temizlenir
- Süresi dolan cache dosyaları otomatik olarak silinir

### Manuel Temizleme
- `/Cache` sayfasından cache yönetimi yapılabilir
- Tüm cache temizlenebilir
- Belirli arama sorguları için cache temizlenebilir
- Eski cache dosyaları manuel olarak temizlenebilir

## URL Log Özellikleri

### İstatistikler
- **Toplam İstek Sayısı:** Kaç kez API'ye istek gönderildiği
- **Arama İstekleri:** Kitap arama isteklerinin sayısı
- **Kitap İstekleri:** Tekil kitap bilgisi isteklerinin sayısı
- **Son 24 Saat:** Son 24 saatteki istek sayısı
- **Son 7 Gün:** Son 7 gündeki istek sayısı

### Popüler Aramalar
- En çok yapılan arama sorguları
- Her sorgunun kaç kez yapıldığı
- Zaman bazlı analiz

### URL Cache Durumu Kontrolü
- Belirli bir URL'nin cache'de olup olmadığı
- Cache süresi ve geçerlilik durumu
- Cache dosya adı ve konumu

## Cache İstatistikleri

Cache yönetim sayfasından şu bilgiler görüntülenebilir:
- Toplam cache dosya sayısı
- Arama cache dosya sayısı
- Kitap cache dosya sayısı
- Toplam cache boyutu
- Son temizleme tarihi
- URL log istatistikleri
- Popüler arama sorguları

## Güvenlik

- Cache dosyaları sadece yetkili kullanıcılar tarafından yönetilebilir
- Cache dizini web erişimine açık değildir
- Cache dosyaları JSON formatında saklanır ve güvenlidir
- URL log'ları sadece sistem yöneticileri tarafından görüntülenebilir

## Sorun Giderme

### Cache Çalışmıyor
1. `wwwroot/cache/googlebooks` dizininin var olduğundan emin olun
2. Dizin yazma izinlerini kontrol edin
3. Disk alanının yeterli olduğundan emin olun
4. URL log dosyasının oluşturulduğunu kontrol edin

### Cache Çok Büyük
1. Eski cache dosyalarını temizleyin
2. Cache sürelerini kontrol edin
3. Gereksiz cache dosyalarını manuel olarak silin
4. URL log dosyasının boyutunu kontrol edin

### Performans Sorunları
1. Cache boyutunu kontrol edin
2. Eski cache dosyalarını temizleyin
3. Cache sürelerini optimize edin
4. URL log istatistiklerini analiz edin

### URL Log Sorunları
1. `url_log.json` dosyasının var olduğunu kontrol edin
2. Dosya yazma izinlerini kontrol edin
3. JSON formatının geçerli olduğunu kontrol edin
4. Disk alanının yeterli olduğundan emin olun

## Notlar

- Cache dosyaları UTF-8 encoding ile kaydedilir
- JSON dosyaları okunabilir formatta (indented) saklanır
- Cache dosya adları güvenli hale getirilir (geçersiz karakterler temizlenir)
- Cache boyutu otomatik olarak kontrol edilir
- URL log'ları gerçek zamanlı olarak güncellenir
- Her API isteği URL log'una kaydedilir
- Cache dosyaları ile URL log'ları otomatik olarak senkronize edilir
