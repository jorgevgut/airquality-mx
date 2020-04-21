using System.Collections.Generic;
using System.Linq;

namespace Latincoder.AirQuality.Model.DTO
{
        public class Attribution {
        public Attribution() {}

        public Attribution(string name = null, string url = null) {
            Name = name;
            Url = url;
        }

        public static Attribution From(Latincoder.AirQuality.Model.External.Attribution attribution) {
            return new Attribution(attribution.Name, attribution.Url);
        }

        public static List<Attribution> ListFrom(
            List<Latincoder.AirQuality.Model.External.Attribution> waqiAttributions) {
                return (from waqiAttribution  in waqiAttributions
                        select Attribution.From(waqiAttribution)).ToList();
        }

        public string Name { get; set; } = string.Empty;

        public string Url   { get; set; } = string.Empty;

        public string GetNameOrDefault() {
            if (Name != string.Empty) return Name;
            if (Url != string.Empty) return Url;
            return string.Empty;
        }

        public override string ToString() {
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Url)) {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Url)) {
                return $"Attr: {Name}${Url}";
            }

            return $"Attr: {Name} - {Url}";
        }
    }

}
