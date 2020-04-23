using Amazon.CDK;
using Dynamo  = Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.S3;

using System;
using System.Text.Json;
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
            // Metadata to be used by internal construct methods - might need a bit of refactoring and better design
            // this holds keyvaluePairs which can be used to configure environmentVariables
            var metadata = new Dictionary<string, KeyValuePair<string, string>>();
            // The code that defines your stack goes here
            // SNS Topics
            new Topic(this, "EmailNotificationPublisher");

            // SQS queues
            var cityFeedSqs = AirQualitySQS("CityFeedQueue");
            // SNS Topics
            var twitterPubSNS = new Topic(this, $"twitterPub-{Guid.NewGuid().ToString().Substring(0,10)}");
            var generalPubSNS = new Topic(this, $"generalPub-{Guid.NewGuid().ToString().Substring(0,10)}");

            // Dynamo DB table
            var tableProps = new Dynamo.TableProps();
            tableProps.TableName = Constants.DbAirQualityTableName;
            var primaryKey = new Dynamo.Attribute{Name = Constants.DbPartitionKeyName, Type = Dynamo.AttributeType.STRING };
            tableProps.BillingMode = Dynamo.BillingMode.PAY_PER_REQUEST; // on-demand pricing
            tableProps.PartitionKey = primaryKey;
            tableProps.Stream = Dynamo.StreamViewType.NEW_IMAGE;
            var airqualityTable = new Dynamo.Table(this, Constants.DbAirQualityTableName, tableProps);

            //Environment value metadata for lambda creation
            metadata[Constants.EnvCityFeedSqsUrl] = KeyValuePair.Create(Constants.EnvCityFeedSqsUrl, cityFeedSqs.QueueUrl);
            metadata[Constants.EnvTwitterSNS] = KeyValuePair.Create(Constants.EnvTwitterSNS, twitterPubSNS.TopicArn);
            metadata[Constants.EnvTwitterAPIKey] = KeyValuePair.Create(Constants.EnvTwitterAPIKey, System.Environment.GetEnvironmentVariable(Constants.EnvTwitterAPIKey));
            metadata[Constants.EnvTwitterAPISecret] = KeyValuePair.Create(Constants.EnvTwitterAPISecret, System.Environment.GetEnvironmentVariable(Constants.EnvTwitterAPISecret));
            metadata[Constants.EnvTwitterAccessToken] = KeyValuePair.Create(Constants.EnvTwitterAccessToken, System.Environment.GetEnvironmentVariable(Constants.EnvTwitterAccessToken));
            metadata[Constants.EnvTwitterTokenSecret] = KeyValuePair.Create(Constants.EnvTwitterTokenSecret, System.Environment.GetEnvironmentVariable(Constants.EnvTwitterTokenSecret));
            metadata[Constants.EnvAirQualityTable] = KeyValuePair.Create(Constants.EnvAirQualityTable, airqualityTable.TableName);
            metadata[Constants.EnvGeneralSNSTopic] = KeyValuePair.Create(Constants.EnvGeneralSNSTopic, generalPubSNS.TopicArn);
            // AWS lambdas
            // TODO: Lambda generation logic to be done using environment values. Config might also be pulled from S3
            // GetCityFeed lambda
            var functionsByName = ConstructLambdas(metadata);

            // subscribe and configure
            // find twitter publisher lambda

            twitterPubSNS.AddSubscription(new LambdaSubscription(functionsByName
                    .Where(pair => pair.Key.Contains(Constants.DefaultAirQualityTwitterPublisherLambdaName))
                    .Single().Value));

            // trigger feed processor lambda only on dynamo db stream change by coutry-city feed
            functionsByName
                    .Where(pair => pair.Key.Contains(Constants.DefaultAirQualityFeedProcessorLambdaName))
                    .Single().Value.AddEventSource(new DynamoEventSource(airqualityTable,new DynamoEventSourceProps()));

            // Event schedule every 15 mins - // architecture targets 5, change when tested and working
            var waqiLambda = functionsByName
                    .Where(pair => pair.Key.Contains(Constants.DefaultWaqiGetCityFeedLambdaName))
                    .Single().Value;
            var targetWaqiLambda =  new LambdaFunction(waqiLambda);
            new Rule(this, "Waqi5min", new RuleProps{
                Schedule = Schedule.Rate(Duration.Minutes(5))
            }).AddTarget(targetWaqiLambda);
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
            properties.Timeout = Duration.Seconds(20); // ensures max 20 sec duration
            // ensures unique Function names - use substring because of 64 character name limit
            var functionName = $"{name}-{Guid.NewGuid().ToString().Substring(0,12)}";
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
            properties.Role = LambdaAirRole(); // pass in unique function name singleton
            return new Function(this, name, properties);
        }

        internal Queue AirQualitySQS(string name) {
            var props = new QueueProps();
            props.Fifo = false; // use standard queue, is cheaper
            return new Queue(this, name, props);
        }

        private Role _role = null;
        internal  Role LambdaAirRole() {
            if (_role != null) { return _role; }
            // TODO: create roles with required permissions
            _role = new Role(this, $"lrole-{Guid.NewGuid().ToString()}", new RoleProps{
                RoleName = $"lrole-{Guid.NewGuid().ToString()}",
                Description = @"THis is role is to be used by lambda functions within AirQuality project",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });
            // Adding permissions
            _role.AddToPolicy(SQSFullAccess);
            _role.AddToPolicy(SNSFullAccess);
            _role.AddToPolicy(LambdaFullAccess);
            _role.AddToPolicy(DynamoFullAccess);
            _role.AddToPolicy(CloudWatchFullAccess);
            return _role;
        }

        /*
            Useful policy statements
            TODO: code more restrictive policies
        */

        internal PolicyStatement LambdaFullAccess {
            get {
            var policyProps = new PolicyStatementProps();
            policyProps.Actions = new string[]{"lambda:*"};
            policyProps.Effect = Effect.ALLOW;
            policyProps.Resources = new string[]{"*"};
            return new PolicyStatement(policyProps);
            }
        }

        /// <summary>
        /// Access to cloudwatch actions, creation of loggroups and log publishing
        /// </summary>
        /// <value></value>
        internal PolicyStatement CloudWatchFullAccess {
            get {
            var policyProps = new PolicyStatementProps();
            policyProps.Actions = new string[]{"cloudwatch:*", "logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents"};
            policyProps.Effect = Effect.ALLOW;
            policyProps.Resources = new string[]{"*"};
            return new PolicyStatement(policyProps);
            }
        }

        internal PolicyStatement DynamoFullAccess {
            get {
            var policyProps = new PolicyStatementProps();
            policyProps.Actions = new string[]{"dynamodb:*"};
            policyProps.Effect = Effect.ALLOW;
            policyProps.Resources = new string[]{"*"};
            return new PolicyStatement(policyProps);
            }
        }

         internal PolicyStatement SNSFullAccess {
            get {
            var policyProps = new PolicyStatementProps();
            policyProps.Actions = new string[]{"SNS:*"};
            policyProps.Effect = Effect.ALLOW;
            policyProps.Resources = new string[]{"*"};
            return new PolicyStatement(policyProps);
            }
        }

                 internal PolicyStatement SQSFullAccess {
            get {
            var policyProps = new PolicyStatementProps();
            policyProps.Actions = new string[]{"SQS:*"};
            policyProps.Effect = Effect.ALLOW;
            policyProps.Resources = new string[]{"*"};
            return new PolicyStatement(policyProps);
            }
        }

        internal List<KeyValuePair<string, Function>> ConstructLambdas(IDictionary<string, KeyValuePair<string, string>> metadata) {
            var lambdaOptions = new List<AirQualityLambdaOptions>();
            var envVarsFeedProcessor = new List<KeyValuePair<string, string>>();
            envVarsFeedProcessor.Add(metadata[Constants.EnvCityFeedSqsUrl]);
            envVarsFeedProcessor.Add(metadata[Constants.EnvTwitterSNS]);
            envVarsFeedProcessor.Add(metadata[Constants.EnvAirQualityTable]);
            envVarsFeedProcessor.Add(metadata[Constants.EnvGeneralSNSTopic]);
            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultAirQualityFeedProcessorLambdaName,
                Handler = Constants.DefaultAirQualityFeedProcessorLambdaHandler,
                EnvironmentVariables = envVarsFeedProcessor
            });

            var envVarsTwitterPublisher = new List<KeyValuePair<string, string>>();
            envVarsTwitterPublisher.Add(metadata[Constants.EnvTwitterSNS]);
            envVarsTwitterPublisher.Add(metadata[Constants.EnvTwitterAPIKey]);
            envVarsTwitterPublisher.Add(metadata[Constants.EnvTwitterAPISecret]);
            envVarsTwitterPublisher.Add(metadata[Constants.EnvTwitterAccessToken]);
            envVarsTwitterPublisher.Add(metadata[Constants.EnvTwitterTokenSecret]);
            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultAirQualityTwitterPublisherLambdaName,
                Handler = Constants.DefaultAirQualityTwitterPublisherLambdaHandler,
                EnvironmentVariables = envVarsTwitterPublisher
            });


            // config for Lambda to gather information from WAQI's API
            var cities = new List<City>();
            // jsons where obtained manually using a Postman script
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"nuevo-leon\",\"Stations\":[\"san-nicolas\",\"s.-pedro\",\"obispado\",\"monterrey/universidad\",\"monterrey/cadereyta\",\"monterrey/pueblo-serena\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"mexico\",\"Stations\":[\"miguel-hidalgo\",\"gustavo-a.-madero\",\"uam-iztapalapa\",\"benito-juarez\",\"merced\",\"fes-aragon\",\"iztacalco\",\"hospital-general-de-mexico\",\"coyoacan\",\"san-juan-de-aragon\",\"camarones\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"mexico\",\"Stations\":[\"pedregal\",\"ajusco-medio\",\"santiago-acahualtepec\",\"tlalnepantla\",\"tlahuac\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"veracruz\",\"Stations\":[\"xalapa/stps\",\"minatitlan/tecnologico\",\"poza-rica/universidad-veracruzana\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"veracruz\",\"Stations\":[\"poza-rica/universidad-veracruzana\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"baja-california\",\"Stations\":[\"tijuana/laboratorio\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"guanajuato\",\"Stations\":[\"guanajuato/universidad-gto-sede-belen\",\"ciceg\",\"irapuato/bomberos\",\"t-21\",\"facultad-de-medicina\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"yucatan\",\"Stations\":[\"merida/sds01\",\"merida/seduma01\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"oaxaca\",\"Stations\":[\"casa-hogar\",\"cedart\"]}"));
            cities.Add(JsonSerializer.Deserialize<City>("{\"Country\":\"mexico\",\"Name\":\"guadalajara\",\"Stations\":[\"tlaquepaque\",\"miravalle\",\"vallarta\",\"las-pintas\",\"loma-dorada\"]}"));
            // manually added city
            cities.Add(new City {
                Country = "mexico",
                Name = "jalisco", // guadalajara is also good
                Stations = new List<string>(new string[]{"oblatos", "aguilas"})
                });

            var envVarsWaqiGetCityFeed = new List<KeyValuePair<string, string>>();
            envVarsWaqiGetCityFeed.Add(new KeyValuePair<string, string>(
                EnvCitiesWithStations.EnvironmentKey, EnvCitiesWithStations.GetValueFromCities(cities)));
            envVarsWaqiGetCityFeed.Add(new KeyValuePair<string, string>(Constants.EnvWaqiTokenKey, System.Environment.GetEnvironmentVariable(Constants.EnvWaqiTokenKey)));
            // add queueUrl as an environment variable
            envVarsWaqiGetCityFeed.Add(metadata[Constants.EnvCityFeedSqsUrl]);
            envVarsWaqiGetCityFeed.Add(metadata[Constants.EnvAirQualityTable]);

            lambdaOptions.Add(new AirQualityLambdaOptions{
                Name = Constants.DefaultWaqiGetCityFeedLambdaName,
                Handler = Constants.DefaultWaqiGetCityFeedLambdaHandler,
                EnvironmentVariables = envVarsWaqiGetCityFeed
            });

            // to reduce operational load lambdas within AirQuality project share roles and permissions
            // they should be managed by modifying Name, Handler and EnvVars.
            // TODO: Need to work on options for event actions that trigger these lambdas might change constructor
            var functionByName = from lambda in lambdaOptions
                    let function = CreateAirQualityLambda(lambda.Name, lambda.Handler, lambda.EnvironmentVariables)
                    group function by lambda.Name;
            return (from f in functionByName
             select KeyValuePair.Create<string, Function>(f.Key, f.FirstOrDefault())).ToList();
        }
    }
}
