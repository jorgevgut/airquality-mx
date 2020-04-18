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
        public static readonly HttpClient defaultClient = new HttpClient();
        private string token;
        private HttpClient _client;
        private Waqi(string token) {
            this.token = token;
            _client = Waqi.defaultClient;
        }

        private Waqi(HttpClient client, string token) {
            this.token = token;
            _client = client;
        }

        public static Waqi create(string token) {
            return new Waqi(token);
        }

        public static Waqi create(HttpClient client, string token) {
            return new Waqi(client, token);
        }

        public async Task<string> getCityFeed(string city) {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", ".Net - LatinCoder.AirQuality");
            var response = await _client.GetStringAsync($"{ApiDomain}/feed/{city}/?token={token}");
            return response;
        }

    }
}
