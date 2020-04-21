using System.Collections.Generic;
using System;

namespace Latincoder.AirQuality.Model.DTO
{
    public class Station {

        public Station() {
            Id = Guid.NewGuid().ToString();
        }
        public Station(string id) {
            Id = id;
        }

        public Station(string id, int aqi, string name, string url, string timeStr, List<Attribution> attributions) {
            Id = id;
            AQI = aqi;
            Name = name;
            Url = url;
            Time = validateAndGetTimeString(timeStr);
            Attributions = attributions;
        }

        private string validateAndGetTimeString(string timeStr) {
            if(string.IsNullOrEmpty(timeStr)) {
                return DateTime.Now.ToString();
            }
            // TODO: should have error handling, however on any failure, use "now"
            if (DateTime.TryParse(timeStr, out _)) {
                return timeStr;
            }
            return DateTime.Now.ToString();

        }

        public string Id { get; private set; }
        public int AQI { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Time { get; set; } = DateTime.Now.ToString();

        public List<Attribution> Attributions { get; set; }
    }
}
