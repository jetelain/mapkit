using System;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    public interface IDemStorage
    {
        Task<DemDatabaseIndex> ReadIndex();

        Task<IDemDataCell> Load(string path);

        Task<IDemDataCell> LoadAsync(string path, string? expectedSha256, CancellationToken cancellationToken = default)
            => Load(path);

        Task<string?> GetSha256Async(string path, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}