using Latincoder.AirQuality.Model.External;

using System.Linq;
using System.Collections.Generic;

namespace Latincoder.AirQuality.Model.DTO
{
    public class CityFeed
    {
        private Station _maxAqiStation;

        /* Constructor methods*/
        internal CityFeed() {}

        public CityFeed(List<Station> stations, string cityName) {
            Stations = stations;
            CityName = cityName;
            _maxAqiStation = (from station in stations
                        orderby station.AQI descending
                        select station).First();
        }


        public static CityFeed From(List<WaqiCityFeed> waqiStations) {
            var stations = from station in waqiStations
                            select new Station(station.Data.Idx.ToString(),
                                station.Data.Aqi,
                                station.Data.City.Name,
                                station.Data.City.Url,
                                Attribution.ListFrom(station.Data.Attributions));
            
            return new CityFeed{
                Stations = stations.ToList(),
                CityName = CityFeed.findBestCityName(stations.ToList()),
                _maxAqiStation = (from station in stations
                        orderby station.AQI descending
                        select station).First()
            };
        }

        /* Properties */
        public string CityName { get; set; }

        // TODO: accessing MaxAqiStation is O(n) - cache result in private field
        public int MaxAQI { get => _maxAqiStation.AQI; }

        public List<Station> Stations { get; set; }

        // TODO: accessing MaxAqiStation is O(n) - cache result in private field
        public Station MaxAqiStation { get => _maxAqiStation; }

        /* 
            internal methods - validations and utilities of this  class
            method which do not required of an instance are static

            TODO: use regex pattern to extract groups from valid Waqi API URI
            This is the known pattern:
            https://aqicn.org/city/:country/:city/:station
            Ex:
            https://aqicn.org/city/mexico/guadalajara/vallarta

        */

        //TODO: requires validation and error handling
        static string[] getCountryCityStationArray(string url) {
            var delimiter = @"city/";
            var delimiterIndex = url.IndexOf(delimiter);
            return url.Substring(delimiterIndex).Split('/');
        }

        static string getCityNameFromUrl(string url) {
            var countryCityName = getCountryCityStationArray(url);
            return countryCityName.Length > 2? countryCityName[2] : string.Empty;
        }

        static string findBestCityName(List<Station> stations) {
            var bestStation = (from station in stations
                    orderby station.AQI descending 
                    select station).ToList().First();
            return getCityNameFromUrl(bestStation.Url);
        }
    }

}