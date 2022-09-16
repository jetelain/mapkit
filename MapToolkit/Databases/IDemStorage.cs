using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    public interface IDemStorage
    {
        Task<DemDatabaseIndex> ReadIndex();

        Task<IDemDataCell> Load(string path);
    }
}