using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SNSEvents;

using AirQualityTwitterPublisherLambda;

namespace AirQualityTwitterPublisherLambda.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestEnvironmentVars() {
            var apiKey = Environment.GetEnvironmentVariable("TWITTER_APIK");
            var apiSecret = Environment.GetEnvironmentVariable("TWITTER_APIS");
            var pin = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            Assert.False(string.IsNullOrEmpty(apiKey));
            Assert.False(string.IsNullOrEmpty(apiSecret));
            Assert.False(string.IsNullOrEmpty(pin));
        }

        [Fact]
        public void TestGetCredentials()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var credentials = function.GetCredentials();
            Assert.True(credentials != null);
        }
    }
}
