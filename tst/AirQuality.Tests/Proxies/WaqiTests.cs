using Latincoder.AirQuality.Proxies;
using Latincoder.AirQuality.Model.External;
using Xunit;
using Xunit.Abstractions;
using Moq;

using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace Latincoder.AirQuality.Tests.Proxies
{
    public class WaqiTests
    {
        private HttpClient mockClient;
        private Waqi waqiProxy; // this should be a Mocked object
        private const string TestToken = "this_is_a_test_token";

        // Setup
        private readonly ITestOutputHelper output;

        
        public WaqiTests(ITestOutputHelper output)
        {   
            /*mockClient = Mock.Of<HttpClient>();
            Mock.Get(mockClient).Setup(it => it.GetStringAsync($"https://api.waqi.info/feed/Guadalajara/?token={TestToken}"))
            .Returns(new Task<string>(() => Values.Waqi.WaqiCityFeedResponseJSON));
            waqiProxy = Waqi.create(mockClient, TestToken);
            */
            waqiProxy = Waqi.create(TestToken);
            this.output = output;
        }

        [Fact(Skip="Need to re-design Proxy to accept interfaces so dependencies can be better implemented and make class testable")]
        public async void WaqiProxyTokenIsNotNull()
        {
            var result = await waqiProxy.getCityFeed("Guadalajara");   
            var city = JsonSerializer.Deserialize<WaqiCityFeed>(result);
            //output.WriteLine($"\n${result}\n=====");
            //Assert.True(city.Data.Aqi > 0, $"Aqi is not greater than 0 {city.Data}");
            Assert.True(result == null, $"this is a value {city.Status} \n {city.Data} \n Valor del AQUI:{city.Data.Aqi} \n atribucion {city.Data.Attributions[0].Name}");
        }
    }
}