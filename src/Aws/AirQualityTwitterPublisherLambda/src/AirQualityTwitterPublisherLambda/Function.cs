using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace AirQualityTwitterPublisherLambda
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(System.IO.Stream inputStream, ILambdaContext context)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(inputStream);
            var receivedMessage = reader.ReadToEnd();
            System.Console.WriteLine("This is where twitter API integration will happen");
            System.Console.WriteLine(receivedMessage);
            var apiKey = Environment.GetEnvironmentVariable("TWITTER_API_KEY");
            var apiSecret = Environment.GetEnvironmentVariable("TWITTER_API_SECRET");
            var token = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            var tokenSecret = Environment.GetEnvironmentVariable("TWITTER_TOKEN_SECRET");
            var credentials = new TwitterCredentials(apiKey, apiSecret, token, tokenSecret);
            Auth.SetCredentials(credentials);

            try {
                var tweet = Auth.ExecuteOperationWithCredentials(credentials, () => Tweet.PublishTweet(receivedMessage));
                System.Console.WriteLine($"Published tweet by {tweet.TweetDTO.CreatedBy.Name}");
            } catch(Exception error) {
                // TODO: if tweet fails publish to DLQ (SQS)
                System.Console.WriteLine(error);
            }
        }
    }
}
