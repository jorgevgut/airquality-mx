using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.S3;

using System;
using System.Linq;
using System.Collections.Generic;
using AirQualityCdk.Config;

namespace AirQualityCdk
{
    public class AirQualityCdkStack : Stack
    {
        /// <summary>
        /// Starting point: define stack in this method
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="id"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        internal AirQualityCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here
            // SNS Topics
            new Topic(this, "TwitterNotificationPublisher");
            new Topic(this, "EmailNotificationPublisher");

            // SQS queues
            AirQualitySQS("CityFeedQueue");


            // AWS lambdas
            // TODO: Lambda generation logic to be done using environment values. Config might also be pulled from S3
            // GetCityFeed lambda
            ConstructLambdas();

            // Event schedule
            new Rule(this, "Waqi5min", new RuleProps{
                Schedule = Schedule.Rate(Duration.Minutes(5))
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">Should be function root Namespace, also used for s3 key plus '.zip.</param>
        /// <param name="handler"></param>
        /// <returns></returns>
        internal Function CreateAirQualityLambda(string name, string handler, List<KeyValuePair<string, string>> envVars)
        {
            var properties = new FunctionProps();
            properties.Runtime = Runtime.DOTNET_CORE_3_1;
            // ensures unique Function names
            var functionName = $"{name}-{Guid.NewGuid().ToString()}";
            properties.FunctionName = functionName;

            // Add Environment variables
            properties.Environment = new Dictionary<string, string>();
            envVars.ForEach(properties.Environment.Add);

            // this is dirty but works, pick up from default configs now
            // relative path within project should be \src\AirQualityCdk\bin\Debug\netcoreapp3.1\AirQualityCdk.dll
            properties.Code = Code.FromBucket(
                Bucket.FromBucketName(this,
                                    $"code-{functionName}",
                                    Constants.DefaultLambdasBucketName), $"{name}.zip");
            // Look into picking up from asset in the future
            //Code.FromAsset(System.Environment.GetEnvironmentVariable("AirQualityhandlerPath"));
            properties.Handler = handler;
            properties.Role = LambdaAirRole(functionName); // pass in unique function name
            return new Function(this, name, properties);
        }

        internal Queue AirQualitySQS(string name) {
            var props = new QueueProps();
            props.Fifo = false; // use standard queue, is cheaper
            return new Queue(this, name, props);
        }

        internal Role LambdaAirRole(string name) {

            // TODO: create roles with required permissions
            var role = new Role(this, $"lambda-air-{name}", new RoleProps{
                RoleName = $"lambda-air-{name}",
                Description = @"THis is role is to be used by lambda functions within AirQuality project",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });

            return role;
        }
        internal void ConstructLambdas() {
            var lambdaOptions = new List<AirQualityLambdaOptions>();
            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultAirQualityFeedProcessorLambdaName,
                Handler = Constants.DefaultAirQualityFeedProcessorLambdaHandler
            });

            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultAirQualityTwitterPublisherLambdaName,
                Handler = Constants.DefaultAirQualityTwitterPublisherLambdaHandler
            });


            // config for Lambda to gather information from WAQI's API
            var cities = new List<City>();
            cities.Add(new City {
                Country = "mexico",
                Name = "guadalajara",
                Stations = new List<string>(new string[]{"vallarta", "tlaquepaque"})
                });

            var envVarsWaqiGetCityFeed = new List<KeyValuePair<string, string>>();
            envVarsWaqiGetCityFeed.Add(new KeyValuePair<string, string>(
                EnvCitiesWithStations.EnvironmentKey, EnvCitiesWithStations.GetValueFromCities(cities)));
            envVarsWaqiGetCityFeed.Add(new KeyValuePair<string, string>(Constants.EnvWaqiTokenKey, "replace_this_value"));

            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultWaqiGetCityFeedLambdaName,
                Handler = Constants.DefaultWaqiGetCityFeedLambdaHandler,
                EnvironmentVariables = envVarsWaqiGetCityFeed
            });

            // to reduce operational load lambdas within AirQuality project share roles and permissions
            // they should be managed by modifying Name, Handler and EnvVars.
            // TODO: Need to work on options for event actions that trigger these lambdas might change constructor
            lambdaOptions.ForEach(options => {
                CreateAirQualityLambda(options.Name, options.Handler, options.EnvironmentVariables);
            });
        }
    }
}
