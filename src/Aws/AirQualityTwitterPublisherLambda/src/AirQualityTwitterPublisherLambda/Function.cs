using System;
using System.Linq;
using Amazon.Lambda.SNSEvents;
using Tweetinvi;
using Tweetinvi.Models;

using Amazon.Lambda.Core;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace AirQualityTwitterPublisherLambda
{
    public class Function
    {

        private bool hasMessage(SNSEvent.SNSRecord snsRecord) {
            var msg = string.Empty;
            if (snsRecord == null) return false;
            try {
                msg = snsRecord.Sns.Message;
            } catch(NullReferenceException e) {
                return false;
            }
            return !string.IsNullOrEmpty(msg);
        }

        public ITwitterCredentials GetCredentials() {

            var apiKey = Environment.GetEnvironmentVariable("TWITTER_APIK");
            var apiSecret = Environment.GetEnvironmentVariable("TWITTER_APIS");
            var token = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            var tokenSecret = Environment.GetEnvironmentVariable("TWITTER_SECRET_TOKEN");
            var credentials = new TwitterCredentials(apiKey, apiSecret, token, tokenSecret);

            return credentials;
        }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public System.Threading.Tasks.Task<string> FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
        {
            var credentials = GetCredentials();

            //Auth.SetUserCredentials(apiKey, apiSecret, token, tokenSecret);
            // TweetInvi lacks returning asynchronous types that can be awaited
            var publisher = User.GetAuthenticatedUser(credentials);

            var t = new System.Threading.Tasks.Task<string>(() => {
                System.Console.WriteLine($"Authenticated User {publisher.Name}");

                var records = from r in snsEvent.Records
                                where hasMessage(r)
                                select r;
                foreach(var snsRecord in records) {
                    var receivedMessage = snsRecord.Sns.Message;
                    System.Console.WriteLine("This is where twitter API integration will happen");
                    System.Console.WriteLine(receivedMessage);

                    try {
                        System.Console.WriteLine($"Start to publish Tweet.{receivedMessage}");
                        publisher.PublishTweet(receivedMessage);
                        System.Threading.Thread.Sleep(5000);
                        System.Console.WriteLine($"Published tweet: {publisher.Name}");
                    } catch(Exception error) {
                        // TODO: if tweet fails publish to DLQ (SQS)
                        System.Console.WriteLine($"Error detected:{error}");
                    }
                }
                return "{\"msg\":\"execution completed\"}";
            });
            t.RunSynchronously();
            return t;
        }
    }
}
