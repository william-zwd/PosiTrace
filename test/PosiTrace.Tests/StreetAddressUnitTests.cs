using Xunit;
using PosiTrace.Models;

namespace PosiTrace.Tests
{
    public class StreetAddressUnitTests
    {
        [Theory]
        [InlineData("123 Main St, Apt 4B, Toronto, ON", "123 Main St, Toronto, ON")]
        [InlineData("456 Oak Rd #12, Vancouver, BC", "456 Oak Rd, Vancouver, BC")]
        [InlineData("Unit 5, 789 Pine Ave, Montreal, QC", "789 Pine Ave, Montreal, QC")]
        [InlineData("  , Apt 10,   , 50 Elm St  ", "50 Elm St")]
        public void RemoveAUS_RemovesAptUnitSuiteAndHashes(string input, string expected)
        {
            var actual = StreetAddress.RemoveAUS(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123-12 Main St, Toronto", "123 Main St, Toronto")]
        [InlineData("Building 45-678, 10 King St", "Building 45, 10 King St")]
        public void RemoveAUS_RemovesHyphenNumberRanges(string input, string expected)
        {
            var actual = StreetAddress.RemoveAUS(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2000000 1000000B Avenue y1y 2z2", "Y1Y 2Z2")]
        [InlineData("Some address, y1y-2z2", "Y1Y-2Z2")]
        [InlineData("No postal here", "")]
        public void PostalCode_ExtractsCanadianPostalCodeOrReturnsEmpty(string input, string expected)
        {
            var actual = StreetAddress.PostalCode(input);
            Assert.Equal(expected, actual);
        }
    }
}