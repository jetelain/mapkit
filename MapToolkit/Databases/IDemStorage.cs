using System.Collections.Generic;
using System.Threading.Tasks;
using MapToolkit.DataCells;

namespace MapToolkit.Databases
{
    public interface IDemStorage
    {
        Task<DemDatabaseIndex> ReadIndex();

        Task<IDemDataCell> Load(string path);
    }
}