# KitApp

Kitap takip ve sosyal okuma platformu.

## Proje Yapısı

```
kitapp/git
├── Entities/                    # Domain modelleri (User, Book, Quote, Follow, vb.)
├── DataAccessLayer/             # Veritabanı erişim katmanı (EF Core, Repository Pattern)
├── Core/                        # İş mantığı katmanı
├── BussinessLogicLayer/         # Ek iş mantığı servisleri
├── DataTransferObject/          # DTO sınıfları
└── UI/                          # ASP.NET Core MVC web uygulaması
    ├── Controllers/             # MVC kontrolörleri
    ├── Models/                  # View modelleri
    ├── Views/                   # Razor görünümleri
    ├── Services/                # Uygulama servisleri
    └── wwwroot/                 # Statik dosyalar (CSS, JS, resimler)
```

## Teknolojiler

- **Backend:** ASP.NET Core 8.0 MVC
- **Veritabanı:** PostgreSQL
- **ORM:** Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript
- **API Entegrasyonu:** Google Books API

## Özellikler

- Kullanıcı kimlik doğrulama ve yetkilendirme
- Kitap arama (Google Books API)
- Kişisel kitaplık yönetimi (Okudum, Okuyorum, Okunacak)
- Kullanıcı profilleri
- Takip sistemi
- Alıntı paylaşımı
- Aktivite akışı

## Geliştirme Ortamı

### Gereksinimler

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 veya VS Code

### Kurulum

1. Repository'yi klonlayın
2. PostgreSQL bağlantı dizesini `UI/appsettings.json` dosyasında güncelleyin
3. Veritabanı migrasyonlarını uygulayın:
   ```bash
   dotnet ef database update --project DataAccessLayer --startup-project UI
   ```
4. Projeyi çalıştırın:
   ```bash
   dotnet run --project UI
   ```

## Lisans

Bu proje özel kullanım içindir.
