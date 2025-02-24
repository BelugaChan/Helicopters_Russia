using AbstractionsAndModels.Interfaces.Models;

namespace Helicopters_Russia.Interfaces.Repository
{
    public interface IStandartRepository<TStandart>
        where TStandart : IStandart
    {
        Task<TStandart?> GetStandartByIdAsync(Guid id);

        Task<List<TStandart>> GetAllStandartsAsync();

        Task<Dictionary<string, List<TStandart>>> GetStandartsWhichContainGostsAsync(HashSet<string> gosts);

        Task<Dictionary<string, List<TStandart>>> GetAllGroupedStandartsAsync();


        Task AddOrUpdateStandartAsync(TStandart standart);

        Task AddStandartsAsync(List<TStandart> standarts);
    }
}
