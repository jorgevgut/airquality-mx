using Latincoder.AirQuality.Model.External;
using Latincoder.AirQuality.Tests.Values;
using Latincoder.AirQuality.Model.DTO;

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Latincoder.AirQuality.Tests.Model.DTO
{
    public class CityFeedTests
    {
        private const string GuadalajaraCity = "Guadalajara";
        private const string VallartaStation = "Vallarta";
        private const string VallartaStationId = "101";
        private const string TlaquepaqueStation = "Tlaquepaque";
        private const string TlaquepaqueStationId = "102";
        private const string FailingStation = "failingStation";
        private const string FailingStationId = "99";

        private const int MaxAqi = 100;
        private const int MinAqi = 30;
        private const int InvalidAqi = -1;
        internal List<WaqiCityFeed> GuadalajaraCityStations = new List<WaqiCityFeed>();
        public CityFeedTests() {
            // setup Guadalajara city with 2 default
            //var sample = Waqi.WaqiCityFeedResponse();
            //GuadalajaraCityStations.Add(sample);
            GuadalajaraCityStations.Add(Waqi.CreateWaqiCityFeedResponse(GuadalajaraCity, VallartaStation, int.Parse(VallartaStationId), MinAqi));
            GuadalajaraCityStations.Add(Waqi.CreateWaqiCityFeedResponse(GuadalajaraCity, TlaquepaqueStation, int.Parse(TlaquepaqueStationId) ,MaxAqi));
        }

        [Fact]
        public void ShouldParseCityFeedWithInvalidAqiValue() {
            var waqiStations = new List<WaqiCityFeed>();
            waqiStations.Add(Values.Waqi.CreateWaqiCityFeedResponseWithInvalidAqi(GuadalajaraCity, FailingStation, int.Parse(FailingStationId)));
            var cityFeed = CityFeed.From(waqiStations);
            Assert.NotNull(cityFeed);
            Assert.NotNull(cityFeed.CityName);
            Assert.NotEmpty(cityFeed.Stations);
        }


        [Fact]
        public void ShouldCreateCityFeedFromExternalJson() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            Assert.NotNull(cityFeed);
            Assert.NotNull(cityFeed.CityName);
            Assert.NotEmpty(cityFeed.Stations);
        }

        [Fact]
        public void ShouldContainAllStations() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            Assert.NotNull(cityFeed);
            Assert.NotNull(cityFeed.CityName);
            Assert.Equal(GuadalajaraCityStations.Count, cityFeed.Stations.Count);
        }

        [Fact]
        public void ShouldDetectAndFindBestCityName() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            Assert.Equal(cityFeed.CityName, GuadalajaraCity);
        }

        [Fact]
        public void ShouldDetectMaxAqiFromMultipleStations() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            Assert.Equal(MaxAqi, cityFeed.MaxAQI);
        }

        [Fact]
        public void ShouldGetMaxAqiStation() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            // max aqi station is set on constructor, currently is Tlaquepaque
            Assert.Equal(TlaquepaqueStationId, cityFeed.MaxAqiStation.Id);
        }

        [Fact]
        public void ShouldHaveAtLeastOneAttribution() {
            var waqiStations = GuadalajaraCityStations;

            var cityFeed = CityFeed.From(waqiStations);
            // max aqi station is set on constructor, currently is Tlaquepaque
            Assert.NotEmpty(cityFeed.MaxAqiStation.Attributions);
            // attributions by default contain Waqi's URL, we should expect "n = waqiStations.Length" matches
            var attributions = from station in cityFeed.Stations
                                from attribution in station.Attributions
                                where attribution.Url.Contains(Waqi.WaqiAttributionUrl)
                                select attribution;
            Assert.Equal(attributions.Count(), waqiStations.Count());
        }


    }
}
