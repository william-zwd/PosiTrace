using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PosiTrace
{
    public class NominatimService
    {
        private readonly HttpClient _httpClient;

        public NominatimService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Base address configuration
            _httpClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");

        }

        public async Task<string?> GetGeoCodingAsync(string normalizedAddress)
        {
            try
            {
                // Send the GET request
                string encoded = Uri.EscapeDataString(normalizedAddress);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")
                {
                    CharSet = Encoding.UTF8.WebName
                });
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PosiTrace/1.0 (william_zwd@outlook.com)");

                HttpResponseMessage response = await _httpClient.GetAsync($"/search?q={encoded}&format=geocodejson&polygon_kml=1&addressdetails=1");

                // Throws an exception if the status code is not 200-299
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response directly into the C# object
                GEOCode? geocode = await response.Content.ReadFromJsonAsync<GEOCode>();
                if (geocode is not null && geocode.features.Length > 0)
                {
                    var ret = new List<geocodingInner>();
                    foreach (var feature in geocode.features) 
                    {
                        ret.Add(feature.properties.geocoding);
                    }
                    return JsonSerializer.Serialize(ret);
                }
               
                return "";
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }
}
