using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleNotificationService;

using Latincoder.AirQuality.Model.Dynamo;
using Latincoder.AirQuality.Model.DTO;
using System.Text.Json;

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
        public string FunctionHandler(DynamoDBEvent dynamoDBEvent, ILambdaContext context)
        {
            snsClient = new AmazonSimpleNotificationServiceClient();

            foreach(var record in dynamoDBEvent.Records) {
                var dynamoDocument = Document.FromAttributeMap(record.Dynamodb.NewImage);
                CityFeed feed = CityFeedDocument.ToDTO(dynamoDocument);
                System.Console.WriteLine($"Accessed stream: {feed}");
                var msg = JsonSerializer.Serialize(feed);
                publishToTwitter(msg);

            }
        return "";
        }
        public void publishToTwitter(string message) {
            var arn = Environment.GetEnvironmentVariable("TWITTER_SNS_ARN");
            snsClient.PublishAsync(arn, message);
        }

        public void publishToGeneral(string message) {
            var arn = Environment.GetEnvironmentVariable("GENERAL_SNS_ARN");
            snsClient.PublishAsync(arn, message);
        }
    }
}
