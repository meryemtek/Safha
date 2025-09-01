using System.Text.Json.Serialization;

namespace UI.Models.GoogleBooks
{
    public class Volume
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("selfLink")]
        public string SelfLink { get; set; } = string.Empty;

        [JsonPropertyName("volumeInfo")]
        public VolumeInfo VolumeInfo { get; set; } = new VolumeInfo();
    }
}
