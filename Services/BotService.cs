using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Abstractions.Interfaces;
using Helicopters_Russia.Interfaces;
using ExcelHandler.Interfaces;
using Algo.Models;
using Algo.Factory;
using Algo.Interfaces;

namespace Helicopters_Russia.Services
{
    public class BotService : IHostedService
    {
        private readonly ILogger<BotService> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly UpdateHandler _updateHandler;
        private readonly IStandartService _standartService;
        private readonly IExcelReader _excelReader;
        private readonly IHandle _handle;

        private readonly string pathToInitExcelStandarts = "initialxlsx/Эталоны_общий.xlsx";
        public BotService(IOptions<BotConfiguration> config, UpdateHandler updateHandler, ILogger<BotService> logger, IStandartService standartService, IExcelReader excelReader, IHandle handle)
        {
            _botClient = new TelegramBotClient(config.Value.BotToken);
            _updateHandler = updateHandler;
            _logger = logger;
            _standartService = standartService;
            _excelReader = excelReader;
            _handle = handle;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _botClient.GetMeAsync(cancellationToken);
            //Console.WriteLine($"Bot started with username: {me.Username}");
            var returnedStandarts = await _standartService.GetStandartsAsync<Standart>();
            if (returnedStandarts.Count == 0)
            {
                _logger.LogInformation($"Bot started with username: {me.Username}");
                var standarts = _excelReader.CreateCollectionFromExcel<Standart, StandartFactory>(pathToInitExcelStandarts, new StandartFactory());
                _logger.LogInformation("Стандарты считаны из init excel файла");
                var handledStandartNames = _handle.HandleStandartNames(standarts, new StandartFactory());
                _logger.LogInformation("Обработаны наименования стандартов");
                await _standartService.InsertStandartsAsync(handledStandartNames);
                _logger.LogInformation($"Стандарты записаны в бд");
            }
            
            _botClient.StartReceiving(
                _updateHandler,
                cancellationToken: cancellationToken
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
