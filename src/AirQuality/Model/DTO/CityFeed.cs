using Latincoder.AirQuality.Model.External;

using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace Latincoder.AirQuality.Model.DTO
{
    public class CityFeed
    {
        private const int AqiNotAvailable = -1;

        /* Constructor methods*/
        internal CityFeed() {}

        public CityFeed(List<Station> stations, string cityName) {
            Stations = stations;
            CityName = cityName;
        }


        public static CityFeed From(List<WaqiCityFeed> waqiStations) {
            var stations = from station in waqiStations
                            let dynamicAqi = station.Data.Aqi
                            // System.Text.Json obtains JsonElement - if serializer changes this will fail
                            let aqi = (JsonElement) dynamicAqi
                            let isNumber = aqi.ValueKind == JsonValueKind.Number
                            select new Station(station.Data.Idx.ToString(),
                                isNumber ? aqi.GetInt32() : AqiNotAvailable,
                                station.Data.City.Name,
                                station.Data.City.Url,
                                station.Data.TimeStr,
                                Attribution.ListFrom(station.Data.Attributions));

            return new CityFeed {
                Stations = stations.ToList(),
                CityName = CityFeed.findBestCityName(stations.ToList())
            };
        }

        /* Properties */
        public string UpdatedAtText { get =>  MaxAqiStation != null? MaxAqiStation.Time: string.Empty; }

        // _maxAqiStation.Time is guaranteed to be a valid DateTime, hence rely on Parse()
        public DateTime UpdatedAt { get => MaxAqiStation != null ? DateTime.Parse(MaxAqiStation.Time): DateTime.Now; }
        public string CityName { get; set; } = string.Empty;

        // TODO: accessing MaxAqiStation is O(n) - cache result in private field
        public int MaxAQI { get => MaxAqiStation !=null? MaxAqiStation.AQI:-1; }

        public List<Station> Stations { get; set; }

        // TODO: accessing MaxAqiStation is O(n) - cache result in private field
        public Station MaxAqiStation { get => (from station in Stations
                        orderby station.AQI descending
                        select station).First(); }

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
