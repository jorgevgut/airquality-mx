#!/usr/bin/bash.exe
# package all lambdas and uploads to s3, requires your own configuration
dotnet lambda package -- -pl AirQualityTwitterPublisherLambda/src/AirQualityTwitterPublisherLambda/

dotnet lambda package -- -pl AirQualityFeedProcessorLambda/src/AirQualityFeedProcessorLambda/

dotnet lambda package -- -pl WaqiGetCityFeedLambda/src/WaqiGetCityFeedLambda/

S3Bucket="s3://dotnet-apps"
WaqiZip="WaqiGetCityFeedLambda/src/WaqiGetCityFeedLambda/bin/Release/netcoreapp3.1/WaqiGetCityFeedLambda.zip"
FeedProcessorZip="AirQualityFeedProcessorLambda/src/AirQualityFeedProcessorLambda/bin/Release/netcoreapp3.1/AirQualityFeedProcessorLambda.zip"
TwitterPublisherZip="AirQualityTwitterPublisherLambda/src/AirQualityTwitterPublisherLambda/bin/Release/netcoreapp3.1/AirQualityTwitterPublisherLambda.zip"

aws s3 cp $WaqiZip $S3Bucket
aws s3 cp $FeedProcessorZip $S3Bucket
aws s3 cp $TwitterPublisherZip $S3Bucket
