using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Pmad.Cartography.DataCells;
using System;

namespace Pmad.Cartography.Databases
{
    internal class DemDatabaseEntry
    {
        internal DemDatabaseEntry(string path, IDemDataCellMetadata metadata)
        {
            Path = path;
            Metadata = metadata;
        }

        public string Path { get; }

        public IDemDataCellMetadata Metadata { get; }

        public bool Contains(Coordinates coordinates)
        {
            return Metadata.Start.Latitude <= coordinates.Latitude &&
                    Metadata.End.Latitude >= coordinates.Latitude &&
                    Metadata.Start.Longitude <= coordinates.Longitude &&
                    Metadata.End.Longitude >= coordinates.Longitude;
        }

        internal bool Overlaps(Coordinates start, Coordinates end)
        {
            return Metadata.Start.Latitude <= end.Latitude &&
                    Metadata.Start.Longitude <= end.Longitude &&
                    Metadata.End.Latitude >= start.Latitude &&
                    Metadata.End.Longitude >= start.Longitude;
        }

        public async Task<IDemDataCell> Load(IDemStorage storage, IMemoryCache cache)
        {
            if (!cache.TryGetValue(this, out IDemDataCell? result) || result == null)
            {
                using var entry = cache.CreateEntry(this);
                result = await storage.Load(Path).ConfigureAwait(false);
                entry.Value = result;
                entry.Size = result.SizeInBytes;
            }
            return result;
        }

        public IDemDataCell? PickData(IMemoryCache memoryCache)
        {
            return memoryCache.Get<IDemDataCell>(this);
        }

        public void UnLoad(IMemoryCache memoryCache)
        {
            memoryCache.Remove(this);
        }

        internal double GetCoverageSurface(Coordinates start, Coordinates end)
        {
            return Math.Max(0,Math.Min(end.Latitude, Metadata.End.Latitude) - Math.Max(start.Latitude, Metadata.Start.Latitude)) *
                Math.Max(0, Math.Min(end.Longitude, Metadata.End.Longitude) - Math.Max(start.Longitude, Metadata.Start.Longitude));
        }
    }
}
