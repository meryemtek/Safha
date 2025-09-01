using System.Text.Json.Serialization;

namespace UI.Models.GoogleBooks
{
    public class VolumeInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;

        [JsonPropertyName("authors")]
        public List<string>? Authors { get; set; }

        [JsonPropertyName("publisher")]
        public string Publisher { get; set; } = string.Empty;

        [JsonPropertyName("publishedDate")]
        public string PublishedDate { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; }

        [JsonPropertyName("ratingsCount")]
        public int RatingsCount { get; set; }

        [JsonPropertyName("imageLinks")]
        public ImageLinks? ImageLinks { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("previewLink")]
        public string PreviewLink { get; set; } = string.Empty;

        [JsonPropertyName("infoLink")]
        public string InfoLink { get; set; } = string.Empty;

        [JsonPropertyName("industryIdentifiers")]
        public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }

        public string GetMainAuthor()
        {
            return Authors != null && Authors.Any() ? Authors[0] : "Bilinmeyen Yazar";
        }

        public string GetThumbnail()
        {
            return ImageLinks?.Thumbnail ?? "/image/book-cover1.jpg";
        }

        public int? GetPublishedYear()
        {
            if (string.IsNullOrEmpty(PublishedDate))
                return null;

            if (int.TryParse(PublishedDate, out int year))
                return year;

            if (DateTime.TryParse(PublishedDate, out DateTime date))
                return date.Year;

            return null;
        }
    }

    public class IndustryIdentifier
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty;
    }
}
