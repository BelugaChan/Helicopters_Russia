using Abstractions.Interfaces;
using Algo.Models;
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
                Code = row.GetCell(0)?.ToString(),
                Name = row.GetCell(1)?.ToString(),
                NTD = row.GetCell(2)?.ToString()/*.Replace(" ", "")*/,
                MaterialNTD = row.GetCell(3)?.ToString()/*.Replace(" ", "")*/,
                ENSClassification = row.GetCell(4)?.ToString()
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
