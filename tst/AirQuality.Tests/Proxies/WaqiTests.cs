using Latincoder.AirQuality.Proxies;
using Latincoder.AirQuality.Model;
using Xunit;
using Xunit.Abstractions;

using System.Text.Json;

namespace Latincoder.AirQuality.Tests.Proxies
{
    public class WaqiTests
    {
        private Waqi waqiProxy;
        private const string TestToken = "d1cafe10a8dadb506b3652d29201dd26ecee7a3f";

        // Setup
        private readonly ITestOutputHelper output;
        public WaqiTests(ITestOutputHelper output)
        {
            waqiProxy = Waqi.create(TestToken);
            this.output = output;
        }

        [Fact]
        public async void WaqiProxyTokenIsNotNull()
        {
            System.Console.WriteLine("== Requesting Gualajara Feed ==");
            var result = await waqiProxy.getCityFeed("Guadalajara");
            
            var city = JsonSerializer.Deserialize<WaqiCityFeed>(result);
            //output.WriteLine($"\n${result}\n=====");
            //Assert.True(city.Data.Aqi > 0, $"Aqi is not greater than 0 {city.Data}");
            Assert.True(result == null, $"this is a value {city.Status} \n {city.Data} \n Valor del AQUI:{city.Data.Aqi} \n atribucion {city.Data.Attributions[0].Name}");
        }
    }
}