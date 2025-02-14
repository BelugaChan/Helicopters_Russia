using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Models;
using NPOI.SS.UserModel;
using Serilog;

namespace Algo.Factory
{
    public class StandartFactory : IUpdatedEntityFactoryStandart<Standart>
    {
        public Standart CreateFromRow(IRow row)
        {
            if (row.GetCell(1) is null
                || row.GetCell(4) is null
                || row.GetCell(1).CellType == CellType.Blank
                || row.GetCell(4).CellType == CellType.Blank)
            {
                Log.Error("Отсутствие необходимых атрибутов в строке с эталонами (наименование/классификатор ЕНС). Строка будет пропущена.");
                return null;
            }
            return new Standart
            {
                
                Id = Guid.NewGuid(),
                Code = row.GetCell(0).ToString() ?? string.Empty,
                Name = row.GetCell(1).ToString(),
                NTD = row.GetCell(2).ToString() ?? string.Empty,
                MaterialNTD = row.GetCell(3).ToString() ?? string.Empty,
                ENSClassification = row.GetCell(4).ToString()
            };
        }

        public Standart CreateUpdatedEntity(Guid id,string code, string name, string ntd, string materialNTD, string ensClassification)
        {
            return new Standart 
            {
                Id = id,
                Code = code, 
                Name = name,
                NTD = ntd,
                MaterialNTD = materialNTD,
                ENSClassification= ensClassification
            };
        }
    }
}
