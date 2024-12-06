using System.Text.Json.Serialization;

namespace Pmad.Cartography.Databases
{
    [JsonSerializable(typeof(DemDatabaseIndex))]
    internal partial class DemDatabaseIndexContext : JsonSerializerContext
    {
    }
}
