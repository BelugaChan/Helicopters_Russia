using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Models;
using NPOI.SS.UserModel;

namespace Algo.Factory
{
    public class StandartFactory : IUpdatedEntityFactoryStandart<Standart>
    {
        public Standart CreateFromRow(IRow row)
        {
            return new Standart
            {
                Id = Guid.NewGuid(),
                Code = row.GetCell(0).ToString() ?? string.Empty,
                Name = row.GetCell(1).ToString() ?? string.Empty,
                NTD = row.GetCell(2).ToString() ?? string.Empty,
                MaterialNTD = row.GetCell(3).ToString() ?? string.Empty,
                ENSClassification = row.GetCell(4).ToString() ?? string.Empty
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
