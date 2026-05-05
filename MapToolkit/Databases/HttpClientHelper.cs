using System;
using System.Net.Http;

namespace Pmad.Cartography.Databases
{
    internal static class HttpClientHelper
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Pmad-Cartography; Default)";

        internal static HttpClient CreateClient(string baseAddress)
        {
            return CreateClient(new Uri(baseAddress));
        }

        internal static HttpClient CreateClient(Uri baseAddress)
        {
            var httpClient = new HttpClient { BaseAddress = baseAddress };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
            return httpClient;
        }
    }
}
