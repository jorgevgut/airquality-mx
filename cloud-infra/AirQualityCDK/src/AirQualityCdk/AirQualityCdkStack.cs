using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.S3;

namespace AirQualityCdk
{
    public class AirQualityCdkStack : Stack
    {
        internal Role LambdaAirRole(string name) {

            // TODO: create roles with required permissions
            var role = new Role(this, $"lambda-air-{name}", new RoleProps{
                RoleName = $"lambda-air-{name}",
                Description = @"THis is role is to be used by lambda functions within AirQuality project",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });
            
            return role;
        }

        internal Function AirQualityNetLambda(string name, string handler = "AirQuality.Aws.Lambdas.WaqiGetCity") {
            var properties = new FunctionProps();
            properties.Runtime = Runtime.DOTNET_CORE_3_1;
            properties.FunctionName = name;
            // this is dirty but works, pick up frm environment frm now
            // relative path within project should be \src\AirQualityCdk\bin\Debug\netcoreapp3.1\AirQualityCdk.dll
            properties.Code = Code.FromBucket(Bucket.FromBucketArn(this, $"code-{name}", "arn:aws:s3:us-west-2:811709609797:dotnet-apps"), @"lambdas/AirQualityCdk.zip");
            // Look into picking up from asset in the future
            //Code.FromAsset(System.Environment.GetEnvironmentVariable("AirQualityhandlerPath"));
            properties.Handler = handler;
            properties.Role = LambdaAirRole(name);
            return new Function(this, name, properties);
        }

        internal Queue AirQualitySQS(string name) {
            var props = new QueueProps();
            props.Fifo = false; // use standard queue, is cheaper
            return new Queue(this, name, props);
        }
        internal AirQualityCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here
            // SNS Topics
            new Topic(this, "TwitterNotificationPublisher");
            new Topic(this, "EmailNotificationPublisher");

            // SQS queues
            AirQualitySQS("CityFeedQueue");


            // AWS lambdas
            // GetCityFeed lambda
            AirQualityNetLambda("WaqiGetCityFeed");
            AirQualityNetLambda("FeedProcessor");

            // Event schedule        
            new Rule(this, "Waqi5min", new RuleProps{
                Schedule = Schedule.Rate(Duration.Minutes(5))
            });
        }
    }
}
