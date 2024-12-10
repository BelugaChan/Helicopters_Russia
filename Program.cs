using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Algotithms;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.GOST;
using Algo.Handlers.Standart;
using Algo.Interfaces;
using Algo.Interfaces.Algorithms;
using Algo.Interfaces.Handlers;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.Wrappers;
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
            builder.Services.AddSingleton<IENSHandler, ENSHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<CalsibCirclesHandler>,CalsibCirclesHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<RopesAndCablesHandler>, RopesAndCablesHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<LumberHandler>, LumberHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<MountingWiresHandler>, MountingWiresHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<WireHandler>, WireHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<BarsAndTiresHandler>, BarsAndTiresHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<PipesHandler>, PipesHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<WashersHandler>, WashersHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<RodHandler>, RodHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<ScrewsHandler>, ScrewsHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<SoldersHandler>, SoldersHandler>();
            builder.Services.AddSingleton<IAdditionalENSHandler<NailsHandler>, NailsHandler>();
            builder.Services.AddSingleton<IGostHandle, GostHandler>();
            builder.Services.AddSingleton<IGostRemove, GostRemover>();
            builder.Services.AddSingleton<IStandartHandle<Standart>, StandartHandler<Standart>>();
            builder.Services.AddSingleton<ISimilarityCalculator, CosineSimAlgo>();
            builder.Services.AddSingleton<IAlgoWrapper<Standart, GarbageData>, AlgoWrapper<Standart, GarbageData>>();
            builder.Services.AddSingleton<IExcelReader, NPOIReader>();
            builder.Services.AddSingleton<IExcelWriter, NPOIWriter>();
            builder.Services.AddSingleton<IUpdatedEntityFactoryGarbageData<GarbageData>, GarbageDataFactory>();
            builder.Services.AddSingleton<IUpdatedEntityFactoryStandart<Standart>, StandartFactory>();
            builder.Services.AddSingleton<Cosine>(provider => new Cosine(3));
            //builder.Services.AddSingleton<FileProcessingService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
