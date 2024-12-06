using System.Collections.Generic;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Databases
{
    public interface IDemStorage
    {
        Task<DemDatabaseIndex> ReadIndex();

        Task<IDemDataCell> Load(string path);
    }
}