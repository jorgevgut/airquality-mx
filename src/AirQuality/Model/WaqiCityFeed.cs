using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Latincoder.AirQuality.Model
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
        /// <value>Air Quality Index</value>
        [JsonPropertyName("aqi")]
        public int Aqi { get; set; }

        [JsonPropertyName("time.s")]
        public string TimeStr { get; }
        
    }

    public class City {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("geo")]
        public List<int> Coordinates { get; }

        [JsonPropertyName("url")]
        public string Url { get; }

    }

    public class Attribution {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}