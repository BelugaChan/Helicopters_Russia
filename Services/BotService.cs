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

namespace Helicopters_Russia.Services
{
    public class BotService : IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UpdateHandler _updateHandler;

        public BotService(IOptions<BotConfiguration> config, UpdateHandler updateHandler)
        {
            _botClient = new TelegramBotClient(config.Value.BotToken);
            _updateHandler = updateHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _botClient.GetMeAsync(cancellationToken);
            Console.WriteLine($"Bot started with username: {me.Username}");

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
