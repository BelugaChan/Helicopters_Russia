using Abstractions.Interfaces;
using Algo.Models;
using Helicopters_Russia.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helicopters_Russia.Services
{
    public class StandartService : IStandartService
    {
        private IStandartRepository _repository;
        private readonly ILogger<StandartService> _logger;
        public StandartService(IStandartRepository repository, ILogger<StandartService> logger)
        {
            _repository = repository;
            _logger = logger;

        }

        public async Task<List<TStandart>> GetStandartsAsync<TStandart>() where TStandart : class, IStandart
        {
            _logger.LogInformation("Запущен метод получения коллекции стандартов");
            var standarts = await _repository.GetStandartsAsync<TStandart>();
            return standarts;
        }

        public async Task InsertStandartsAsync<TStandart>(List<TStandart> standarts) where TStandart : class, IStandart
        {
            await _repository.InsertStandartsAsync(standarts);
            //double indexer = 0;
            //foreach (var item in standarts)
            //{
            //    await _repository.InsertStandartAsync(item);
            //    if (indexer % 100 == 0)
            //    {
            //        _logger.LogInformation($"процент добавления стандартов в БД: {indexer/standarts.Count*100:0.00}");
            //    }
            //    indexer++;
            //}
        }
    }
}
