using Amazon.DynamoDBv2.DocumentModel;
using Latincoder.AirQuality.Model.DTO;
using System.Text.Json;

namespace Latincoder.AirQuality.Model.Dynamo
{
    public class CityFeedDocument
    {
        public static readonly string PartitionKeyName = "country-cityName";
        public static readonly string FieldMaxStationName = "max-station-name";
        public static readonly string FieldUpdatedTime = "last-update-time";
        public static readonly string FieldDeltaAqi = "last-delta-aqi";
        public static readonly string FieldMaxAqi = "max-aqi";
        public static readonly string JsonValue = "jsonValue";

        private const string _defaultCountry = "MX";

        public static Document From(CityFeed cityFeed) {
            var doc = new Document();
            // NOTE: default country is only MX as this is the only supported use case, however leaving room for scaling in format
            doc.Add(PartitionKeyName, $"{_defaultCountry}-{cityFeed.CityName}");
            doc.Add(FieldMaxStationName, $"{cityFeed.MaxAqiStation.Id}-{cityFeed.MaxAqiStation.Name}");
            doc.Add(FieldMaxAqi, cityFeed.MaxAQI);
            doc.Add(FieldUpdatedTime, cityFeed.UpdatedAtText);
            doc.Add(FieldDeltaAqi, 0); // delta is 0 by default
            doc.Add(JsonValue, JsonSerializer.Serialize(cityFeed));
            return doc;
        }

        /// <summary>
        /// Creates Document from a cityFeed but also accepts a previous Max aqui value
        /// to create a Delta
        /// delta is negative if aqi is less than previous
        /// </summary>
        /// <param name="cityFeed"></param>
        /// <param name="previousMaxAqi"></param>
        /// <returns></returns>
        public static Document From(CityFeed cityFeed, int previousMaxAqi) {
            var doc = new Document();
            // NOTE: default country is only MX as this is the only supported use case, however leaving room for scaling in format
            doc.Add(PartitionKeyName, $"{_defaultCountry}-{cityFeed.CityName}");
            doc.Add(FieldMaxStationName, $"{cityFeed.MaxAqiStation.Id}-{cityFeed.MaxAqiStation.Name}");
            doc.Add(FieldMaxAqi, cityFeed.MaxAQI);
            doc.Add(FieldDeltaAqi, cityFeed.MaxAQI - previousMaxAqi); // delta is null by default
            doc.Add(JsonValue, JsonSerializer.Serialize(cityFeed));
            doc.Add(FieldUpdatedTime, cityFeed.UpdatedAtText);
            return doc;
        }

        public static CityFeed ToDTO(Document doc) {
            if (doc.ContainsKey(JsonValue)) {
                return JsonSerializer.Deserialize<CityFeed>(doc[JsonValue].AsString());
            }
            return null;
        }

    }
}
