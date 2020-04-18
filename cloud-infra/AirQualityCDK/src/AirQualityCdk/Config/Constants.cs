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

    }
}
