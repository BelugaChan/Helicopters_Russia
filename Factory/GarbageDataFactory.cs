using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Models;
using NPOI.SS.UserModel;
using Serilog;

namespace Algo.Factory
{
    public class GarbageDataFactory : IUpdatedEntityFactoryGarbageData<GarbageData>
    {
        public GarbageData CreateFromRow(IRow row)
        {
            if (row.GetCell(1).CellType == CellType.Blank
                || row.GetCell(1) is null)
            {
                Log.Error("Отсутствие необходимых атрибутов в строке с грязными данными (наименование). Строка будет пропущена.");
                return null;
            }              
            return new GarbageData()
            {
                ShortName = row.GetCell(1).ToString(),
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
