using System.Collections.Generic;

namespace Latincoder.AirQuality.Model.DTO
{
    public class Station {
        
        public Station(string id) {
            Id = id;
        }

        public Station(string id, int aqi, string name, string url, List<Attribution> attributions) {
            Id = id;
            AQI = aqi;
            Name = name;
            Url = url;
            Attributions = attributions;
        }

        public string Id { get; private set; }  
        public int AQI { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public List<Attribution> Attributions { get; set; }
    }
}