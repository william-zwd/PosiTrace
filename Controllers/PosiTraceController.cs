using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PosiTrace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PosiTraceController : ControllerBase
    {
        private static string[] Addresses = new[]
        {
            "514-12677 110A Avenue", "5172 Kingsway, Unit 250, Burnaby", "5172 Kingsway Unit 250, Burnaby"
        };

        private readonly ILogger<PosiTraceController> _logger;

        public PosiTraceController(ILogger<PosiTraceController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<string> request)
        {
            // Simple validation
            if (request is null || request.Count == 0)
            {
                return BadRequest("No address.");
            }

            var httpClient = new HttpClient();
            var apiService = new APIService(httpClient);

            // 
            var ret = new List<StreetAddress>();
            foreach (var address in request)
            {
                var normal = StreetAddress.Normalize(address);
                ret.Add(new StreetAddress
                {
                    NormalizedAddress = normal,
                    GeoCoding = await apiService.GetGeoCodingAsync(normal)
                });

                // wait 1 second
                Thread.Sleep(1000);
            }

            // Returns a HTTP 201 Created status
            return Ok(ret.ToArray());
        }

    }
}
