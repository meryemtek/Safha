using System.Text.Json.Serialization;

namespace UI.Models.GoogleBooks
{
    public class VolumesResponse
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonPropertyName("items")]
        public List<Volume>? Items { get; set; }
    }
}
