using Abstractions.Interfaces;
using Algo.Algotithms;
using Algo.Facade;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.GOST;
using Algo.Handlers.Standart;
using Algo.Interfaces.Algorithms;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using Algo.ProgressStrategy;
using Algo.Registry;
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

            builder.Services.AddSingleton<IExcelReader, NPOIReader>();
            builder.Services.AddSingleton<IExcelWriter, NPOIWriter>();
            builder.Services.AddSingleton<IProgressStrategy, ConsoleProgressStrategy>();
            builder.Services.AddSingleton<IUpdatedEntityFactoryGarbageData<GarbageData>, GarbageDataFactory>();
            builder.Services.AddSingleton<IUpdatedEntityFactoryStandart<Standart>, StandartFactory>();
            builder.Services.AddSingleton<IENSHandler, ENSHandler>();
            builder.Services.AddSingleton(provider => new Cosine(3));
            builder.Services.AddSingleton(provider => 
            {
                var registry = new ENSHandlerRegistry();

                registry.RegisterHandler(["Калиброванные круги, шестигранники, квадраты"], new Func<string, string>((str) => new CalsibCirclesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Пиломатериалы"], new Func<string, string>((str) => new LumberHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Канаты, Тросы"], new Func<string, string>((str) => new RopesAndCablesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Провода монтажные"], new Func<string, string>((str) => new MountingWiresHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Проволока"], new Func<string, string>((str) => new WireHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Прутки, шины из алюминия и сплавов", "Прутки, шины из меди и сплавов", "Прутки из титана и сплавов"], new Func<string, string>((str) => new BarsAndTiresHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Канаты, Тросы"], new Func<string, string>((str) => new RopesAndCablesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Трубы бесшовные", "Трубы сварные", "Трубы, трубки из алюминия и сплавов", "Трубы, трубки из меди и сплавов"], new Func<string, string>((str) => new RopesAndCablesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Шайбы"], new Func<string, string>((str) => new WashersHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Катанка, проволока", "Катанка, проволока из меди и сплавов"], new Func<string, string>((str) => new RodHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Шурупы"], new Func<string, string>((str) => new ScrewsHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Припои (прутки, проволока, трубки)"], new Func<string, string>((str) => new SoldersHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Гвозди, Дюбели"], new Func<string, string>((str) => new NailsHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Ленты, широкополосный прокат"], new Func<string, string>((str) => new TapesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Круги, шестигранники, квадраты"], new Func<string, string>((str) => new CirclesHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Части соединительные"], new Func<string, string>((str) => new ConnectionPartsHandler().AdditionalStringHandle(str)));
                registry.RegisterHandler(["Листы, плиты, ленты из титана и сплавов"], new Func<string, string>((str) => new SheetsAndPlatesHandler().AdditionalStringHandle(str)));

                return registry;
            });
            
            builder.Services.AddSingleton<IGostHandle, GostHandler>();
            builder.Services.AddSingleton<IGostRemove, GostRemover>();
            builder.Services.AddSingleton<IStandartHandle<Standart>, StandartHandler<Standart>>();
            builder.Services.AddSingleton<ISimilarityCalculator, CosineSimAlgo>();
            builder.Services.AddSingleton<AlgoFacade<Standart, GarbageData>>();




            //builder.Services.AddSingleton<FileProcessingService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
