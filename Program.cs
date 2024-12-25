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
using Algo.MethodStrategy;
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
            builder.Services.AddSingleton(provider => new Cosine(2));
            builder.Services.AddSingleton<IReplacementsStrategy, ReplacementsStrategy>();
            builder.Services.AddSingleton<IRegexReplacementStrategy, RegexReplacementsStrategy>();
            builder.Services.AddSingleton<IStopWordsStrategy, StopWordsStrategy>();

            builder.Services.AddTransient<IAdditionalENSHandler<CalsibCirclesHandler>,CalsibCirclesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<LumberHandler>, LumberHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<RopesAndCablesHandler>, RopesAndCablesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<MountingWiresHandler>, MountingWiresHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<WireHandler>, WireHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<BarsAndTiresHandler>, BarsAndTiresHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<PipesHandler>, PipesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<WashersHandler>, WashersHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<RodCopperHandler>, RodCopperHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<RodHandler>, RodHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<ScrewsHandler>, ScrewsHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<SoldersHandler>, SoldersHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<NailsHandler>, NailsHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<TapesHandler>, TapesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<CirclesHandler>, CirclesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<SheetsAndPlatesHandler>, SheetsAndPlatesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<InsulatingTubesHandler>, InsulatingTubesHandler>();
            builder.Services.AddTransient<IAdditionalENSHandler<RivetsHandler>, RivetsHandler>();

            builder.Services.AddSingleton(provider => 
            {
                var registry = new ENSHandlerRegistry();

                registry.RegisterHandler(
                    ["Калиброванные круги, шестигранники, квадраты"], 
                    str => 
                    {
                        var handler = ActivatorUtilities.CreateInstance<CalsibCirclesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Пиломатериалы"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<LumberHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Канаты, Тросы"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<RopesAndCablesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Провода монтажные"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<MountingWiresHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Проволока"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<WireHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Прутки, шины из алюминия и сплавов", "Прутки, шины из меди и сплавов", "Прутки из титана и сплавов"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<BarsAndTiresHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Трубы бесшовные", "Трубы сварные", "Трубы, трубки из алюминия и сплавов", "Трубы, трубки из меди и сплавов"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<PipesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Шайбы"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<WashersHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Катанка, проволока из меди и сплавов"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<RodCopperHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Катанка, проволока"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<RodHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Шурупы"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<ScrewsHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Припои (прутки, проволока, трубки)"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<SoldersHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Гвозди, Дюбели"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<NailsHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Ленты, широкополосный прокат"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<TapesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Круги, шестигранники, квадраты"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<CirclesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Части соединительные"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<ConnectionPartsHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Листы, плиты, ленты из титана и сплавов"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<SheetsAndPlatesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Трубки изоляционные гибкие"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<InsulatingTubesHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

                registry.RegisterHandler(
                    ["Заклепки"],
                    str =>
                    {
                        var handler = ActivatorUtilities.CreateInstance<RivetsHandler>(provider);
                        return handler.AdditionalStringHandle(str);
                    });

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
