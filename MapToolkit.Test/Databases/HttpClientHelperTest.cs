using System;
using System.Linq;
using System.Net.Http;
using Pmad.Cartography.Databases;

namespace Pmad.Cartography.Test.Databases
{
    public class HttpClientHelperTest
    {
        private const string BaseAddressString = "https://cdn.dem.pmad.net/SRTM1/";
        private static readonly Uri BaseAddressUri = new Uri(BaseAddressString);

        [Fact]
        public void CreateClient_WithUri_SetsBaseAddress()
        {
            using var client = HttpClientHelper.CreateClient(BaseAddressUri);

            Assert.Equal(BaseAddressUri, client.BaseAddress);
        }

        [Fact]
        public void CreateClient_WithString_SetsBaseAddress()
        {
            using var client = HttpClientHelper.CreateClient(BaseAddressString);

            Assert.Equal(BaseAddressUri, client.BaseAddress);
        }

        [Fact]
        public void CreateClient_WithUri_SetsUserAgentHeader()
        {
            using var client = HttpClientHelper.CreateClient(BaseAddressUri);

            var userAgentValues = client.DefaultRequestHeaders.UserAgent.ToList();
            Assert.Equal(2, userAgentValues.Count);
            Assert.Equal("Mozilla", userAgentValues[0].Product?.Name);
            Assert.Equal("5.0", userAgentValues[0].Product?.Version);
            Assert.Equal("(Pmad-Cartography; Default)", userAgentValues[1].Comment);
        }

        [Fact]
        public void CreateClient_WithString_SetsUserAgentHeader()
        {
            using var client = HttpClientHelper.CreateClient(BaseAddressString);

            var userAgentValues = client.DefaultRequestHeaders.UserAgent.ToList();
            Assert.Equal(2, userAgentValues.Count);
            Assert.Equal("Mozilla", userAgentValues[0].Product?.Name);
            Assert.Equal("5.0", userAgentValues[0].Product?.Version);
            Assert.Equal("(Pmad-Cartography; Default)", userAgentValues[1].Comment);
        }

        [Fact]
        public void CreateClient_ReturnsDifferentInstances()
        {
            using var client1 = HttpClientHelper.CreateClient(BaseAddressUri);
            using var client2 = HttpClientHelper.CreateClient(BaseAddressUri);

            Assert.NotSame(client1, client2);
        }

        [Fact]
        public void CreateClient_WithString_AndWithUri_ProduceSameBaseAddress()
        {
            using var clientFromString = HttpClientHelper.CreateClient(BaseAddressString);
            using var clientFromUri = HttpClientHelper.CreateClient(BaseAddressUri);

            Assert.Equal(clientFromString.BaseAddress, clientFromUri.BaseAddress);
        }
    }
}
