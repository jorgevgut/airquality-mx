using Latincoder.AirQuality.Model.External;
using System.Text.Json;

namespace Latincoder.AirQuality.Tests.Values
{
    public static class Waqi
    {
        public static string WaqiAttributionUrl = "https://waqi.info/";
        public static string WaqiCityFeedResponseJSON = "{\"status\": \"ok\", \"data\": {     \"aqi\": 33,     \"idx\": 391,     \"attributions\": [         {             \"url\": \"http://sinaica.inecc.gob.mx/\",             \"name\": \"INECC - Instituto Nacional de Ecologia y Cambio Climtico\",             \"logo\": \"Mexico-INECC.png\"         },         {             \"url\": \"https://waqi.info/\",             \"name\": \"World Air Quality Index Project\"         }     ],     \"city\": {         \"geo\": [             20.680450840176,             -103.39873310432         ],         \"name\": \"Vallarta, Guadalajara, Mexico\",         \"url\": \"https://aqicn.org/city/mexico/guadalajara/vallarta\"     },     \"dominentpol\": \"o3\",     \"iaqi\": {         \"co\": {             \"v\": 2.1         },         \"h\": {             \"v\": 11.3         },         \"no2\": {             \"v\": 5.8         },         \"o3\": {             \"v\": 33         },         \"p\": {             \"v\": 845         },         \"pm10\": {             \"v\": 21         },         \"t\": {             \"v\": 33.1         },         \"w\": {             \"v\": 5         },         \"wg\": {             \"v\": 13         }     },     \"time\": {         \"s\": \"2020-04-09 19:00:00\",         \"tz\": \"-05:00\",         \"v\": 1586458800     },     \"debug\": {         \"sync\": \"2020-04-10T09:26:23+09:00\"     } }}";
        public static WaqiCityFeed WaqiCityFeedResponse() {
            return JsonSerializer.Deserialize<WaqiCityFeed>(WaqiCityFeedResponseJSON);
        }
        public static WaqiCityFeed CreateWaqiCityFeedResponse(string cityName, string stationName, int id, int aqi) {
            var rawJson = "{\"status\": \"ok\", \"data\": {\"aqi\":"+ aqi +","+
            "\"idx\": "+ id +",     \"attributions\": [{" +
            "\"url\": \"http://sinaica.inecc.gob.mx/\", \"name\": \"INECC - Instituto Nacional de Ecologia y Cambio Climatico\", "+
            "\"logo\": \"Mexico-INECC.png\"},{\"url\": \"https://waqi.info/\",\"name\": \"World Air Quality Index Project\"}],\"city\": {\"geo\": [20.680450840176,-103.39873310432],"+
            "\"name\": \""+ stationName +" , "+ cityName +", Mexico\",\"url\":"+
            " \"https://aqicn.org/city/mexico/" + cityName+"/" + stationName + "\"},\"dominentpol\": \"o3\",\"iaqi\": {\"co\": {\"v\": 2.1},\"h\": {\"v\": 11.3},\"no2\": {\"v\": 5.8},\"o3\": {\"v\": 33},\"p\": {             \"v\": 845         },         \"pm10\": {             \"v\": 21         },         \"t\": {             \"v\": 33.1         },         \"w\": {             \"v\": 5         },         \"wg\": {             \"v\": 13}},\"time\": {\"s\": \"2020-04-09 19:00:00\",\"tz\": \"-05:00\",\"v\": 1586458800     },\"debug\": {\"sync\": \"2020-04-10T09:26:23+09:00\"} }}";

            return JsonSerializer.Deserialize<WaqiCityFeed>(rawJson);
        }

        public static WaqiCityFeed CreateWaqiCityFeedResponseWithInvalidAqi(string cityName, string stationName, int id) {
            var rawJson = "{\"status\": \"ok\", \"data\": {\"aqi\":\"-\","+
            "\"idx\": "+ id +",     \"attributions\": [{" +
            "\"url\": \"http://sinaica.inecc.gob.mx/\", \"name\": \"INECC - Instituto Nacional de Ecologia y Cambio Climatico\", "+
            "\"logo\": \"Mexico-INECC.png\"},{\"url\": \"https://waqi.info/\",\"name\": \"World Air Quality Index Project\"}],\"city\": {\"geo\": [20.680450840176,-103.39873310432],"+
            "\"name\": \""+ stationName +" , "+ cityName +", Mexico\",\"url\":"+
            " \"https://aqicn.org/city/mexico/" + cityName+"/" + stationName + "\"},\"dominentpol\": \"o3\",\"iaqi\": {\"co\": {\"v\": 2.1},\"h\": {\"v\": 11.3},\"no2\": {\"v\": 5.8},\"o3\": {\"v\": 33},\"p\": {             \"v\": 845         },         \"pm10\": {             \"v\": 21         },         \"t\": {             \"v\": 33.1         },         \"w\": {             \"v\": 5         },         \"wg\": {             \"v\": 13}},\"time\": {\"s\": \"2020-04-09 19:00:00\",\"tz\": \"-05:00\",\"v\": 1586458800     },\"debug\": {\"sync\": \"2020-04-10T09:26:23+09:00\"} }}";

            return JsonSerializer.Deserialize<WaqiCityFeed>(rawJson);
        }
    }
}

