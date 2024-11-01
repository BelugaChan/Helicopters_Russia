using Abstractions.Interfaces;
using Algo.Models;
using NPOI.SS.UserModel;

namespace Algo.Factory
{
    public class StandartFactory : IEntityFactory<Standart>
    {
        public Standart CreateFromRow(IRow row)
        {
            return new Standart
            {
                Name = row.GetCell(1)?.ToString()!
            };
        }
    }
}
