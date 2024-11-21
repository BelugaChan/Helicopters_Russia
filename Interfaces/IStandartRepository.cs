using Abstractions.Interfaces;
using Algo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helicopters_Russia.Interfaces
{
    public interface IStandartRepository
    {
        Task<List<TStandart>> GetStandartsAsync<TStandart>() where TStandart : class, IStandart;

        Task InsertStandartAsync<TStandart>(TStandart item) where TStandart : class, IStandart;

        Task InsertStandartsAsync<TStandart>(List<TStandart> items) where TStandart : class, IStandart;
    }
}
