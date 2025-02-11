using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;

namespace Helicopters_Russia.Services
{
    public class BotService : IHostedService
    {
        private readonly ITelegramBotClient botClient;
        private readonly UpdateHandler _updateHandler;

        public BotService(IOptions<BotConfiguration> config, UpdateHandler updateHandler)
        {
            botClient = new TelegramBotClient(config.Value.BotToken);
            _updateHandler = updateHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //await botClient.SetWebhook("https://localhost/bot");
            var me = await botClient.GetMe(cancellationToken);
            Log.Information($"Bot started with username: {me.Username}");
            botClient.StartReceiving(
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
