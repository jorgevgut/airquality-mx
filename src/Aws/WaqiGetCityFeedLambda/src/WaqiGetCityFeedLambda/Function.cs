using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Latincoder.AirQuality.Model.Dynamo;
using Latincoder.AirQuality.Model.DTO;
using Latincoder.AirQuality.Model.External;
using Latincoder.AirQuality.Proxies;
using Latincoder.AirQuality.Services;

using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace WaqiGetCityFeedLambda
{
    public class Function
    {
        /* Environment variables used by WaqiCityFeed Lambdas */
        const string EnvCitiesWaqiGetCityFeedLambda = "CITIES";
        const string EnvWaqiTokenKey = "TOKEN";
        const string EnvAirQualityTable = "TABLE_NAME";
        const string EnvCityFeedSqsUrl = "CITYFEED_SQS_URL";

        private string _citiesRaw;
        private string _token;
        private string _sqsCityFeedUrl;
        private string _AqiTableName;

        private AmazonSQSClient _sqsClient;
        private AmazonDynamoDBClient _dynamoClient;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(System.IO.Stream input, ILambdaContext context)
        {
            _citiesRaw = Environment.GetEnvironmentVariable(EnvCitiesWaqiGetCityFeedLambda);
            _token = Environment.GetEnvironmentVariable(EnvWaqiTokenKey);
            _sqsCityFeedUrl = Environment.GetEnvironmentVariable(EnvCityFeedSqsUrl);
            _sqsClient = new AmazonSQSClient(); // use default sqs client
            _dynamoClient = new AmazonDynamoDBClient(); // use default credentials for DB client
            _AqiTableName = Environment.GetEnvironmentVariable(EnvAirQualityTable);
            await AsyncExecute(input, context);
        }

        public async Task<string> AsyncExecute(System.IO.Stream input, ILambdaContext context) {
                _ = input; // discard
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

                var aqiTable = Table.LoadTable(_dynamoClient, _AqiTableName);
                var notificationIsUrgent = NotificationValidator.CreateyValidator();
                notificationIsUrgent.AddRules(Rules.AqiAboveUnhealthyThreshold);
                // store in DynamoDB if meets notification criteria
                foreach(var feed in cityFeedsDTO) {
                    // Gather previous and new feeds
                    var prevFeedDoc = await aqiTable.GetItemAsync(CityFeedDocument.PartitionKeyName);
                    var newFeed = prevFeedDoc != null ?
                        CityFeedDocument.From(feed, prevFeedDoc[CityFeedDocument.FieldMaxAqi].AsInt()) :
                        CityFeedDocument.From(feed);
                    // if no previous stuff
                    if (prevFeedDoc == null) {
                        Console.WriteLine($"FIRST TIME: Store in dynamo now for {feed.CityName}");
                        await aqiTable.PutItemAsync(newFeed);
                        continue;
                    }

                    var prevFeed = CityFeedDocument.ToDTO(prevFeedDoc);

                    // each feed will has a custom validator with it's own criteria since this is done per city
                    var notificationIsNotSpam = NotificationValidator.CreateyValidator();
                    // spam means max aqi changed at least by 5 points, and is at least 20 minutes apart frm last notification
                    notificationIsNotSpam.AddRules(
                        Rules.MinutesApartFrom(prevFeed, 30),
                        Rules.AbsoluteAqiChangedBy(prevFeed, 5));
                    // urgent or meets criteria
                    if (notificationIsUrgent.MeetsGlobalCriteria(feed) && notificationIsNotSpam.MeetsGlobalCriteria(feed)) {
                        Console.WriteLine($"Store in dynamo now for {feed.CityName}");
                        await aqiTable.PutItemAsync(newFeed);
                    } else {
                        Console.WriteLine("Does not meet rules...");
                    }

                }

                return "done";
        }

        /* Notes on development */

        // Deprecated code

        /* Using SQS prior to DynamoDB streams

        This is code was used to send an SQS batch message in order to trigger feed processor lambda
        however this was too costly since if there were not any changes, it would raise costs.

        usage:
        -> var response = await sendCityFeed(cityFeedsDTO);

        previous function:
            private Task<SendMessageBatchResponse> sendCityFeed(List<CityFeed> cityFeeds) {
                // Prone to failure: requires a formal SQSClient designed to handle Errors and failures
                var batch = from feed in cityFeeds
                            select new SendMessageBatchRequestEntry(Guid.NewGuid().ToString(), JsonSerializer.Serialize(feed));
                return _sqsClient.SendMessageBatchAsync(_sqsCityFeedUrl, batch.ToList());
            }
        */

    }
}
