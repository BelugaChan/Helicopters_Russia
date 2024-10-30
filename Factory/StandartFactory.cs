using MinHash.Interfaces;
using MinHash.Models;
using NPOI.SS.UserModel;
using OfficeOpenXml;

namespace MinHash.Factory
{
    public class StandartFactory : IEntityFactory<Standart>
    {
        public Standart CreateFromRow(IRow row)
        {
            return new Standart
            {
                Name = row.GetCell(1)?.ToString()
            };
        }
    }
}
