using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
namespace Latincoder.AirQuality.Proxies
{
    /// <summary>
    /// This class is a client Proxy used to consume API from
    /// the World's Air Quality Index Project.
    /// See https://aqicn.org/api for more details 
    /// </summary>
    public class Waqi
    {
        // constants
        private const string ApiDomain = "https://api.waqi.info";
        private static readonly HttpClient client = new HttpClient();
        private string token;
        
        private Waqi(string token) {
            this.token = token;
        }

        public static Waqi create(string token) {
            return new Waqi(token);
        }

        public async Task<string> getCityFeed(string city) {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".Net - LatinCoder.AirQuality");
            var response = await client.GetStringAsync($"{ApiDomain}/feed/{city}/?token={token}");
            return response;
        }

    }
}