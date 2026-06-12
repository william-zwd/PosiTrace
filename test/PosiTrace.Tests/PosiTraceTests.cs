using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using PosiTrace.Models;
using PosiTrace.Services;
using System.Net;
using System.Net.Http.Json;

namespace PosiTrace.Tests
{
    public class PosiTraceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly IDbContextFactory<AppDBContext> _context;

        public PosiTraceTests(WebApplicationFactory<Program> factory)
        {
            // Creates an in-memory HttpClient connected directly to your real API routing
            _client = factory.CreateClient();
            _context = factory.Services.GetRequiredService<IDbContextFactory<AppDBContext>>();
        }

        [Fact]
        public async Task AddressNoPostal_WithNoCache_WithNoGeoCoding()
        {
            List<string> _address = new List<string>();
            _address.Add("2000000 1000000B Avenue");
            var normal = StreetAddress.RemoveAUS(_address[0]);

            using var context = _context.CreateDbContext();
            context.StreetAddresses.Where(s => s.Address == _address[0]).ExecuteDeleteAsync();
            context.SaveChanges();
            context.Dispose();

            var response = await _client.PostAsJsonAsync("/positrace", _address);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var sa = await response.Content.ReadFromJsonAsync<StreetAddress>();
            Assert.NotNull(sa);
            Assert.True(sa.Address == normal);
            Assert.True(string.IsNullOrEmpty(sa.GeoCoding));
        }

        [Fact]
        public async Task AddressWithPostal_WithNoCache_WithNoGeoCoding()
        {
            

            List<string> _address = new List<string>();
            _address.Add("2000000 1000000B Avenue y1y 2z2");
            var normal = StreetAddress.RemoveAUS(_address[0]);

            using var context = await _context.CreateDbContextAsync();
            context.StreetAddresses.Where(s => s.Address == normal).ExecuteDeleteAsync();
            context.SaveChanges();
            context.Dispose();

            var response = await _client.PostAsJsonAsync("/positrace", _address);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<StreetAddress> rr = await response.Content.ReadFromJsonAsync<List<StreetAddress>>();
            var sa = rr.ToArray();
            Assert.NotNull(sa);
            Assert.True(sa[0].Address == normal);
            Assert.True(sa[0].GeoCoding.Contains("Using PostalCode") && sa[0].GeoCoding.Contains("Y1Y 2Z2"));
        }

    }
}
