﻿using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    public class DemHttpStorage : IDemStorage
    {
        private readonly string localCache;
        private readonly HttpClient client;

        public DemHttpStorage (string? localCache, HttpClient client)
        {
            this.localCache = localCache ?? DefaultCacheLocation;
            this.client = client;
        }

        public DemHttpStorage(string? localCache, Uri baseAddress) 
            : this(localCache, new HttpClient() { BaseAddress = baseAddress })
        {

        }

        public DemHttpStorage(Uri baseAddress)
            : this(null, baseAddress)
        {

        }

        public static string DefaultCacheLocation => Path.Combine(Path.GetTempPath(), "dem");
        
        public async Task<IDemDataCell> Load(string path)
        {
            var uri = new Uri(client.BaseAddress!, path);
            var cacheFile = Path.Combine(localCache, uri.DnsSafeHost, uri.AbsolutePath.Substring(1).Replace('/', Path.DirectorySeparatorChar));
            if(!File.Exists(cacheFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
                // XXX: Limit cache size ?
                // XXX: Cache invalidation ?
                using (var input = await client.GetStreamAsync(path).ConfigureAwait(false))
                {
                    using (var cache = File.Create(cacheFile))
                    {
                        await input.CopyToAsync(cache).ConfigureAwait(false);
                    }
                }
            }
            return DemDataCell.Load(cacheFile);
        }

        public async Task<DemDatabaseIndex> ReadIndex()
        {
            using (var input = await client.GetStreamAsync("index.json").ConfigureAwait(false))
            {
                return (await JsonSerializer.DeserializeAsync<DemDatabaseIndex>(input, DemDatabaseIndexContext.Default.DemDatabaseIndex).ConfigureAwait(false))!;
            }
        }
    }
}
