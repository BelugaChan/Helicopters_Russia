using Helicopters_Russia.Models;
using Microsoft.VisualBasic;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Helicopters_Russia.Services
{
    public class UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, FileProcessingService fileProcessingService) : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly ILogger<UpdateHandler> _logger = logger;
        private readonly FileProcessingService _fileProcessingService = fileProcessingService;

        // Хранение состояний пользователей
        private static readonly ConcurrentDictionary<long, Models.UserState> _userStates = new();

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogInformation($"HandleError: {exception}");
            //Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

        // public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //     await (update switch
        //     {
        //         { Message: { } message }                => OnMessage(update.Message, update),

        //         _                                       => UnknownUpdateHandlerAsync(update)
        //     });
        // }

        // private async Task OnMessage(Message msg, Update update)
        // {
        //     logger.LogInformation($"Receive message type: {update.Message.Type}");
        //     if (update.Message.Text is not { } messageText)
        //         return;

        //     Message sentMessage = await (messageText.Split(' ')[0] switch
        //     {
        //         "/proccesing_start"             => StartProcessingFiles(update),
        //         _                               => Usage(msg)
        //     });

        //     logger.LogInformation($"The message was sent with id: {sentMessage.MessageId} from {update.Message.From}");
        // }

        // async Task<Message> Usage(Message msg)
        // {
        //     const string usage = """
        //         <b><u>Bot menu</u></b>:
        //         /proccesing_start  - Начать обработку файлов 
        //     """;
        //     return await _botClient.SendTextMessageAsync(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        // }

        //private async Task StartProcessingFiles(Update update)
        //{

        //}

        // private Task UnknownUpdateHandlerAsync(Update update)
        // {
        //     logger.LogInformation($"Unknown update type: {update.Type}");
        //     return Task.CompletedTask;
        // }

        //public void UpdateUserState(long userId, UserState newState)
        //{
        //    _userStates[userId] = newState; // Обновляем или добавляем состояние пользователя
        //}
        //public UserState GetUserState(long userId)
        //{
        //    return _userStates.TryGetValue(userId, out var state) ? state : UserState.Idle; // Возвращаем состояние
        //}

        //public void HandleUpdateAsync(Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
        //{
        //    // Проверяем, является ли обновление сообщением
        //    if (update.Type == UpdateType.Message && update.Message != null)
        //    {
        //        long userId = update.Message.From.Id; // Получаем идентификатор пользователя

        //        // Теперь вы можете использовать userId для управления состоянием
        //        HandleUserInput(userId, update.Message.Text);
        //    }
        //}

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {



            //    long userId = update.Message.From.Id; // Получаем идентификатор пользователя
            //    var currentState = GetUserState(userId);

            //    switch (currentState)
            //    {
            //        case UserState.Idle:
            //            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            //            {
            //                Console.WriteLine($"Receive message type: {}");
            //            }
            //            break;

            //        case UserState.WaitingForDirtyData:
            //            break;

            //        case UserState.WaitingForCleanData: 
            //            break;
            //    }

            //    if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            //    {
            //        _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;
            //    }



            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            {
                // Устанавливаем начальное состояние пользователя
                _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;

                await _botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Отправьте \"грязные\" данные.",
                    cancellationToken: cancellationToken
                );
            }
            else if (update.Type == UpdateType.Message && update.Message.Document != null)
            {
                // Получение файла
                var document = update.Message.Document;
                var filePath = Path.Combine("Data", document.FileName);

                // Скачиваем и сохраняем файл
                var file = await botClient.GetFileAsync(document.FileId, cancellationToken);
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await botClient.DownloadFileAsync(file.FilePath!, fileStream, cancellationToken);
                }

                // Обновляем состояние в зависимости от текущего этапа
                var userState = _userStates.GetOrAdd(update.Message.Chat.Id, Models.UserState.WaitingForDirtyData);

                if (userState == Models.UserState.WaitingForDirtyData)
                {
                    _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForCleanData;
                    _fileProcessingService.SaveDirtyFilePath(filePath);

                    await _botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Отправьте \"чистые\" данные.",
                        cancellationToken: cancellationToken
                    );
                }
                else if (userState == Models.UserState.WaitingForCleanData)
                {
                    _fileProcessingService.SaveCleanFilePath(filePath);

                    await _botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Начинается обработка.",
                        cancellationToken: cancellationToken
                    );

                    // Запуск обработки файлов
                    var resultFilePath = await _fileProcessingService.ProcessFilesAsync(cancellationToken);

                    await _botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Обработка завершена",
                        cancellationToken: cancellationToken
                    );

                    // Отправка результата пользователю
                    await using var resultStream = System.IO.File.OpenRead(resultFilePath);
                    var inputFile = new InputFileStream(resultStream, "Result.xlsx");

                    await botClient.SendDocumentAsync(
                        update.Message.Chat.Id,
                        inputFile,
                        cancellationToken: cancellationToken
                    );

                    // Сброс состояния пользователя
                    _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;
                }
            }
            else if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Пожалуйста, загрузите Excel файлы для обработки.",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
