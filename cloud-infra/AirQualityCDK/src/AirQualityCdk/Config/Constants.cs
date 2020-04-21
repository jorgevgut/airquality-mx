namespace AirQualityCdk.Config
{
    public static class Constants
    {

        /* Default values for AirQuality lambda templates */
        public readonly static string DefaultLambdasBucketName = "dotnet-apps";
        public readonly static string DefaultAirQualityFeedProcessorLambdaName = "AirQualityFeedProcessorLambda";

        public readonly static string DefaultAirQualityFeedProcessorLambdaHandler = $"{DefaultAirQualityFeedProcessorLambdaName}::{DefaultAirQualityFeedProcessorLambdaName}.Function::FunctionHandler";
        public readonly static string DefaultAirQualityTwitterPublisherLambdaName = "AirQualityTwitterPublisherLambda";
        public readonly static string DefaultAirQualityTwitterPublisherLambdaHandler = $"{DefaultAirQualityTwitterPublisherLambdaName}::{DefaultAirQualityTwitterPublisherLambdaName}.Function::FunctionHandler";
        public readonly static string DefaultWaqiGetCityFeedLambdaName = "WaqiGetCityFeedLambda";
        public readonly static string DefaultWaqiGetCityFeedLambdaHandler = $"{DefaultWaqiGetCityFeedLambdaName}::{DefaultWaqiGetCityFeedLambdaName}.Function::FunctionHandler";

        /* Environment variables used by AirQuality Lambdas */
        public readonly static string EnvCitiesWaqiGetCityFeedLambda = "CITIES";
        public readonly static string EnvWaqiTokenKey = "TOKEN";
        public readonly static string EnvCityFeedSqsUrl = "CITYFEED_SQS_URL";
        public readonly static string EnvTwitterSNS = "TWITTER_SNS_ARN";
        public readonly static string EnvTwitterAPIKey = "TWITTER_APIK";
        public readonly static string EnvTwitterAPISecret = "TWITTER_APIS";
        public readonly static string EnvTwitterAccessToken = "TWITTER_ACCESS_TOKEN";
        public readonly static string EnvTwitterTokenSecret = "TWITTER_SECRET_TOKEN";
        public readonly static string EnvAirQualityTable = "TABLE_NAME";
        public readonly static string EnvGeneralSNSTopic = "GENERAL_SNS_ARN";

        /* Values used for DynamoDB */
        public readonly static string DbAirQualityTableName = "airquality-mx-table";
        public readonly static string DbPartitionKeyName = "country-cityName"; // only one record associated with coutry/city used for Dynamo's internal hash

    }
}
