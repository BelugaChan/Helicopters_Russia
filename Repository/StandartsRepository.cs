using AbstractionsAndModels.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Helicopters_Russia.Context;
using Helicopters_Russia.Interfaces.Repository;
using Serilog;

namespace Helicopters_Russia.Repository
{
    public class StandartsRepository<TStandart> : IStandartRepository<TStandart>
        where TStandart : class, IStandart
    {
        private MyDbContext<TStandart> dbContext;
        public StandartsRepository(MyDbContext<TStandart> dbContext)
            => this.dbContext = dbContext;

        public async Task<List<TStandart>> GetAllStandartsAsync()
            => await dbContext.Standarts.ToListAsync();

        public async Task<Dictionary<string, List<TStandart>>> GetStandartsWhichContainGostsAsync(HashSet<string> gosts)
        {
            var matchingClasses = await dbContext.Standarts
                .Where(st => gosts
                    .Any(g => st.NTD.Contains(g) || st.MaterialNTD.Contains(g)))
                        .Select(st => st.ENSClassification)
                            .Distinct()
                                .ToListAsync();

            var res = await dbContext.Standarts
                .Where(st => matchingClasses.Contains(st.ENSClassification))
                        .GroupBy(st => st.ENSClassification)
                            .ToDictionaryAsync(g => g.Key, g => g.ToList());
            return res;
        }

        public async Task<Dictionary<string, List<TStandart>>> GetAllGroupedStandartsAsync()
        {
            return await dbContext.Standarts.GroupBy(st => st.ENSClassification).ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

        public async Task<TStandart?> GetStandartByIdAsync(Guid id)
            => await dbContext.FindAsync<TStandart>(id);

        public async Task AddOrUpdateStandartAsync(TStandart standart)
        {
            var existingStandart = await dbContext.Standarts
                .FirstOrDefaultAsync(st => st.Name == standart.Name && st.ENSClassification == standart.ENSClassification);

            if (existingStandart is not null)
                dbContext.Entry(existingStandart).CurrentValues.SetValues(standart);
            else
                dbContext.Standarts.Add(standart);

            await dbContext.SaveChangesAsync();
        }

        public async Task AddStandartsAsync(List<TStandart> standarts)
        {
            Log.Information("In repository. Checkin unique els");
            var existingItems = dbContext.Standarts.Select(s => new {s.Name, s.ENSClassification}).AsNoTracking().ToList()
                .SelectMany(s => new[] { s.Name, s.ENSClassification }).ToList();

            var newRecords = standarts.Where(s => !existingItems.Contains(s.Name) && !existingItems.Contains(s.ENSClassification)).ToList();
            Log.Information("In repository. Checkin unique els done");
            await dbContext.BulkInsertAsync(newRecords/*, new BulkConfig() { CustomDestinationTableName= "standarts" }*/);
            Log.Information("In repository. Successfully added new standarts");
        }
    }
}
