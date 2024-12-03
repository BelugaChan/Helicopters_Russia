using Abstractions.Interfaces;
using Algo.Models;
using NPOI.SS.UserModel;

namespace Algo.Factory
{
    public class GarbageDataFactory : IUpdatedEntityFactoryGarbageData<GarbageData>
    {
        public GarbageData CreateFromRow(IRow row)
        {
            return new GarbageData()
            {
                ShortName = row.GetCell(1)?.ToString()
            };
        }

        public GarbageData CreateUpdatedEntity(string shortName)
        {
            return new GarbageData
            {
                ShortName = shortName
            };
        }
    }
}
