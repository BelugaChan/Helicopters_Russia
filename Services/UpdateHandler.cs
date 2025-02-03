using AbstractionsAndModels.Abstract;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using ExcelHandler.Interfaces;
using ExcelHandler.Mergers;
using Helicopters_Russia.Models;
using Serilog;
using Serilog.Context;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Helicopters_Russia.Services
{
    public class UpdateHandler
        (ITelegramBotClient botClient,
        //ILogger<UpdateHandler> logger, 
        FileProcessingService fileProcessingService,
        ProgressStrategy progressStrategy) : IUpdateHandler
    {
        private readonly string downloadDataPath = "Download Data";
        private readonly string dataPath = "Data";
        private Dictionary<long, List<string>> _userDirtyFiles = new(); // Список Грязных файлов для каждого пользователя
        private Dictionary<long, List<string>> _userCleanFiles = new(); // Список Чистых файлов для каждого пользователя
        private static readonly ConcurrentDictionary<long, Models.UserState> _userStates = new(); //Состояние пользователей

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken) // Обработка ошибок
        {
            Log.Information($"HandleError: {exception}.");
            //logger.LogInformation($"HandleError: {exception}, time: {DateTimeOffset.Now}");
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) // Обработка сообщений 
        {
            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //var directoryPath = Path.Combine(rootDirectory, "Download Data");
            if (_userDirtyFiles.Count == 0 && _userCleanFiles.Count == 0)
            {
                if (Directory.Exists(/*"Download Data"*/downloadDataPath))
                {
                    // Получаем массив файлов в указанной папке
                    var files = Directory.GetFiles(downloadDataPath/*"Download Data"*/);

                    // Если файлы существуют, удаляем их
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            System.IO.File.Delete(file); // Удаляем файл
                        }
                        Log.Information("In the \"Download Data\" folder, there were unused files, and they have been deleted.");
                        //logger.LogInformation($"\nIn the \"Download Data\" folder, there were unused files, and they have been deleted, time: {DateTimeOffset.Now}.");
                    }
                }
                else if (!Directory.Exists(downloadDataPath/*"Download Data"*/)) //Если папка не существует, создаем ее
                {
                    Directory.CreateDirectory(downloadDataPath);
                    Log.Information("The folder \"Download Data\" was created.");
                    //logger.LogInformation($"The folder \"Download Data\" was created, time: {DateTimeOffset.Now}.");
                }

                if (Directory.Exists(dataPath/*"Data"*/))
                {
                    // Получаем массив файлов в указанной папке
                    var files = Directory.GetFiles(dataPath/*"Data"*/);

                    // Если файлы существуют, удаляем их
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            System.IO.File.Delete(file); // Удаляем файл
                        }
                        Log.Information("In the \"Download Data\" folder, there were unused files, and they have been deleted.");
                        //logger.LogInformation($"\nIn the \"Download Data\" folder, there were unused files, and they have been deleted, time: {DateTimeOffset.Now}.");
                    }
                }
                else if (!Directory.Exists(dataPath/*"Data"*/)) //Если папка не существует, создаем ее
                {
                    Directory.CreateDirectory(dataPath/*"Data"*/);
                    Log.Information("The folder \"Data\" was created.");
                    //logger.LogInformation($"The folder \"Data\" was created, time: {DateTimeOffset.Now}.");
                }
            }


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

        private async Task OnMessage(Message msg, Update update, CancellationToken cancellationToken) // Обработка текстовых сообщений 
        {
            Log.Information($"Receive message \n\t\ttype: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\".");
            //logger.LogInformation($"Receive message \n\t\ttype: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");
            if (update.Message.Text is { } messageText)
                await (messageText.Split(' ')[0] switch
                {
                    "/proccesing_start" => StartProccessing(update),
                    "/status" => GetStatus(update),
                    _ => UnknownCommand(msg, update)
                });

            else if (update.Message.Document != null)
                await HandleDocumentUpload(update, cancellationToken);
        }

        async Task<Message> UnknownCommand(Message msg, Update update) //Обработка неизвестной команды 
        {
            Log.Warning($"Receive unknown command\n\t\tcommand: \"{msg.Text}\", type: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\".");
            //logger.LogInformation($"Receive unknown command\n\t\tcommand: \"{msg.Text}\", type: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

            const string usage = """
                Вы ввели неизвестную команду
             """;

            await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());

            return await Usage(msg);
        }

        async Task<Message> InvalidUserState(Message msg, Update update) // Обработка ошибки состояния пользователя
        {
            Log.Information($"Data received from user prior to command invocation\n\t\tData: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\".");
            //logger.LogInformation($"Data received from user prior to command invocation\n\t\tData: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

            const string usage = """
                Для обработки данных сначала нужно вызвать команду
                /proccesing_start  - Начать обработку файлов
             """;

            return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        async Task<Message> Usage(Message msg) //Вывод команд 
        {
            const string usage = """
                 <b><u>Bot menu</u></b>:
                 /proccesing_start  - Начать обработку файлов 
                 /status - отобразить статус работы алгоритма
             """;
            return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task GetStatus(Update update)
        {
            var chatId = update.Message!.Chat.Id;
            Log.Information($"The \"GetStatus\" method was called from the user: \"{update.Message.From}\".");
            //logger.LogInformation($"The \"GetStatus\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");
            var progress = progressStrategy.GetCurrentProgress();
            await botClient.SendMessage(
                chatId,
                $"Step: {progress.Step}, Progress: {progress.CurrentProgress}%",
                cancellationToken: default
            );
        }

        private async Task StartProccessing(Update update) //Начало обработки пользователя 
        {
            var chatId = update.Message!.Chat.Id;
            Log.Information($"The \"StartProccessingFiles\" method was called from the user: \"{update.Message.From}\".");
            //logger.LogInformation($"The \"StartProccessingFiles\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

            if (!_userStates.TryGetValue(chatId, out var userState) || userState == UserState.Idle)
            {
                _userStates[chatId] = UserState.WaitingForDirtyData;
                _userCleanFiles[chatId] = new List<string>();
                _userDirtyFiles[chatId] = new List<string>();

                await botClient.SendMessage(
                    chatId,
                    "Пожалуйста, отправьте \"грязные\" данные.",
                    cancellationToken: default
                );
            }
            else if (_userStates.TryGetValue(chatId, out userState) && (userState == UserState.WaitingForCleanData || userState == UserState.WaitingForDirtyData))
            {
                if (userState == UserState.WaitingForCleanData)
                    await botClient.SendMessage(
                        chatId,
                        "Пожалуйста, отправьте \"чистые\" данные.",
                        cancellationToken: default
                    );
                else if (userState == UserState.WaitingForDirtyData)
                    await botClient.SendMessage(
                        chatId,
                        "Пожалуйста, отправьте \"грязные\" данные.",
                        cancellationToken: default
                    );
            }
        }

        private async Task HandleDocumentUpload(Update update, CancellationToken cancellationToken) // Получение файлов 
        {
            var chatId = update.Message!.Chat.Id; // Получаем Id чата

            // Проверяем состояние пользователя
            if (!_userStates.TryGetValue(chatId, out var userState))
            {
                await InvalidUserState(update.Message, update);
                return;
            }


            var document = update.Message.Document; // Получаем документ
            var fileId = document!.FileId; // Получаем его Id
            var fileExtension = Path.GetExtension(document.FileName); // Получаем расширение файла
            var fileName = document.FileId; // использует имя файла - id этого файла   
            //var fileName = document.FileName ?? fileId; // используем имя файла, если оно есть, иначе - его id
            var filePath = Path.Combine(/*"Download data"*/downloadDataPath, fileName + fileExtension);

            var file = await botClient.GetFile(document.FileId, cancellationToken); // Загружаем документ

            Log.Information($"The \"HandleDocumentUpload\" method was called from the user: \"{update.Message.From}\".");
            //logger.LogInformation($"The \"HandleDocumentUpload\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

            if (userState == UserState.WaitingForDirtyData)
            {
                Log.Information($"Added file of \"Dirty\" data from the user: \"{update.Message.From}\".");
                //logger.LogInformation($"Added file of \"Dirty\" data from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

                // Сохраняем путь к файлу (не fileId) в список "грязных" файлов
                if (!_userDirtyFiles.ContainsKey(chatId))
                    _userDirtyFiles[chatId] = new List<string>();

                _userDirtyFiles[chatId].Add(filePath);

                // Сохраняем файл временно
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await botClient.DownloadFile(file.FilePath!, fileStream, cancellationToken);
                }

                // Inline кнопки
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отправлены неправильные \"Грязные\" данные", "incorrect_dirty_data_sent") // кнопка для отмены отправки файлов
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Все \"Грязные\" файлы отправлены", "dirty_files_done") // подтверждение отправки
                    }
                });

                await botClient.SendMessage(
                    chatId,
                    "Файл принят.\n    Добавьте еще файлы или нажмите \"Все \"Грязные\" файлы отправлены\".\n    Если был отправлен неправильный файл, нажмите \"Отправлены неправильные \"Грязные\" данные\".",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: default
                );
            }

            else if (userState == UserState.WaitingForCleanData)
            {
                Log.Information($"Added file of \"Clean\" data from the user: \"{update.Message.From}\".");
                //logger.LogInformation($"Added file of \"Clean\" data from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

                // Сохраняем путь к файлу (не fileId) в список "чистых" файлов
                if (!_userCleanFiles.ContainsKey(chatId))
                    _userCleanFiles[chatId] = new List<string>();

                _userCleanFiles[chatId].Add(filePath);

                // Сохраняем файл временно
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await botClient.DownloadFile(file.FilePath!, fileStream, cancellationToken);
                }

                // Inline кнопки
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отправлены неправильные \"Чистые\" данные", "incorrect_clear_data_sent") // кнопка для отмены отправки файлов
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Все \"Чистые\" файлы отправлены", "clean_files_done") // подтверждение отправки
                    }
                });

                await botClient.SendMessage(
                    chatId,
                    "Файл принят.\n    Добавьте еще файлы или нажмите \"Все \"Чистые\" файлы отправлен\".\n    Если был отправлен неправильный файл, нажмите \"Отправлены неправильные \"Чистые\" данные\".",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: default
                );
            }
        }

        public async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken) // Ответы на Inline кнопки 
        {
            var chatId = callbackQuery.Message!.Chat.Id;

            // Проверяем состояние пользователя
            if (!_userStates.TryGetValue(chatId, out var userState))
            {
                return;
            }

            // Удаление Чистых файлов 
            if (callbackQuery.Data == "incorrect_clear_data_sent")
            {
                var cleanFilePaths = _userCleanFiles[chatId]; // Получаем пути всех чистых файлов

                // Удаляем файлы
                foreach (var file in cleanFilePaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error deleting uploaded clean files: {ex.Message}");
                        //logger.LogError($"Error deleting uploaded clean files: {ex.Message}, time: {DateTimeOffset.Now}\n");
                    }
                }

                _userCleanFiles[chatId].Clear(); // Удаляем сохранения файлов для пользователя

                Log.Information($"Deleted \"Clean\" data for the user: \"{callbackQuery.From}\".");
                //logger.LogInformation($"Deleted \"Clean\" data for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

                await botClient.SendMessage(
                    chatId,
                    "\"Чистые\" файлы были удалены. Можете загрузить их заново",
                    cancellationToken: cancellationToken
                );
            }

            // Удаление Грязных данных
            else if (callbackQuery.Data == "incorrect_dirty_data_sent")
            {
                var dirtyFilePaths = _userDirtyFiles[chatId]; // Получаем пути всех чистых файлов

                // Удаляем файлы
                foreach (var file in dirtyFilePaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error deleting uploaded dirty files: {ex.Message}");
                        //logger.LogError($"Error deleting uploaded dirty files: {ex.Message}, time: {DateTimeOffset.Now}");

                        //await botClient.SendTextMessageAsync(
                        //    chatId,
                        //    "Произошла ошибка при удалении файлов.",
                        //    cancellationToken: cancellationToken
                        //);
                    }
                }

                _userDirtyFiles[chatId].Clear(); // Удаляем сохранения файлов для пользователя

                Log.Information($"Deleted \"Dirty\" data for the user: \"{callbackQuery.From}\".");
                //logger.LogInformation($"Deleted \"Dirty\" data for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

                await botClient.SendMessage(
                    chatId,
                    "\"Грязные\" файлы были удалены. Можете загрузить их заново",
                    cancellationToken: cancellationToken
                );
            }

            // Все грязные данные получены
            else if (callbackQuery.Data == "dirty_files_done" && _userStates[chatId] == UserState.WaitingForDirtyData)
            {
                _userStates[chatId] = UserState.WaitingForCleanData; // Переходим к ожиданию "чистых" данных

                // Мержим грязные файлы
                var dirtyFilePaths = _userDirtyFiles[chatId];  // Получаем пути всех грязных файлов
                var dirtyOutputPath = dataPath/*"Data"*/;  // Путь для сохранения объединенного файла
                var dirtyResultFileName = "Грязные данные.xlsx";  // Имя объединенного файла

                // Проверяем, существует ли файл, и если да, то удаляем его
                if (System.IO.File.Exists(Path.Combine(dirtyOutputPath, dirtyResultFileName)))
                {
                    System.IO.File.Delete(Path.Combine(dirtyOutputPath, dirtyResultFileName));
                }

                await MergeFilesAsync(dirtyFilePaths, dirtyOutputPath, dirtyResultFileName, cancellationToken); // Объединение и сохранение

                await botClient.SendMessage(chatId, "Теперь отправьте \"чистые\" данные.", cancellationToken: default);
            }

            // Все Чистые данные получены
            else if (callbackQuery.Data == "clean_files_done" && _userStates[chatId] == UserState.WaitingForCleanData)
            {
                _userStates[chatId] = UserState.Idle; // Заканчиваем прием файлов и переходим к обработке

                // Мержим чистые файлы
                var cleanFilePaths = _userCleanFiles[chatId];  // Получаем пути всех чистых файлов
                var cleanOutputPath = dataPath/*"Data"*/;  // Путь для сохранения объединенного файла
                var cleanResultFileName = "Чистые данные.xlsx";  // Имя объединенного файла

                // Проверяем, существует ли файл, и если да, то удаляем его
                if (System.IO.File.Exists(Path.Combine(cleanOutputPath, cleanResultFileName)))
                {
                    System.IO.File.Delete(Path.Combine(cleanOutputPath, cleanResultFileName));
                }

                await MergeFilesAsync(cleanFilePaths, cleanOutputPath, cleanResultFileName, cancellationToken); // Объединение и сохранение

                await botClient.SendMessage(chatId, "Файлы получены. Начинаю обработку данных.", cancellationToken: default);

                _ = Task.Run(async () =>
                {
                    await StartProccessingFiles(callbackQuery, cancellationToken); // Запуск алгоритма
                });
            }

            // Удаляем сообщение с кнопкой после нажатия
            await botClient.EditMessageReplyMarkup(chatId, callbackQuery.Message.MessageId, replyMarkup: null);
        }

        private async Task StartProccessingFiles(CallbackQuery callbackQuery, CancellationToken cancellationToken) // Запуск обработки алгоритма 
        {
            var chatId = callbackQuery.Message!.Chat.Id; // Получаем Id чата

            // Проверяем состояние пользователя
            if (_userStates[chatId] != UserState.Idle)
                return;

            Log.Information($"Starting to process files for the user: \"{callbackQuery.From}\".");
            //logger.LogInformation($"Starting to process files for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

            // Запускаем алгоритм
            try
            {
                // Указываем пути к объединенным файлам
                var dirtyFilePath = Path.Combine(/*"Data"*/dataPath, "Грязные данные.xlsx");
                var cleanFilePath = Path.Combine(/*"Data"*/dataPath, "Чистые данные.xlsx");

                // Проверка существования файлов
                if (!System.IO.File.Exists(dirtyFilePath) || !System.IO.File.Exists(cleanFilePath))
                {
                    await botClient.SendMessage(chatId, "Не удалось найти загруженные файлы для обработки.", cancellationToken: cancellationToken);
                    return;
                }

                // Установка путей файлов в сервис обработки
                fileProcessingService.SaveDirtyFilePath(dirtyFilePath);
                fileProcessingService.SaveCleanFilePath(cleanFilePath);

                // Запуск обработки файлов
                var resultFilePath = string.Empty;
                using (LogContext.PushProperty("SenderInfo", callbackQuery.From))
                {
                    resultFilePath = await fileProcessingService.ProcessFilesAsync(cancellationToken);
                }
                
                Log.Information($"Files have been processed, sending the result to the user \"{callbackQuery.From}\".");
                //logger.LogInformation($"Files have been processed, sending the result to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");

                // Проверка размера файла и выбор способа отправки
                var fileInfo = !string.IsNullOrEmpty(resultFilePath) ? new FileInfo(resultFilePath) : throw new ArgumentException($"{resultFilePath} не должен быть пустым. Метод ProcessFilesAsync отработал некорректно.");
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
                    await botClient.SendDocument(chatId, inputFile, cancellationToken: cancellationToken);
                }

                // Очистка состояния и сброс данных для следующей операции
                _userStates[chatId] = UserState.Idle;
                _userDirtyFiles.Remove(chatId);
                _userCleanFiles.Remove(chatId);

                // Удаляем временные файл после обработки
                System.IO.File.Delete(dirtyFilePath);
                System.IO.File.Delete(cleanFilePath);
                System.IO.File.Delete(resultFilePath);

                Log.Information($"Result file has been sent to the user \"{callbackQuery.From}\".");
                //logger.LogInformation($"Result file has been sent to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");
            }
            catch (Exception ex)
            {
                Log.Error($"Error while processing files: {ex.Message}");
                //logger.LogError($"Error while processing files: {ex.Message}, time: {DateTimeOffset.Now}\n");
                await botClient.SendMessage(
                    chatId,
                    "Произошла ошибка при обработке файлов. Пожалуйста, попробуйте снова.",
                    cancellationToken: cancellationToken
                );
            }
        }

        private async Task SplitAndSendLargeFileAsync(string filePath, long chatId, CancellationToken cancellationToken) // Если файл слишком большой, то разделяем его 
        {
            // Настройки
            const long maxFileSize = 49 * 1024 * 1024; // 49MB
            int partNumber = 1;
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Пока файл не полностью отправлен
            while (fileStream.Position < fileStream.Length)
            {
                var partPath = $"{dataPath}/Result_Part{partNumber}.xlsx";

                // Создаем новую часть файла, пока она не достигнет лимита
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
                await botClient.SendDocument(
                    chatId: chatId,
                    document: new InputFileStream(sendStream, $"Result_Part{partNumber}.xlsx"),
                    cancellationToken: cancellationToken
                );

                partNumber++;
            }
        }

        private async Task MergeFilesAsync(IEnumerable<string> filePaths, string outputFilePath, string resultFileName, CancellationToken cancellationToken) //Объединение файлов 
        {
            try
            {
                // Используем IExcelMerger для объединения файлов
                IExcelMerger excelMerger = new NPOIMerger(); // Здесь можно внедрить через DI, если нужно.

                var fileList = filePaths.ToList(); // Преобразуем список в List<string> (если это необходимо)

                await excelMerger.MergeExcelFilesAsync(fileList, outputFilePath, resultFileName); // Выполняем объединение файлов

                Log.Information($"Files have been successfully merged and saved to: \"{Path.Combine(outputFilePath, resultFileName)}\".");
                //logger.LogInformation($"Files have been successfully merged and saved to: \"{Path.Combine(outputFilePath, resultFileName)}\", time: {DateTimeOffset.Now}\n");

                // Удаляем временные файлы после обработки
                foreach (var filePath in fileList)
                    System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Log.Error($"Error while merging files: {ex.Message}.");
                //logger.LogError($"Error while merging files: {ex.Message}, time: {DateTimeOffset.Now}\n");
            }
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Log.Information($"Unknown update type: {update.Type}.");
            //logger.LogInformation($"Unknown update type: {update.Type}, time: {DateTimeOffset.Now}");
            return Task.CompletedTask;
        }
    }
}
