using System;
using System.Data;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pmad.Cartography.Databases
{
    public static class WellKnownDatabases
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0";

        internal static HttpClient CreateClient(string baseAddress)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            // OVH CDN Requires a user agent
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
            return httpClient;
        }

        /// <summary>
        /// Get the AW3D 30 data hosted on cdn.dem.pmad.net:
        /// Japan Aerospace Exploration Agency (2021). ALOS World 3D 30 meter DEM. V3.2, Jan 2021.
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// This dataset is available to use with no charge under the following conditions.
        /// When the user provide or publish the products and services to a third party using this dataset, it is necessary to display that the original data is provided by JAXA.
        /// You are kindly requested to show the copyright (© JAXA) and the source of data When you publish the fruits using this dataset.
        /// JAXA does not guarantee the quality and reliability of this dataset and JAXA assume no responsibility whatsoever for any direct or indirect damage and loss caused by use of this dataset.Also, JAXA will not be responsible for any damages of users due to changing, deleting or terminating the provision of this dataset.
        /// </remarks>
        public static DemHttpStorage GetAW3D30Storage(string? localCache = null)
        {
            return new DemHttpStorage(localCache, CreateClient("https://cdn.dem.pmad.net/AW3D30/"));
        }

        /// <summary>
        /// Get the SRTM 1 data hosted on cdn.dem.pmad.net:
        /// NASA Shuttle Radar Topography Mission (SRTM)(2013). Shuttle Radar Topography Mission (SRTM) Global.
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// Public domain.
        /// </remarks>
        public static DemHttpStorage GetSRTM1Storage(string? localCache = null)
        {
            return new DemHttpStorage(localCache, CreateClient("https://cdn.dem.pmad.net/SRTM1/"));
        }

        /// <summary>
        /// Get the SRTM 15+ data hosted on cdn.dem.pmad.net:
        /// GLOBAL BATHYMETRY AND TOPOGRAPHY AT 15 ARCSECONDS. SRTM15+V2.5.5 - March 20, 2023
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// Reference: Tozer, B. , D. T. Sandwell, W. H. F. Smith, C. Olson, J. R. Beale, and P. Wessel, Global bathymetry and topography at 15 arc seconds: SRTM15+, Accepted Earth and Space Science, August 3, 2019.
        /// Public domain.
        /// </remarks>
        public static DemHttpStorage GetSRTM15PlusStorage(string? localCache = null)
        {
            return new DemHttpStorage(localCache, CreateClient("https://cdn.dem.pmad.net/SRTM15Plus/"));
        }

        /// <summary>
        /// Get the AW3D 30 database hosted on cdn.dem.pmad.net:
        /// Japan Aerospace Exploration Agency (2021). ALOS World 3D 30 meter DEM. V3.2, Jan 2021.
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// This dataset is available to use with no charge under the following conditions.
        /// When the user provide or publish the products and services to a third party using this dataset, it is necessary to display that the original data is provided by JAXA.
        /// You are kindly requested to show the copyright (© JAXA) and the source of data When you publish the fruits using this dataset.
        /// JAXA does not guarantee the quality and reliability of this dataset and JAXA assume no responsibility whatsoever for any direct or indirect damage and loss caused by use of this dataset.Also, JAXA will not be responsible for any damages of users due to changing, deleting or terminating the provision of this dataset.
        /// </remarks>
        public static DemDatabase GetAW3D30(string? localCache = null)
        {
            return new DemDatabase(GetAW3D30Storage(localCache));
        }

        /// <summary>
        /// Get the SRTM 1 database hosted on cdn.dem.pmad.net:
        /// NASA Shuttle Radar Topography Mission (SRTM)(2013). Shuttle Radar Topography Mission (SRTM) Global.
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// Public domain.
        /// </remarks>
        public static DemDatabase GetSRTM1(string? localCache = null)
        {
            return new DemDatabase(GetSRTM1Storage(localCache));
        }

        /// <summary>
        /// Get the SRTM 15+ database hosted on cdn.dem.pmad.net:
        /// GLOBAL BATHYMETRY AND TOPOGRAPHY AT 15 ARCSECONDS. SRTM15+V2.5.5 - March 20, 2023
        /// </summary>
        /// <param name="localCache">Cache location. If null, will use <see cref="DemHttpStorage.DefaultCacheLocation"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// Reference: Tozer, B. , D. T. Sandwell, W. H. F. Smith, C. Olson, J. R. Beale, and P. Wessel, Global bathymetry and topography at 15 arc seconds: SRTM15+, Accepted Earth and Space Science, August 3, 2019.
        /// Public domain.
        /// </remarks>
        public static DemDatabase GetSRTM15Plus(string? localCache = null)
        {
            return new DemDatabase(GetSRTM15PlusStorage(localCache));
        }
    }
}
