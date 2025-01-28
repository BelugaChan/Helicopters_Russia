using AbstractionsAndModels.Abstract;
using AbstractionsAndModels.Facade;
using AbstractionsAndModels.Interfaces.Algorithms;
using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.Handlers.GOST;
using AbstractionsAndModels.Interfaces.Handlers.Standart;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using Algo.Algotithms;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.GOST;
using Algo.Handlers.Standart;
using Algo.MethodStrategy;
using Algo.ProgressStrategy;
using Algo.Registry;
using Algo.Services.Order;
using Algo.Simpled;
using ExcelHandler.Interfaces;
using ExcelHandler.Readers;
using ExcelHandler.Writers;
using F23.StringSimilarity;
using Helicopters_Russia.Services;
using Serilog;
using Serilog.Formatting.Compact;
using Telegram.Bot;

namespace Helicopters_Russia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Http("http://localhost:5044",
                                queueLimitBytes: null,
                                textFormatter: new CompactJsonFormatter())
                .CreateLogger();


            builder.Services.AddSerilog();

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
            builder.Services.AddSingleton(provider => new CosineSimpled/*Cosine*/());
            builder.Services.AddSingleton<OrderService,OrderByLinearConvolutionService>();
            builder.Services.AddSingleton<IReplacementsStrategy, ReplacementsStrategy>();
            builder.Services.AddSingleton<IRegexReplacementStrategy, RegexReplacementsStrategy>();
            builder.Services.AddSingleton<IStopWordsStrategy, StopWordsStrategy>();
            builder.Services.AddSingleton<IProcessingGostStrategyFactory, ProcessingGostStrategyFactory>();

            builder.Services.Scan(scan => scan
                .FromAssemblyOf<LumberHandler>()
                .AddClasses(classes => classes.AssignableTo<IAdditionalENSHandler>())
                .AsSelfWithInterfaces()
                .WithTransientLifetime());


            builder.Services.AddSingleton(CreateENSHandlerRegistry);


            builder.Services.AddSingleton<IGostHandle, GostHandler>();
            builder.Services.AddSingleton<IGostRemove, GostRemover>();
            builder.Services.AddSingleton<IStandartHandle<Standart>, StandartHandler<Standart>>();
            builder.Services.AddSingleton<ISimilarityCalculator, CosineSimAlgo>();
            builder.Services.AddSingleton<AlgoFacade<Standart, GarbageData>>();




            //builder.Services.AddSingleton<FileProcessingService>();

            var host = builder.Build();
            host.Run();
        }

        private static ENSHandlerRegistry CreateENSHandlerRegistry(IServiceProvider provider)
        {
            var registry = new ENSHandlerRegistry();

            var handlers = provider.GetServices<IAdditionalENSHandler>().ToList();
            foreach (var handler in handlers)
            {
                registry.RegisterHandler(handler.SupportedKeys, handler.AdditionalStringHandle);
            }
            return registry;
        }
    }
}
