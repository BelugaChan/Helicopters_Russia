using Algo.Algotithms;
using Algo.Interfaces;
using ExcelHandler.Interfaces;
using ExcelHandler.Readers;
using ExcelHandler.Writers;
using Helicopters_Russia.Services;
using Telegram.Bot;
using Serilog;
using Helicopters_Russia.Context;
using Microsoft.EntityFrameworkCore;
using Helicopters_Russia.Interfaces;
using Helicopters_Russia.Repositories;
using Npgsql;

namespace Helicopters_Russia
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Logging.AddSerilog();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Log.Information("Waiting for PostgreSQL to become ready...");
            while (true)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        Log.Information("PostgreSQL is ready!");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"PostgreSQL is not ready: {ex.Message}");
                    await Task.Delay(2000); // Подождать 2 секунды перед повторной попыткой
                }
            }

            Log.Information("Starting bot...");

            
            try
            {
                // Подключаем конфигурацию
                var configuration = builder.Configuration;
                builder.Services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

                builder.Services.AddDbContext<BotDbContext>(
                    options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

                // Регистрируем ITelegramBotClient с использованием токена
                var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
                if (string.IsNullOrEmpty(botConfig?.BotToken))
                {
                    Log.Fatal("Bot token is not configured in 'appsettings.json'.");
                    throw new InvalidOperationException("Bot token is not configured in 'appsettings.json'.");
                }
                builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botConfig.BotToken));

                //builder.Services.AddHostedService<Worker>();
                builder.Services.AddSingleton<FileProcessingService>();
                builder.Services.AddSingleton<UpdateHandler>();
                builder.Services.AddHostedService<BotService>();
                builder.Services.AddSingleton<IStandartRepository, StandartRepository>();
                builder.Services.AddSingleton<IStandartService, StandartService>();
                builder.Services.AddSingleton<ISimilarityCalculator, CosineSimAlgo>();
                builder.Services.AddSingleton<IHandle, CosineSimAlgo>();
                builder.Services.AddSingleton<IExcelReader, NPOIReader>();
                builder.Services.AddSingleton<IExcelWriter, NPOIWriter>();
                builder.Services.AddSingleton<FileProcessingService>();

                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Sudden error while starting an app");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
