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
            var cellValues = new string[4];
            for (int i = 0; i < 4; i++)
            {
                cellValues[i] = row.GetCell(i+1)?.ToString() ?? string.Empty;
            }

            bool isCell0Empty = string.IsNullOrWhiteSpace(cellValues[0]);
            bool isCell3Empty = string.IsNullOrWhiteSpace(cellValues[3]);

            if (isCell0Empty || isCell3Empty)
            {
                if (isCell3Empty && !isCell0Empty)
                {
                    Log.Error($"Отсутствие необходимых атрибутов в строке с эталонами (наименование/классификатор ЕНС). Строка будет пропущена. \nНаименование грязной позиции: {cellValues[1]}");
                }               
                return null;
            }
            return new Standart
            {   
                Id = Guid.NewGuid(),
                //Code = cellValues[0],
                Name = cellValues[0],
                NTD = cellValues[1],
                MaterialNTD = cellValues[2],
                ENSClassification = cellValues[3]
            };
        }

        public Standart CreateUpdatedEntity(Guid id,/*string code,*/ string name, string ntd, string materialNTD, string ensClassification)
        {
            return new Standart 
            {
                Id = id,
                //Code = code, 
                Name = name,
                NTD = ntd,
                MaterialNTD = materialNTD,
                ENSClassification= ensClassification
            };
        }
    }
}
