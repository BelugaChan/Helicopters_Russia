using Algo.Abstract;
using Algo.Algotithms;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.Standart;
using Algo.Interfaces;
using Algo.Interfaces.Handlers;
using Algo.Models;
using Algo.Wrappers;
using ExcelHandler.Interfaces;
using ExcelHandler.Readers;
using ExcelHandler.Writers;
using F23.StringSimilarity;
using Helicopters_Russia.Services;
using Telegram.Bot;

namespace Helicopters_Russia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
           
            // Подключаем конфигурацию
            var configuration = builder.Configuration;
            builder.Services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

            // Регистрируем ITelegramBotClient с использованием токена
            var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            if (string.IsNullOrEmpty(botConfig?.BotToken))
            {
                throw new InvalidOperationException("Bot token is not configured in 'appsettings.json'.");
            }
            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botConfig.BotToken));

            //builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<FileProcessingService>();
            builder.Services.AddSingleton<UpdateHandler>();
            builder.Services.AddHostedService<BotService>();
            builder.Services.AddSingleton<IAdditionalENSHandler<CalsibCirclesHandler>,CalsibCirclesHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<RopesAndCablesHandler>, RopesAndCablesHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<LumberHandler>, LumberHandler>();
            builder.Services.AddSingleton<IENSHandler, ENSHandler>();
            builder.Services.AddSingleton<IGarbageHandle, GarbageHandler>();
            builder.Services.AddSingleton<IStandartHandle, StandartHandler>();
            builder.Services.AddSingleton<ISimilarityCalculator, CosineSimAlgo>();
            builder.Services.AddSingleton<IAlgoWrapper, AlgoWrapper<Standart>>();
            builder.Services.AddSingleton<IExcelReader, NPOIReader>();
            builder.Services.AddSingleton<IExcelWriter, NPOIWriter>();
            builder.Services.AddSingleton<Cosine>(provider => new Cosine(3));
            //builder.Services.AddSingleton<FileProcessingService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
