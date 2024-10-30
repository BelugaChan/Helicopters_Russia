using MinHash.Interfaces;
using MinHash.Models;
using NPOI.SS.UserModel;
using OfficeOpenXml;

namespace MinHash.Factory
{
    public class GarbageDataFactory : IEntityFactory<GarbageData>
    {
        public GarbageData CreateFromRow(IRow row)
        {
            return new GarbageData()
            {
                ShortName = row.GetCell(1)?.ToString()
            };
        }
    }
}
