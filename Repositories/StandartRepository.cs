using Abstractions.Interfaces;
using Algo.Models;
using Helicopters_Russia.Context;
using Helicopters_Russia.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helicopters_Russia.Repositories
{
    public class StandartRepository : IStandartRepository
    {
        private BotDbContext _botDbContext;
        public StandartRepository(BotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        public async Task<List<TStandart>> GetStandartsAsync<TStandart>() where TStandart : class, IStandart
        {
            var standarts = await _botDbContext.Set<TStandart>().ToListAsync();
            return standarts;
        }

        public async Task InsertStandartAsync<TStandart>(TStandart item) where TStandart : class, IStandart
        {
            await _botDbContext.Set<TStandart>().AddAsync(item);
            await _botDbContext.SaveChangesAsync();
        }

        public async Task InsertStandartsAsync<TStandart>(List<TStandart> items) where TStandart : class, IStandart
        {
            await _botDbContext.Set<TStandart>().AddRangeAsync(items);
            await _botDbContext.SaveChangesAsync();
        }
    }
}
