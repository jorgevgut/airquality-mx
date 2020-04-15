using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Latincoder.AirQuality.Model.DTO;
using Latincoder.AirQuality.Model.External;
using Latincoder.AirQuality.Proxies;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace WaqiGetCityFeedLambda
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(string input, ILambdaContext context)
        {
            await AsyncExecute(input, context);
        }
    
        public async Task<string> AsyncExecute(string input, ILambdaContext context) {

                var waqiProxy = Waqi.create("sometoken"); // add token
                var feeds = new List<string>();
                var stationFeedsObtained = new List<WaqiCityFeed>();

                feeds.AddRange(new string[]{@"mexico/guadalajara/tlaquepaque"
                , @"mexico/guadalajara/vallarta"
                , @"mexico/guadalajara/miravalle"
                });
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
                var cityFeedDto = CityFeed.From(stationFeedsObtained);

                return JsonSerializer.Serialize(cityFeedDto).ToString();
                //System.Console.WriteLine(JsonSerializer.Serialize(cityFeedDto).ToString());
        }

    }
}
