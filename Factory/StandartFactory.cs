using MinHash.Interfaces;
using MinHash.Models;
using OfficeOpenXml;

namespace MinHash.Factory
{
    public class StandartFactory : IEntityFactory<Standart>
    {
        public Standart CreateFromRow(ExcelWorksheet worksheet, int row)
        {
            return new Standart()
            {
                Name = worksheet.Cells[row, 2].Value?.ToString()
            };
        }
    }
}
