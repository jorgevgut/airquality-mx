using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Latincoder.AirQuality.Model.DTO;
using Latincoder.AirQuality.Model.External;
using Latincoder.AirQuality.Proxies;

using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace WaqiGetCityFeedLambda
{
    public class Function
    {
        /* Environment variables used by WaqiCityFeed Lambdas */
        const string EnvCitiesWaqiGetCityFeedLambda = "CITIES";
        const string EnvWaqiTokenKey = "TOKEN";
        const string EnvCityFeedSqsUrl = "CITYFEED_SQS_URL";

        private string _citiesRaw;
        private string _token;
        private string _sqsCityFeedUrl;

        private AmazonSQSClient _sqsClient;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(string input, ILambdaContext context)
        {
            _citiesRaw = Environment.GetEnvironmentVariable(EnvCitiesWaqiGetCityFeedLambda);
            _token = Environment.GetEnvironmentVariable(EnvWaqiTokenKey);
            _sqsCityFeedUrl = Environment.GetEnvironmentVariable(EnvCityFeedSqsUrl);
            _sqsClient = new AmazonSQSClient(); // use default sqs client

            await AsyncExecute(input, context);
        }

        public async Task<string> AsyncExecute(string input, ILambdaContext context) {


                var waqiProxy = Waqi.create(_token);
                var cities = JsonSerializer.Deserialize<List<Latincoder.AirQuality.Model.Config.City>>(_citiesRaw); // default C# serialization
                // process multiple cities
                var stationFeedsByCity = from city in cities
                                from station in city.Stations
                                let feed = new { Uri = $"{city.Country}/{city.Name}/{station}", CityName = city.Name}
                                group feed by $"{city.Country}-{city.Name}" into stationsByCity
                                select stationsByCity;

                var cityFeedsDTO = new List<CityFeed>();

                foreach(var cityFeeds in stationFeedsByCity) {
                    var feeds = from value in cityFeeds
                                select value.Uri;
                    var stationFeedsObtained = new List<WaqiCityFeed>();
                    foreach (var feed in feeds) {
                        System.Console.WriteLine($"Requesting feed for:{feed}");
                        string mutableString = string.Empty;
                        try {
                            mutableString = await waqiProxy.getCityFeed(feed);

                            if(mutableString != string.Empty) {
                                System.Console.WriteLine("found this result:\n");
                                System.Console.WriteLine(mutableString);
                                var wcf = JsonSerializer.Deserialize<WaqiCityFeed>(mutableString);
                                stationFeedsObtained.Add(wcf);
                            }
                        } catch(Exception e) { System.Console.WriteLine(e);}
                    }
                    try {
                        cityFeedsDTO.Add(CityFeed.From(stationFeedsObtained));
                    } catch(Exception e) { System.Console.WriteLine(e);}
                }
                var response = await sendCityFeed(cityFeedsDTO);
                System.Console.WriteLine(response.ToString());
                return "done";
        }

        private Task<SendMessageBatchResponse> sendCityFeed(List<CityFeed> cityFeeds) {
            // Prone to failure: requires a formal SQSClient designed to handle Errors and failures
            var batch = from feed in cityFeeds
                        select new SendMessageBatchRequestEntry(Guid.NewGuid().ToString(), JsonSerializer.Serialize(feed));
            return _sqsClient.SendMessageBatchAsync(_sqsCityFeedUrl, batch.ToList());
        }

    }
}
