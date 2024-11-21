
using Abstractions.Interfaces;

namespace Helicopters_Russia.Interfaces
{
    public interface IStandartService
    {
        Task<List<TStandart>> GetStandartsAsync<TStandart>() where TStandart : class, IStandart;

        Task InsertStandartsAsync<TStandart>(List<TStandart> standarts) where TStandart : class, IStandart;
    }
}
