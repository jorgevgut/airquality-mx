using System.Collections.Generic;
using System.Text.Json;

namespace Latincoder.AirQuality.Model.Config
{
        public class City {

        /// <summary>
        /// each station value represents the sation uri used from waqi API
        /// </summary>
        /// <value></value>
        public List<string> Stations { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class AirQualityLambdaOptions
    {
        public string Name { get; set; }
        public string Handler { get; set; }
        public List<KeyValuePair<string, string>> EnvironmentVariables { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Cities are property required for WaqiCityFeedLambda as URIs will
        /// be from it's values
        /// /// </summary>
        /// <value>List of City to support</value>
        public List<City> Cities { get; set; } = new List<City>(); // defaults to empty list
    }
}
