using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using Latincoder.AirQuality.Model.Dynamo;
using Latincoder.AirQuality.Model.DTO;
using Latincoder.AirQuality.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace AirQualityFeedProcessorLambda
{
    public class Function
    {
        private AmazonSimpleNotificationServiceClient snsClient;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(DynamoDBEvent dynamoDBEvent, ILambdaContext context)
        {
            snsClient = new AmazonSimpleNotificationServiceClient();

            foreach(var record in dynamoDBEvent.Records) {
                var dynamoDocument = Document.FromAttributeMap(record.Dynamodb.NewImage);
                CityFeed feed = CityFeedDocument.ToDTO(dynamoDocument);
                if (feed == null) {
                    System.Console.WriteLine("Streamed had no new data");
                    continue;
                    }
                System.Console.WriteLine($"Feed Object:{feed}");
                System.Console.WriteLine($"Accessed stream: {feed.CityName}");
                var msg = NotificationFormatter.GetTwitterMessageSpanish(feed);
                await publishToGeneral(NotificationFormatter.GetSimpleMessage(feed));
                await publishToTwitter(msg);

            }
        return "";
        }
        public Task<PublishResponse> publishToTwitter(string message) {
            var arn = Environment.GetEnvironmentVariable("TWITTER_SNS_ARN");
            return snsClient.PublishAsync(arn, message);
        }

        public Task<PublishResponse> publishToGeneral(string message) {
            var arn = Environment.GetEnvironmentVariable("GENERAL_SNS_ARN");
            return snsClient.PublishAsync(arn, message);
        }
    }
}
