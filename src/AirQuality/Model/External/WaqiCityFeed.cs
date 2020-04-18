using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Latincoder.AirQuality.Model.External
{
    public class WaqiCityFeed
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }

    }

    public class Data {
        [JsonPropertyName("attributions")]
        public List<Attribution> Attributions { get; set; }

        /// <summary>
        /// Represents the index that determines air quality, lower is better
        /// </summary>
        /// <value>Air Quality Index which may be int or string if it is not available</value>
        /// <remarks>Whenever a type object is deserialized we get a JsonElement</remarks>
        [JsonPropertyName("aqi")]
        public object Aqi { get; set; }

        [JsonPropertyName("idx")]
        public int Idx { get; set; }

        [JsonPropertyName("time.s")]
        public string TimeStr { get; set; }

        [JsonPropertyName("city")]
        public City City { get; set; }
    }

    public class City {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("geo")]
        public List<float> Coordinates { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

    }

    public class Attribution {
        public Attribution() {}

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
