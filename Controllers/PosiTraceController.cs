using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PosiTrace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PosiTraceController : ControllerBase
    {
        private readonly IDbContextFactory<AppDBContext> _factory;

        private static string[] Addresses = new[]
        {
            "514-12677 110A Avenue", "5172 Kingsway, Unit 250, Burnaby", "5172 Kingsway Unit 250, Burnaby"
        };

        private readonly ILogger<PosiTraceController> _logger;

        public PosiTraceController(ILogger<PosiTraceController> logger,
            IDbContextFactory<AppDBContext> factory)
        {
            _logger = logger;
            _factory = factory;
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
            var apiService = new NominatimService(httpClient);

            // cache db
            var context = await _factory.CreateDbContextAsync();

            // 
            var ret = new List<StreetAddress>();
            foreach (var address in request)
            {
                var normal = StreetAddress.RemoveAUS(address);
                var sa = context.StreetAddresses.FirstOrDefault(s => s.Address == normal);
                if (sa != null)
                {
                    ret.Add(sa);
                }
                else
                {
                    var newSA = new StreetAddress
                    {
                        Address = normal,
                        GeoCoding = await apiService.GetGeoCodingAsync(normal)
                    };
                    ret.Add(newSA);

                    // add to cache db
                    context.StreetAddresses.Add(newSA);
                    context.SaveChanges();

                    // wait 1 second
                    Thread.Sleep(1000);
                }
            }

            // Returns a HTTP 201 Created status
            return Ok(ret.ToArray());
        }

    }
}
