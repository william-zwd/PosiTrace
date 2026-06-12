using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PosiTrace.Models;
using PosiTrace.Services;
using System.Text;

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
            context.Database.EnsureCreated();

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
                    StreetAddress newSA = null;

                    var geoCoding = await apiService.GetGeoCodingAsync(normal);
                    // normal address does not find geoCoding
                    if (geoCoding == "")
                    {
                        // wait 1 second
                        Thread.Sleep(1000);

                        // try to find postal code in address
                        var postalCode = StreetAddress.PostalCode(address);
                        // there is postal code exists in address
                        if (postalCode != "")
                        {
                            geoCoding = await apiService.GetGeoCodingAsync(postalCode);
                            if (geoCoding != "")
                            {
                                newSA = new StreetAddress()
                                {
                                    Address = normal,
                                    GeoCoding = "Using PostalCode " + postalCode + " " + geoCoding
                                };
                            }
                            else
                            {
                                newSA = new StreetAddress()
                                {
                                    Address = normal,
                                    GeoCoding = "Using PostalCode " + postalCode
                                };
                            }
                        }
                        // there is no postal code in address
                        else
                        {
                            newSA = new StreetAddress()
                            {
                                Address = normal,
                                GeoCoding = ""
                            };
                        }
                    }
                    // normal address finds geoCoding
                    else
                    {
                        newSA = new StreetAddress
                        {
                            Address = normal,
                            GeoCoding = geoCoding
                        };
                    }
                        
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
