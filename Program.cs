using AbstractionsAndModels.Abstract;
using Helicopters_Russia.Facade;
using Helicopters_Russia.Interfaces.Algorithms;
using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.Handlers.GOST;
using AbstractionsAndModels.Interfaces.Handlers.Standart;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using Helicopters_Russia.Algotithms;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.GOST;
using Algo.Handlers.Standart;
using Algo.MethodStrategy;
using Algo.ProgrStrategy;
using Algo.Registry;
using Algo.Services.Order;
using Algo.Simpled;
using ExcelHandler.Interfaces;
using ExcelHandler.Readers;
using ExcelHandler.Writers;
using Helicopters_Russia.Enrichers;
using Helicopters_Russia.Formatters;
using Helicopters_Russia.Services;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Telegram.Bot;
using Helicopters_Russia.Context;
using Microsoft.EntityFrameworkCore;
using Helicopters_Russia.Repository;
using Helicopters_Russia.Interfaces.Repository;

namespace Helicopters_Russia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Создание и очистка папок для скаченных файлов при запуске бота
            string downloadDataPath = "Download Data";
            string dataPath = "Data";

            EnsureDirectoryAndCleanFiles(downloadDataPath);
            EnsureDirectoryAndCleanFiles(dataPath);

            var builder = /*WebApplication.CreateBuilder(args);*/Host.CreateApplicationBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.GrafanaLoki("http://loki:3100",
                                    labels: new List<LokiLabel>
                                    {
                                        new LokiLabel()
                                        {
                                            Key="tgBotHeliRus", Value="Monitored Service Version 1"
                                        }
                                    },
                                    textFormatter: new CustomLokiJsonFormatter())
                //.ReadFrom.Configuration(builder.Configuration)
                .Enrich.With(new RemovePropertiesEnricher())
                .CreateLogger();


            builder.Services.AddSerilog();

            // Подключаем конфигурацию
            var configuration = builder.Configuration;
            builder.Services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

            //Строка подключения к БД
            var connectionString = configuration.GetConnectionString("PostgresConnection");

            builder.Services.AddDbContext<MyDbContext<Standart>>(opt => opt.UseNpgsql(connectionString));

            // Регистрируем ITelegramBotClient с использованием токена
            var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            if (string.IsNullOrEmpty(botConfig?.BotToken))
            {
                Log.Error("Bot token is not configured in 'appsettings.json'.");
                throw new InvalidOperationException();
            }
            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botConfig.BotToken));

            //builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<FileProcessingService>();
            builder.Services.AddSingleton<UpdateHandler>();
            builder.Services.AddHostedService<BotService>();

            builder.Services.AddScoped<IStandartRepository<Standart>, StandartsRepository<Standart>>();

            builder.Services.AddSingleton<IExcelReader, NPOIReader>();
            builder.Services.AddSingleton<IExcelWriter, NPOIWriter>();
            builder.Services.AddSingleton<ProgressStrategy, SerilogProgressStrategy>();
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

            var host/*app*/ = builder.Build();

            //app.MapPost("/bot", async (HttpContext context, ITelegramBotClient botClient, UpdateHandler updateHandler) =>
            //{
            //    var update = await context.Request.ReadFromJsonAsync<Update>();
            //    if (update is not null)
            //    {
            //        await updateHandler.HandleUpdateAsync(botClient, update, CancellationToken.None);
            //    }
            //});
            //app.Run();
            host.Run();
        }

        public static void EnsureDirectoryAndCleanFiles(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath);

                if (files.Length > 0)
                {
                    Parallel.ForEach(files, (file, _) =>
                    {
                        System.IO.File.Delete(file); // Удаляем файлы в нескольких потоках
                    });

                    Log.Information($"In the \"{Path.GetFileName(directoryPath)}\" folder, unused files were deleted.");
                }
            }
            else
            {
                Directory.CreateDirectory(directoryPath);
                Log.Information($"The folder \"{Path.GetFileName(directoryPath)}\" was created.");
            }
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
