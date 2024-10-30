using MinHash.Interfaces;
using MinHash.Models;
using OfficeOpenXml;

namespace MinHash.Factory
{
    public class GarbageDataFactory : IEntityFactory<GarbageData>
    {
        public GarbageData CreateFromRow(ExcelWorksheet worksheet, int row)
        {
            return new GarbageData()
            {
                ShortName = worksheet.Cells[row,2].Value?.ToString(),
            };
        }
    }
}
