using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MapToolkit.DataCells;

namespace MapToolkit.Databases
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
            if (!cache.TryGetValue(this, out IDemDataCell result))
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
    }
}
