using ExcelHandler.Interfaces;
using ExcelHandler.Mergers;
using Helicopters_Russia.Models;
using Microsoft.VisualBasic;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Concurrent;
using System.Threading;
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
        private Dictionary<long, List<string>> _userDirtyFiles = new();
        private Dictionary<long, List<string>> _userCleanFiles = new();
        private static readonly ConcurrentDictionary<long, Models.UserState> _userStates = new(); //Состояние пользователей

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"HandleError: {exception}");
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            {
                //Telegram.Bot.Types.Document
                await (update switch
                {
                    { Message: { } message } => OnMessage(update.Message, update, cancellationToken),
                    { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery, cancellationToken),
                    _ => UnknownUpdateHandlerAsync(update)
                });
            }
        }

        private async Task OnMessage(Message msg, Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Receive message \n\t\ttype: \"{update.Message.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\"\n");
            if (update.Message.Text is { } messageText)
                await (messageText.Split(' ')[0] switch
                {
                    "/proccesing_start" => StartProccessing(update),
                    _ => Usage(msg)
                });

            else if (update.Message.Document != null)
                await HandleDocumentUpload(update, cancellationToken);
        }

        async Task<Message> Usage(Message msg)
        {
            const string usage = """
                 <b><u>Bot menu</u></b>:
                 /proccesing_start  - Начать обработку файлов 
             """;
            return await _botClient.SendTextMessageAsync(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task StartProccessing(Update update)
        {
            var chatId = update.Message.Chat.Id;
            _logger.LogInformation($"The \"StartProccessingFiles\" method was called from the user: \"{update.Message.From}\"\n");

            if (!_userStates.TryGetValue(chatId, out var userState) || userState == UserState.Idle)
            {
                _userStates[chatId] = UserState.WaitingForDirtyData;
                _userCleanFiles[chatId] = new List<string>();
                _userDirtyFiles[chatId] = new List<string>();

                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Пожалуйста, отправьте \"грязные\" данные.",
                    cancellationToken: default
                );
            }
        }

        private async Task HandleDocumentUpload(Update update, CancellationToken cancellationToken)
        {
            var chatId = update.Message.Chat.Id;

            if (!_userStates.TryGetValue(chatId, out var userState)) //Проверяем состояние пользователя
                return;

            var document = update.Message.Document;
            var fileId = document.FileId;
            var fileName = document.FileName ?? "UnnamedFile"; // используем имя файла, если оно есть
            var filePath = Path.Combine("Download data", document.FileName);

            string directoryPath = "/app/Download data/";
            if (!Directory.Exists(directoryPath)) 
            { 
                Directory.CreateDirectory(directoryPath); 
            }

            var file = await botClient.GetFileAsync(document.FileId, cancellationToken);

            _logger.LogInformation($"The \"HandleDocumentUpload\" method was called from the user: \"{update.Message.From}\"\n");

            if (userState == UserState.WaitingForDirtyData && update.Message.Type == MessageType.Document)
            {
                _logger.LogInformation($"Добавлен файл \"грязных\" данных от пользователя {update.Message.From.Username}.");

                // Сохраняем путь к файлу (не fileId) в список "грязных" файлов
                if (!_userDirtyFiles.ContainsKey(chatId))
                    _userDirtyFiles[chatId] = new List<string>();

                _userDirtyFiles[chatId].Add(filePath);

                // Сохраняем файл временно
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await _botClient.DownloadFileAsync(file.FilePath!, fileStream, cancellationToken);
                }

                // Inline кнопка для завершения отправки грязных файлов
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все грязные файлы отправлены", "dirty_files_done")
                });

                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Файл принят. Добавьте еще файлы или нажмите \"Все грязные файлы отправлены\".",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: default
                );

            }
            else if (userState == UserState.WaitingForCleanData)
            {
                _logger.LogInformation($"Добавлен файл \"чистых\" данных от пользователя {update.Message.From.Username}.");

                // Сохраняем путь к файлу (не fileId) в список "чистых" файлов
                if (!_userCleanFiles.ContainsKey(chatId))
                    _userCleanFiles[chatId] = new List<string>();

                _userCleanFiles[chatId].Add(filePath);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await _botClient.DownloadFileAsync(file.FilePath!, fileStream, cancellationToken);
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все чистые файлы отправлены", "clean_files_done")
                });

                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Файл принят. Добавьте еще файлы или нажмите \"Все чистые файлы отправлены\".",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: default
                );
            }
        }

        public async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            if (callbackQuery.Data == "dirty_files_done" && _userStates[chatId] == UserState.WaitingForDirtyData)
            {
                // Переходим к ожиданию "чистых" данных
                _userStates[chatId] = UserState.WaitingForCleanData;

                // Мержим грязные файлы
                var dirtyFilePaths = _userDirtyFiles[chatId];  // Получаем пути всех грязных файлов
                var dirtyOutputPath = "Data";  // Путь для сохранения объединенного файла
                var dirtyResultFileName = "Грязные данные.xlsx";  // Имя объединенного файла

                await MergeFilesAsync(dirtyFilePaths, dirtyOutputPath, dirtyResultFileName, cancellationToken);

                await _botClient.SendTextMessageAsync(chatId, "Теперь отправьте \"чистые\" данные.", cancellationToken: default);
            }
            else if (callbackQuery.Data == "clean_files_done" && _userStates[chatId] == UserState.WaitingForCleanData)
            {
                // Заканчиваем прием файлов и переходим к обработке
                _userStates[chatId] = UserState.Idle;

                // Мержим чистые файлы
                var cleanFilePaths = _userCleanFiles[chatId];  // Получаем пути всех чистых файлов
                var cleanOutputPath = "Data";  // Путь для сохранения объединенного файла
                var cleanResultFileName = "Чистые данные.xlsx";  // Имя объединенного файла

                await MergeFilesAsync(cleanFilePaths, cleanOutputPath, cleanResultFileName, cancellationToken);



                await _botClient.SendTextMessageAsync(chatId, "Файлы получены. Начинаю обработку данных.", cancellationToken: default);

                StartProccessingFiles(callbackQuery, cancellationToken);

                // Здесь можно вызвать метод для обработки файлов
                //await _fileProcessingService.ProcessFilesAsync(chatId, _userDirtyFiles[chatId], _userCleanFiles[chatId]);
            }

            // Удаляем сообщение с кнопкой после нажатия
            await _botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, replyMarkup: null);
        }

        private async Task StartProccessingFiles(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            if (_userStates[chatId] != UserState.Idle)
                return;

            _logger.LogInformation($"Запуск обработки файлов для пользователя {callbackQuery.From.Username}");

            try
            {
                //string directoryPath = "/app/Data/";
                //if (!Directory.Exists(directoryPath))
                //{
                //    Directory.CreateDirectory(directoryPath);
                //}
                // Указываем пути к объединенным файлам
                var dirtyFilePath = Path.Combine("Data", "Грязные данные.xlsx");
                var cleanFilePath = Path.Combine("Data", "Чистые данные.xlsx");

                // Проверка существования файлов
                if (!System.IO.File.Exists(dirtyFilePath) || !System.IO.File.Exists(cleanFilePath))
                {
                    await _botClient.SendTextMessageAsync(chatId, "Не удалось найти загруженные файлы для обработки.", cancellationToken: cancellationToken);
                    return;
                }

                // Установка путей файлов в сервис обработки
                _fileProcessingService.SaveDirtyFilePath(dirtyFilePath);
                _fileProcessingService.SaveCleanFilePath(cleanFilePath);

                // Запуск обработки файлов
                var resultFilePath = await _fileProcessingService.ProcessFilesAsync(cancellationToken);

                _logger.LogInformation("Файлы обработаны, отправляем результат пользователю");

                System.IO.File.Delete(dirtyFilePath); // Удаляем временный файл после обработки
                System.IO.File.Delete(cleanFilePath); // Удаляем временный файл после обработки

                // Проверка размера файла и выбор способа отправки
                var fileInfo = new FileInfo(resultFilePath);
                if (fileInfo.Length > 49 * 1024 * 1024)
                {
                    // Если файл слишком большой, разбить и отправить по частям
                    await SplitAndSendLargeFileAsync(resultFilePath, chatId, cancellationToken);
                }
                else
                {
                    // Отправить файл целиком
                    await using var resultStream = System.IO.File.OpenRead(resultFilePath);
                    var inputFile = new InputFileStream(resultStream, "Result.xlsx");
                    await _botClient.SendDocumentAsync(chatId, inputFile, cancellationToken: cancellationToken);
                }

                // Очистка состояния и сброс данных для следующей операции
                _userStates[chatId] = UserState.Idle;
                _userDirtyFiles.Remove(chatId);
                _userCleanFiles.Remove(chatId);

                System.IO.File.Delete(resultFilePath); // Удаляем временный файл после обработки

                _logger.LogInformation($"Файл с результатом отправлен пользователю {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обработке файлов: {ex.Message}");
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Произошла ошибка при обработке файлов. Пожалуйста, попробуйте снова.",
                    cancellationToken: cancellationToken
                );
            }
        }

        private async Task SplitAndSendLargeFileAsync(string filePath, long chatId, CancellationToken cancellationToken)
        {
            const long maxFileSize = 49 * 1024 * 1024; // 49MB
            int partNumber = 1;
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Пока файл не полностью отправлен
            while (fileStream.Position < fileStream.Length)
            {
                var partPath = $"Data/Result_Part{partNumber}.xlsx";

                // Создайте новую часть файла, пока она не достигнет лимита
                using var partStream = new FileStream(partPath, FileMode.Create, FileAccess.Write);

                int bytesRead;
                byte[] buffer = new byte[1024 * 1024]; // Буфер 1 МБ
                long partSize = 0;

                while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0 && partSize < maxFileSize)
                {
                    await partStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    partSize += bytesRead;
                }

                // Отправка текущей части пользователю
                await using var sendStream = new FileStream(partPath, FileMode.Open, FileAccess.Read);
                await _botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: new InputFileStream(sendStream, $"Result_Part{partNumber}.xlsx"),
                    cancellationToken: cancellationToken
                );

                partNumber++;
            }
        }

        private async Task MergeFilesAsync(IEnumerable<string> filePaths, string outputFilePath, string resultFileName, CancellationToken cancellationToken)
        {
            try
            {
                //string directoryPath = "/app/Data/";
                //if (!Directory.Exists(directoryPath))
                //{
                //    Directory.CreateDirectory(directoryPath);
                //}

                // Используем IExcelMerger для объединения файлов
                IExcelMerger excelMerger = new NPOIMerger(); // Здесь можно внедрить через DI, если нужно.

                // Преобразуем список в List<string> (если это необходимо)
                var fileList = filePaths.ToList();

                // Выполняем объединение файлов
                await excelMerger.MergeExcelFilesAsync(fileList, outputFilePath, resultFileName);

                _logger.LogInformation($"Файлы успешно объединены и сохранены в: {Path.Combine(outputFilePath, resultFileName)}");

                foreach ( var filePath in fileList )
                    System.IO.File.Delete(filePath); // Удаляем временный файл после обработки
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при объединении файлов: {ex.Message}");
            }


        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
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

//public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//{



//    //    long userId = update.Message.From.Id; // Получаем идентификатор пользователя
//    //    var currentState = GetUserState(userId);

//    //    switch (currentState)
//    //    {
//    //        case UserState.Idle:
//    //            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
//    //            {
//    //                Console.WriteLine($"Receive message type: {}");
//    //            }
//    //            break;

//    //        case UserState.WaitingForDirtyData:
//    //            break;

//    //        case UserState.WaitingForCleanData: 
//    //            break;
//    //    }

//    //    if (update.Type == UpdateType.Message && update.Message.Text == "/start")
//    //    {
//    //        _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;
//    //    }



//if (update.Type == UpdateType.Message && update.Message.Text == "/start")
//{
//    // Устанавливаем начальное состояние пользователя
//    _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;

//    await _botClient.SendTextMessageAsync(
//        update.Message.Chat.Id,
//        "Отправьте \"грязные\" данные.",
//        cancellationToken: cancellationToken
//    );
//}
//else if (update.Type == UpdateType.Message && update.Message.Document != null)
//{
//    // Получение файла
//    var document = update.Message.Document;
//    var filePath = Path.Combine("Data", document.FileName);

//    // Скачиваем и сохраняем файл
//    var file = await botClient.GetFileAsync(document.FileId, cancellationToken);
//    await using (var fileStream = new FileStream(filePath, FileMode.Create))
//    {
//        await botClient.DownloadFileAsync(file.FilePath!, fileStream, cancellationToken);
//    }

//    // Обновляем состояние в зависимости от текущего этапа
//    var userState = _userStates.GetOrAdd(update.Message.Chat.Id, Models.UserState.WaitingForDirtyData);

//    if (userState == Models.UserState.WaitingForDirtyData)
//    {
//        _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForCleanData;
//        _fileProcessingService.SaveDirtyFilePath(filePath);

//        await _botClient.SendTextMessageAsync(
//            update.Message.Chat.Id,
//            "Отправьте \"чистые\" данные.",
//            cancellationToken: cancellationToken
//        );
//    }
//    else if (userState == Models.UserState.WaitingForCleanData)
//    {
//        _fileProcessingService.SaveCleanFilePath(filePath);

//        await _botClient.SendTextMessageAsync(
//            update.Message.Chat.Id,
//            "Начинается обработка.",
//            cancellationToken: cancellationToken
//        );

//        // Запуск обработки файлов
//        var resultFilePath = await _fileProcessingService.ProcessFilesAsync(cancellationToken);

//        await _botClient.SendTextMessageAsync(
//            update.Message.Chat.Id,
//            "Обработка завершена",
//            cancellationToken: cancellationToken
//        );

//        // Отправка результата пользователю
//        await using var resultStream = System.IO.File.OpenRead(resultFilePath);
//        var inputFile = new InputFileStream(resultStream, "Result.xlsx");

//        await botClient.SendDocumentAsync(
//            update.Message.Chat.Id,
//            inputFile,
//            cancellationToken: cancellationToken
//        );

//        // Сброс состояния пользователя
//        _userStates[update.Message.Chat.Id] = Models.UserState.WaitingForDirtyData;
//    }
//}
//else if (update.Type == UpdateType.Message && update.Message.Text != null)
//{
//    await botClient.SendTextMessageAsync(
//        update.Message.Chat.Id,
//        "Пожалуйста, загрузите Excel файлы для обработки.",
//        cancellationToken: cancellationToken
//    );
//}
//        }
