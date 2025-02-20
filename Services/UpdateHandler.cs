using AbstractionsAndModels.Abstract;
using ExcelHandler.Interfaces;
using ExcelHandler.Mergers;
using Helicopters_Russia.Models;
using NPOI.HPSF;
using Serilog;
using Serilog.Context;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Helicopters_Russia.Services
{
    public class UpdateHandler
        (ITelegramBotClient botClient,
        FileProcessingService fileProcessingService,
        ProgressStrategy progressStrategy) : IUpdateHandler
    {
        private readonly string downloadDataPath = "Download Data";
        private readonly string dataPath = "Data";
        private ConcurrentDictionary<long, ConcurrentBag<(string FileId, string FileName)>> _userDirtyFiles = new(); // Список Грязных файлов для каждого пользователя
        private ConcurrentDictionary<long, ConcurrentBag<(string FileId, string FileName)>> _userCleanFiles = new(); // Список Чистых файлов для каждого пользователя
        private static readonly ConcurrentDictionary<long, Models.UserState> _userStates = new(); //Состояние пользователей

        // Основной обработчик бота, в целом закончен
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (!_userStates.ContainsKey(update.Message!.From!.Id)) // Для нового пользователя (в том числе после перезапуска бота) ставим состояние "ожидание"
            {
                Log.Information($"New user: \"{update.Message.From.ToString()}\"");
                _userStates[update.Message.From.Id] = UserState.NewUser;
            }

            //_userDirtyFiles[update.Message!.From!.Id] = new List<string> { "dirtyFile" };

            if (update.Message is not null) // Навигация зависящая от типа сообщения
            {
                await (update.Message.Type switch
                {
                    Telegram.Bot.Types.Enums.MessageType.Text => TextMessage(update, cancellationToken),
                    Telegram.Bot.Types.Enums.MessageType.Document => DocMessage(update, cancellationToken),
                    _ => Task.CompletedTask
                });

            }
        }

        private async Task TextMessage(Update update, CancellationToken cancellationToken)
        {
            await (update.Message!.Text switch
            {
                "/start" => CommandProccessing(update, cancellationToken),
                "📂 Начать обработку файлов" => CommandProccessing(update, cancellationToken),
                "✅ Все файлы отправлены" => CommandProccessing(update, cancellationToken),
                "☁️ Загрузить эталоны в базу данных" => CommandProccessing(update, cancellationToken),
                //"🔍 Статус алгоритма" => 
                //"ℹ Информация о формате отправляемых файлов" => ,
                //"❌ Отправлен неверный файл" => ,
                //"❌ Отмена" => ,
                //_ => обработчик случайных команд
                _ => Task.CompletedTask
            });
        }

        private async Task CommandProccessing(Update update, CancellationToken cancellationToken)
        {
            long userId = update.Message!.From!.Id;
            string command = update.Message!.Text!;
            string messageText = update.Message!.Text!;

            if (command == "/start")
            {
                if (_userStates[userId] == UserState.NewUser)
                {
                    _userStates[userId] = UserState.Idle;

                    const string usage = "Приветствуем Вас в Telegram боте для нормализации нормативно-справочной информации!";
                    await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                }

                await KeyboardCommandsInChat(update);
            }

            else if (command == "📂 Начать обработку файлов")
            {
                if (_userStates[userId] == UserState.Idle || _userStates[userId] == UserState.NewUser) // Пользователь первый раз нажимает "📂 Начать обработку файлов"
                {
                    _userStates[userId] = UserState.WaitingForDirtyData;
                    await CommandProccessing(update, cancellationToken);
                }
                else if (_userStates[userId] == UserState.WorkInProgress)
                {
                    const string usage = "Пожалуйста, дождитесь завершения сопоставления";
                    await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                }
                else
                {
                    await KeyboardCommandsInChat(update);
                }
            }

            else if (command == "✅ Все файлы отправлены")
            {
                if (_userStates[userId] == UserState.WaitingForDirtyData && _userDirtyFiles.TryGetValue(userId, out var dirtyFiles) && dirtyFiles.Any())
                {
                    _userStates[userId] = UserState.WaitingForCleanData;
                    await KeyboardCommandsInChat(update);
                }
                else if (_userStates[userId] == UserState.WaitingForCleanData && _userCleanFiles.TryGetValue(userId, out var cleanFiles) && cleanFiles.Any())
                {
                    var dirtyResultFileName = userId + "_dirtyFile.xlsx";  // Имя объединенного грязного файла
                    var cleanResultFileName = userId + "_cleanFile.xlsx";  // Имя объединенного чистого файла

                    // Проверяем, существует ли файл, и если да, то удаляем его
                    if (System.IO.File.Exists(Path.Combine(dataPath, dirtyResultFileName)))
                    {
                        System.IO.File.Delete(Path.Combine(dataPath, dirtyResultFileName));
                    }
                    try
                    {
                        await MergeFilesAsync(dirtyResultFileName, cleanResultFileName, update); // Объединение и сохранение
                        
                        // Запускаем алгоритм
                        try
                        {
                            // Указываем пути к объединенным файлам
                            var dirtyFilePath = Path.Combine(dataPath, dirtyResultFileName);
                            var cleanFilePath = Path.Combine(dataPath, cleanResultFileName);

                            // Проверка существования файлов
                            if (!System.IO.File.Exists(dirtyFilePath) || !System.IO.File.Exists(cleanFilePath))
                            {
                                const string usage = "Не удалось найти загруженные файлы для обработки.";
                                await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                                return;
                            }
                            // Установка путей файлов в сервис обработки
                            fileProcessingService.SaveDirtyFilePath(dirtyFilePath);
                            fileProcessingService.SaveCleanFilePath(cleanFilePath);

                            _userStates[userId] = UserState.WorkInProgress;

                            // Запуск обработки файлов
                            var resultFilePath = string.Empty;

                            resultFilePath = await fileProcessingService.ProcessFilesAsync(cancellationToken);

                            //Log.Information($"Files have been processed, sending the result to the user \"{callbackQuery.From}\".");
                            //logger.LogInformation($"Files have been processed, sending the result to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");

                            // Проверка размера файла и выбор способа отправки
                            var fileInfo = !string.IsNullOrEmpty(resultFilePath) ? new FileInfo(resultFilePath) : throw new ArgumentException($"{resultFilePath} не должен быть пустым. Метод ProcessFilesAsync отработал некорректно.");
                            if (fileInfo.Length > 49 * 1024 * 1024)
                            {
                                // Если файл слишком большой, разбить и отправить по частям
                                await SplitAndSendLargeFileAsync(resultFilePath, userId, cancellationToken);
                            }
                            else
                            {
                                // Отправить файл целиком
                                await using var resultStream = System.IO.File.OpenRead(resultFilePath);
                                var inputFile = new InputFileStream(resultStream, "Result.xlsx");
                                await botClient.SendDocument(userId, inputFile, cancellationToken: cancellationToken);
                            }

                            // Очистка состояния и сброс данных для следующей операции
                            _userStates[userId] = UserState.Idle;

                            // Удаляем временные файл после обработки
                            System.IO.File.Delete(dirtyFilePath);
                            System.IO.File.Delete(cleanFilePath);
                            System.IO.File.Delete(resultFilePath);

                            //Log.Information($"Result file has been sent to the user \"{callbackQuery.From}\".");
                            //logger.LogInformation($"Result file has been sent to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error while processing files: {ex.Message}");
                            //logger.LogError($"Error while processing files: {ex.Message}, time: {DateTimeOffset.Now}\n");
                            await botClient.SendMessage(
                                userId,
                                "Произошла ошибка при обработке файлов. Пожалуйста, попробуйте снова.",
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    catch
                    {
                        const string usage = "Что-то не так";
                        await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                        return;
                    }
                }
                else
                {
                    const string usage = "Что-то не так";
                    await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                    return;
                }
            }

            else if (command == "☁️ Загрузить эталоны в базу данных") // БД
            {

            }
        }

        // Вывод кнопок и инфы (не закончено)
        private async Task KeyboardCommandsInChat(Update update) 
        {
            Task task = _userStates[update.Message!.From!.Id] switch
            {
                UserState.Idle => botClient.SendMessage(
                    chatId: update.Message.Chat,
                    text: "Выберите действие:",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "📂 Начать обработку файлов", "ℹ Информация о формате отправляемых файлов" },
                        new KeyboardButton[] { "☁️ Загрузить эталоны в базу данных", "❌ Отмена" }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                        InputFieldPlaceholder = "Выберите действие"
                    }),
                // Пользователь уже отправлял "грязные" файлы и алгоритм ждет еще
                UserState.WaitingForDirtyData when _userDirtyFiles.TryGetValue(update.Message!.From!.Id, out var dirtyFiles) && dirtyFiles.Any() => botClient.SendMessage(
                    chatId: update.Message.Chat,
                    text: "Пожалуйста, отправьте еще \"грязные\" данные, если требуется.\nИли нажмите кнопку \"✅ Все файлы отправлены\". ",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "✅ Все файлы отправлены",  "ℹ Информация о формате отправляемых файлов"},
                        new KeyboardButton[] { "❌ Отправлен неверный файл", "❌ Отмена" }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                        InputFieldPlaceholder = "Выберите действие или отправьте документ"
                    }),
                // Пользователь не отправлял "грязные" файлы
                UserState.WaitingForDirtyData when !_userDirtyFiles.ContainsKey(update.Message!.From!.Id) || (_userDirtyFiles.TryGetValue(update.Message!.From!.Id, out var dirtyFiles) && !dirtyFiles.Any()) => botClient.SendMessage(
                    chatId: update.Message.Chat,
                    text: "Пожалуйста, отправьте \"грязные\" данные.",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "ℹ Информация о формате отправляемых файлов", "❌ Отмена"}
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                        InputFieldPlaceholder = "Выберите действие или отправьте документ"
                    }),
                // Пользователь уже отправлял "чистые" файлы и алгоритм ждет еще
                UserState.WaitingForCleanData when _userCleanFiles.TryGetValue(update.Message!.From!.Id, out var cleanFiles) && cleanFiles.Any() => botClient.SendMessage(
                    chatId: update.Message.Chat,
                    text: "Пожалуйста, отправьте еще \"чистые\" данные, если требуется.\nИли нажмите кнопку \"✅ Все файлы отправлены\". ",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                                        new KeyboardButton[] { "✅ Все файлы отправлены",  "ℹ Информация о формате отправляемых файлов"},
                                        new KeyboardButton[] { "❌ Отправлен неверный файл", "❌ Отмена" }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                        InputFieldPlaceholder = "Выберите действие или отправьте документ"
                    }),
                // Пользователь не отправлял "чистые" файлы
                UserState.WaitingForCleanData when !_userCleanFiles.ContainsKey(update.Message!.From!.Id) || (_userCleanFiles.TryGetValue(update.Message!.From!.Id, out var cleanFiles) && !cleanFiles.Any()) => botClient.SendMessage(
                    chatId: update.Message.Chat,
                    text: "Пожалуйста, отправьте \"чистые\" данные.",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "ℹ Информация о формате отправляемых файлов", "❌ Отмена"}
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true,
                        InputFieldPlaceholder = "Выберите действие или отправьте документ"
                    }),


                //сделать для других состояний
            };

            await task;
        }

        // Метод для принятие ботом файлов (не закончен)
        private async Task DocMessage(Update update, CancellationToken cancellationToken)
        {
            long userID = update.Message!.From!.Id;

            switch (_userStates[userID])
            {
                case UserState.Idle:
                    const string usage = "Файл не принят. Сначала нужно нажать кнопку \"📂 Начать обработку файлов\"";

                    await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());

                    await KeyboardCommandsInChat(update);
                    return;
                case UserState.WaitingForDirtyData:
                    await ProcessFile(update, _userDirtyFiles, "dirty", cancellationToken,
                        "Пожалуйста, убедитесь, что Вы не отправляете файл, который уже отправляли и поменяйте название." +
                        "\nЛибо, если больше не требуется отправлять \"грязные\" данные, нажмите кнопку \"✅ Все файлы отправлены\"");
                    return;
                case UserState.WaitingForCleanData:
                    await ProcessFile(update, _userCleanFiles, "clean", cancellationToken,
                        "Пожалуйста, убедитесь, что Вы не отправляете файл, который уже отправляли и поменяйте название." +
                        "\nЛибо, если больше не требуется отправлять \"чистые\" данные, нажмите кнопку \"✅ Все файлы отправлены\"");
                    return;
                default:
                    return;
            }
        }

        // Метод для обработки загрузки файлов (грязных и чистых). (закончен, добавить логи)
        private async Task ProcessFile(Update update, ConcurrentDictionary<long, ConcurrentBag<(string, string)>> fileStorage, string documentClass, CancellationToken cancellationToken,string warningMessage)
        {
            long userId = update.Message!.From!.Id; // ID пользователя
            long chatId = update.Message.Chat.Id; // ID чата
            string fileId = update.Message.Document!.FileId!; // ID файла
            string fileName = update.Message.Document!.FileName!; // Имя файла
            var receivedFile = update.Message.Document; // Получаем файл
            var fileExtension = Path.GetExtension(receivedFile.FileName); // Вытягиваем расширение файла

            // Проверяем, не был ли этот файл отправлен ранее
            if (fileStorage.TryGetValue(userId, out var files))
            {
                foreach (var (existingFileId, existingFileName) in files)
                {
                    if (existingFileName == fileName)
                    {
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: warningMessage,
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "✅ Все файлы отправлены", "ℹ Информация о формате отправляемых файлов" },
                                new KeyboardButton[] { "❌ Отмена" }
                            })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = true,
                                InputFieldPlaceholder = "Выберите действие или отправьте документ"
                            });
                        return; // Останавливаем выполнение, если файл уже отправляли
                    }
                }
            }

            var downloadFilePath = Path.Combine(downloadDataPath, fileId + fileExtension); // Создаем путь, куда будем скачивать файл
            Telegram.Bot.Types.File downloadedFile = null; // Инициализируем переменную
            try
            {
                downloadedFile = await botClient.GetFile(fileId, cancellationToken); // Скачиваем файл
                // Сохраняем его в папку DownloadData
                await using (var fileStream = new FileStream(downloadFilePath, FileMode.Create)) 
                {
                    await botClient.DownloadFile(downloadedFile.FilePath!, fileStream, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(update.Message.Chat, ex.Message, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            // Добавляем новый файл в хранилище
            fileStorage.AddOrUpdate(userId,
                new ConcurrentBag<(string, string)> { (fileId, fileName) },
                (key, existingList) =>
                {
                    existingList.Add((fileId, fileName));
                    return existingList;
                });

            const string usage = "Файл принят!";

            await botClient.SendMessage(update.Message.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());

            // Отправляем обновленный список клавиатуры
            await KeyboardCommandsInChat(update);
        }

        // Метод для объединения файлов (в целом закончен, добавить логи)
        private async Task MergeFilesAsync(string dirtyResultFileName, string cleanResultFileName, Update update) 
        {
            long userId = update.Message!.From!.Id;
            try
            {
                // Используем IExcelMerger для объединения файлов
                IExcelMerger excelMerger = new NPOIMerger(); // Здесь можно внедрить через DI, если нужно.

                // Получаем список FileId для грязных файлов пользователя с добавлением .xlsx
                if (_userDirtyFiles.TryGetValue(userId, out var dirtyFiles) && _userCleanFiles.TryGetValue(userId, out var cleanFiles))
                {
                    var dirtyFileIds = dirtyFiles.Select(file => downloadDataPath + "/" + file.FileId + ".xlsx").ToList();
                    try
                    {
                        await excelMerger.MergeExcelFilesAsync(dirtyFileIds, dataPath, dirtyResultFileName);
                        foreach (var filePath in dirtyFileIds)
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _userDirtyFiles[userId] = new ConcurrentBag<(string FileId, string FileName)>();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex.Message);
                    }

                    var cleanFileIds = cleanFiles.Select(file => downloadDataPath + "/" + file.FileId + ".xlsx").ToList();
                    try
                    {
                        await excelMerger.MergeExcelFilesAsync(cleanFileIds, dataPath, cleanResultFileName);
                        foreach (var filePath in cleanFileIds)
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _userCleanFiles[userId] = new ConcurrentBag<(string FileId, string FileName)>();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex.Message);
                    }

                    // Логирование успешного завершения
                    Log.Information($"Files have been successfully merged and saved to: \"{dirtyResultFileName}\".");
                }
                else
                {
                    Log.Warning($"No dirty files found for user with ID: {userId}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error while merging files: {ex.Message}.");
            }
        }

        // Обработка файла на выходе. Если слишком большой, то разделяем и отправляем частями (в целом закончен, добавить логи, протестировать)
        private async Task SplitAndSendLargeFileAsync(string filePath, long chatId, CancellationToken cancellationToken) 
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

        // Общие ошибки бота (не закончен)
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception ex, HandleErrorSource source, CancellationToken cancellationToken) // Вывод ошибок бота
        {
            Log.Fatal(ex.Message);
            await Task.CompletedTask;
        }
    }
}


        //    private readonly string downloadDataPath = "Download Data";
        //    private readonly string dataPath = "Data";
        //    private Dictionary<long, List<string>> _userDirtyFiles = new(); // Список Грязных файлов для каждого пользователя
        //    private Dictionary<long, List<string>> _userDirtyNameFiles = new(); // Список названий Грязных файлов для каждого пользователя для исключения повторений
        //    private Dictionary<long, List<string>> _userCleanFiles = new(); // Список Чистых файлов для каждого пользователя
        //    private Dictionary<long, List<string>> _userCleanNameFiles = new(); // Список названий Грязных файлов для каждого пользователя для исключения повторений
        //    private static readonly ConcurrentDictionary<long, Models.UserState> _userStates = new(); //Состояние пользователей


        //    async Task<Message> UnknownCommand(Message msg, Update update) //Обработка неизвестной команды 
        //    {
        //        Log.Warning($"Receive unknown command\n\t\tcommand: \"{msg.Text}\", type: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\".");
        //        //logger.LogInformation($"Receive unknown command\n\t\tcommand: \"{msg.Text}\", type: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //        const string usage = """
        //            Вы ввели неизвестную команду
        //         """;

        //        await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());

        //        return await Usage(msg);
        //    }

        //    async Task<Message> InvalidUserState(Message msg, Update update) // Обработка ошибки состояния пользователя
        //    {
        //        Log.Information($"Data received from user prior to command invocation\n\t\tData: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\".");
        //        //logger.LogInformation($"Data received from user prior to command invocation\n\t\tData: \"{update.Message!.Type}\" with id: \"{msg.MessageId}\" from: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //        const string usage = """
        //            Для обработки данных сначала нужно вызвать команду
        //            /proccesing_start  - Начать обработку файлов
        //         """;

        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    async Task<Message> FileWithTheSameName(Message msg, Update update) // Обработка ошибки отправки того же файла
        //    {
        //        Log.Information($"The user has sent a file that they have sent before\n\t\tData: \"{update.Message!.Type}\" file name: \"{msg.MessageId}\" from: \"{update.Message.From}\".");

        //        const string usage = """
        //            Был получен файл с тем же названием как у уже полученного.
        //            Пожалуйста, поменяйте название файла и отправьте еще раз.
        //         """;

        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    async Task<Message> FileIsTooBig(Message msg, Update update, Exception ex) // Обработка ошибки отправки файла весом более 20 МБ
        //    {
        //        Log.Information($"The user tried to upload a file that was too large. User: \"{update.Message.From}\" Error:\n \"{ex}\".");

        //        const string usage = """
        //            На момент написания бота Telegram позволяет боту скачивать файл размером до 20 МБ.
        //            Пожалуйста, разделите ваш файл на два и, если потребуется, более и отправьте поочередно.
        //            Примечание - из первого отправленного файла первая строка будет удалена, в последующих файлах она остается. После отправки файла дождитесь ответной реакции, а после отправляйте следующие.
        //         """;

        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    async Task<Message> UnknownErrorWhenDownloadingFile(Message msg, Update update, Exception ex) // Обработка ошибки отправки того же файла
        //    {
        //        Log.Information($"An unhandled error was encountered while downloading the file. User: \"{update.Message.From}\" Error:\n \"{ex}\".");

        //        const string usage = """
        //            Возникла неизвестная ошибка.
        //            Пожалуйста, проверьте размер файла, попробуйте поменять название и отправьте повторно. Обязательно свяжитесь с разработчиками для устранения подобного в будущем.
        //         """;

        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    async Task<Message> NotAnExcelFileSent(Message msg, Update update) // Обработка ошибки отправки не Excel файла
        //    {
        //        Log.Information($"The user did not send an Excel file. User: \"{update.Message.From}\" File type: \"{update.Message.Document.MimeType}\".");

        //        const string usage = """
        //            Вы отправили файл, который не является файлом Excel.
        //            Пожалуйста, проверьте правильный ли файл Вы отправляете. Если не выходит, измените расширение файла на .xlsx, отправьте и сообщите разработчикам.
        //         """;

        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    async Task<Message> Usage(Message msg) //Вывод команд 
        //    {
        //        const string usage = """
        //             <b><u>Bot menu</u></b>:
        //             /proccesing_start  - Начать обработку файлов 
        //             /status - отобразить статус работы алгоритма
        //         """;
        //        return await botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        //    }

        //    private async Task GetStatus(Update update)
        //    {
        //        var chatId = update.Message!.Chat.Id;
        //        Log.Information($"The \"GetStatus\" method was called from the user: \"{update.Message.From}\".");
        //        //logger.LogInformation($"The \"GetStatus\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");
        //        var progress = progressStrategy.GetCurrentProgress();
        //        await botClient.SendMessage(
        //            chatId,
        //            $"Step: {progress.Step}, Progress: {progress.CurrentProgress}%",
        //            cancellationToken: default
        //        );
        //    }

        //    private async Task StartProccessing(Update update) //Начало обработки пользователя 
        //    {
        //        var chatId = update.Message!.Chat.Id;
        //        Log.Information($"The \"StartProccessingFiles\" method was called from the user: \"{update.Message.From}\".");
        //        //logger.LogInformation($"The \"StartProccessingFiles\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //        if (!_userStates.TryGetValue(chatId, out var userState) || userState == UserState.Idle)
        //        {
        //            _userStates[chatId] = UserState.WaitingForDirtyData;
        //            _userCleanFiles[chatId] = new List<string>();
        //            _userCleanNameFiles[chatId] = new List<string>();
        //            _userDirtyFiles[chatId] = new List<string>();
        //            _userDirtyNameFiles[chatId] = new List<string>();

        //            await botClient.SendMessage(
        //                chatId,
        //                "Пожалуйста, отправьте \"грязные\" данные.",
        //                cancellationToken: default
        //            );
        //        }
        //        else if (_userStates.TryGetValue(chatId, out userState) && (userState == UserState.WaitingForCleanData || userState == UserState.WaitingForDirtyData))
        //        {
        //            if (userState == UserState.WaitingForCleanData)
        //                await botClient.SendMessage(
        //                    chatId,
        //                    "Пожалуйста, отправьте \"чистые\" данные.",
        //                    cancellationToken: default
        //                );
        //            else if (userState == UserState.WaitingForDirtyData)
        //                await botClient.SendMessage(
        //                    chatId,
        //                    "Пожалуйста, отправьте \"грязные\" данные.",
        //                    cancellationToken: default
        //                );
        //        }
        //    }

        //    private async Task HandleDocumentUpload(Update update, CancellationToken cancellationToken) // Получение файлов 
        //    {
        //        var chatId = update.Message!.Chat.Id; // Получаем Id чата

        //        // Проверяем состояние пользователя
        //        if (!_userStates.TryGetValue(chatId, out var userState))
        //        {
        //            await InvalidUserState(update.Message, update);
        //            return;
        //        }


        //        var document = update.Message.Document; // Получаем документ
        //        var fileId = document!.FileId; // Получаем его Id
        //        var fileExtension = Path.GetExtension(document.FileName); // Получаем расширение файла
        //        var fileName = document.FileName; // Получаем имя файла

        //        if (document.MimeType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //        {
        //            await NotAnExcelFileSent(update.Message, update);
        //            return;
        //        }

        //        if (userState == UserState.WaitingForDirtyData)
        //        {
        //            foreach (var filename in _userDirtyNameFiles[chatId])
        //            {
        //                if (filename == fileName)
        //                {
        //                    await FileWithTheSameName(update.Message, update);
        //                    return;
        //                }
        //            }
        //            // Сохраняем имя файла (не fileId) в список файлов
        //            if (!_userDirtyNameFiles.ContainsKey(chatId))
        //                _userDirtyNameFiles[chatId] = new List<string>();

        //            _userDirtyNameFiles[chatId].Add(fileName);
        //        }

        //        if (userState == UserState.WaitingForCleanData)
        //        {
        //            foreach (var filename in _userCleanNameFiles[chatId])
        //            {
        //                if (filename == fileName)
        //                {
        //                    await FileWithTheSameName(update.Message, update);
        //                    return;
        //                }
        //            }
        //            // Сохраняем имя файла (не fileId) в список файлов
        //            if (!_userCleanNameFiles.ContainsKey(chatId))
        //                _userCleanNameFiles[chatId] = new List<string>();

        //            _userCleanNameFiles[chatId].Add(fileName);
        //        }

        //        //var fileName = document.FileName ?? fileId; // используем имя файла, если оно есть, иначе - его id
        //        var filePath = Path.Combine(/*"Download data"*/downloadDataPath, fileId + fileExtension);

        //        Telegram.Bot.Types.File file = null;

        //        try
        //        {
        //            file = await botClient.GetFile(document.FileId, cancellationToken); // Загружаем документ
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex.Message.Contains("Bad Request: file is too big"))
        //                await FileIsTooBig(update.Message, update, ex);
        //            else
        //                await UnknownErrorWhenDownloadingFile(update.Message, update, ex);
        //            return;
        //        }

        //        Log.Information($"The \"HandleDocumentUpload\" method was called from the user: \"{update.Message.From}\".");
        //        //logger.LogInformation($"The \"HandleDocumentUpload\" method was called from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //        if (userState == UserState.WaitingForDirtyData)
        //        {
        //            Log.Information($"Added file of \"Dirty\" data from the user: \"{update.Message.From}\".");
        //            //logger.LogInformation($"Added file of \"Dirty\" data from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //            // Сохраняем путь к файлу (не fileId) в список "грязных" файлов
        //            if (!_userDirtyFiles.ContainsKey(chatId))
        //                _userDirtyFiles[chatId] = new List<string>();


        //            _userDirtyFiles[chatId].Add(filePath);

        //            // Сохраняем файл временно
        //            await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await botClient.DownloadFile(file!.FilePath!, fileStream, cancellationToken);
        //            }

        //            // Inline кнопки
        //            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //            {
        //                new[]
        //                {
        //                    InlineKeyboardButton.WithCallbackData("Отправлены неправильные \"Грязные\" данные", "incorrect_dirty_data_sent") // кнопка для отмены отправки файлов
        //                },
        //                new[]
        //                {
        //                    InlineKeyboardButton.WithCallbackData("Все \"Грязные\" файлы отправлены", "dirty_files_done") // подтверждение отправки
        //                }
        //            });

        //            await botClient.SendMessage(
        //                chatId,
        //                "Файл принят.\n    Добавьте еще файлы или нажмите \"Все \"Грязные\" файлы отправлены\".\n    Если был отправлен неправильный файл, нажмите \"Отправлены неправильные \"Грязные\" данные\".",
        //                replyMarkup: inlineKeyboard,
        //                cancellationToken: default
        //            );
        //        }

        //        else if (userState == UserState.WaitingForCleanData)
        //        {
        //            Log.Information($"Added file of \"Clean\" data from the user: \"{update.Message.From}\".");
        //            //logger.LogInformation($"Added file of \"Clean\" data from the user: \"{update.Message.From}\", time: {DateTimeOffset.Now}\n");

        //            // Сохраняем путь к файлу (не fileId) в список "чистых" файлов
        //            if (!_userCleanFiles.ContainsKey(chatId))
        //                _userCleanFiles[chatId] = new List<string>();

        //            _userCleanFiles[chatId].Add(filePath);

        //            // Сохраняем файл временно
        //            await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await botClient.DownloadFile(file.FilePath!, fileStream, cancellationToken);
        //            }

        //            // Inline кнопки
        //            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //            {
        //                new[]
        //                {
        //                    InlineKeyboardButton.WithCallbackData("Отправлены неправильные \"Чистые\" данные", "incorrect_clear_data_sent") // кнопка для отмены отправки файлов
        //                },
        //                new[]
        //                {
        //                    InlineKeyboardButton.WithCallbackData("Все \"Чистые\" файлы отправлены", "clean_files_done") // подтверждение отправки
        //                }
        //            });

        //            await botClient.SendMessage(
        //                chatId,
        //                "Файл принят.\n    Добавьте еще файлы или нажмите \"Все \"Чистые\" файлы отправлен\".\n    Если был отправлен неправильный файл, нажмите \"Отправлены неправильные \"Чистые\" данные\".",
        //                replyMarkup: inlineKeyboard,
        //                cancellationToken: default
        //            );
        //        }
        //    }

        //    public async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken) // Ответы на Inline кнопки 
        //    {
        //        var chatId = callbackQuery.Message!.Chat.Id;

        //        // Проверяем состояние пользователя
        //        if (!_userStates.TryGetValue(chatId, out var userState))
        //        {
        //            return;
        //        }

        //        // Удаление Чистых файлов 
        //        if (callbackQuery.Data == "incorrect_clear_data_sent")
        //        {
        //            var cleanFilePaths = _userCleanFiles[chatId]; // Получаем пути всех чистых файлов

        //            // Удаляем файлы
        //            foreach (var file in cleanFilePaths)
        //            {
        //                try
        //                {
        //                    if (System.IO.File.Exists(file))
        //                    {
        //                        System.IO.File.Delete(file);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error($"Error deleting uploaded clean files: {ex.Message}");
        //                    //logger.LogError($"Error deleting uploaded clean files: {ex.Message}, time: {DateTimeOffset.Now}\n");
        //                }
        //            }

        //            _userCleanFiles[chatId].Clear(); // Удаляем сохранения файлов для пользователя

        //            Log.Information($"Deleted \"Clean\" data for the user: \"{callbackQuery.From}\".");
        //            //logger.LogInformation($"Deleted \"Clean\" data for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

        //            await botClient.SendMessage(
        //                chatId,
        //                "\"Чистые\" файлы были удалены. Можете загрузить их заново",
        //                cancellationToken: cancellationToken
        //            );
        //        }

        //        // Удаление Грязных данных
        //        else if (callbackQuery.Data == "incorrect_dirty_data_sent")
        //        {
        //            var dirtyFilePaths = _userDirtyFiles[chatId]; // Получаем пути всех чистых файлов

        //            // Удаляем файлы
        //            foreach (var file in dirtyFilePaths)
        //            {
        //                try
        //                {
        //                    if (System.IO.File.Exists(file))
        //                    {
        //                        System.IO.File.Delete(file);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error($"Error deleting uploaded dirty files: {ex.Message}");
        //                    //logger.LogError($"Error deleting uploaded dirty files: {ex.Message}, time: {DateTimeOffset.Now}");

        //                    //await botClient.SendTextMessageAsync(
        //                    //    chatId,
        //                    //    "Произошла ошибка при удалении файлов.",
        //                    //    cancellationToken: cancellationToken
        //                    //);
        //                }
        //            }

        //            _userDirtyFiles[chatId].Clear(); // Удаляем сохранения файлов для пользователя

        //            Log.Information($"Deleted \"Dirty\" data for the user: \"{callbackQuery.From}\".");
        //            //logger.LogInformation($"Deleted \"Dirty\" data for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

        //            await botClient.SendMessage(
        //                chatId,
        //                "\"Грязные\" файлы были удалены. Можете загрузить их заново",
        //                cancellationToken: cancellationToken
        //            );
        //        }

        //        // Все грязные данные получены
        //        else if (callbackQuery.Data == "dirty_files_done" && _userStates[chatId] == UserState.WaitingForDirtyData)
        //        {
        //            _userStates[chatId] = UserState.WaitingForCleanData; // Переходим к ожиданию "чистых" данных

        //            // Мержим грязные файлы
        //            var dirtyFilePaths = _userDirtyFiles[chatId];  // Получаем пути всех грязных файлов
        //            var dirtyOutputPath = dataPath/*"Data"*/;  // Путь для сохранения объединенного файла
        //            var dirtyResultFileName = "Грязные данные.xlsx";  // Имя объединенного файла

        //            // Проверяем, существует ли файл, и если да, то удаляем его
        //            if (System.IO.File.Exists(Path.Combine(dirtyOutputPath, dirtyResultFileName)))
        //            {
        //                System.IO.File.Delete(Path.Combine(dirtyOutputPath, dirtyResultFileName));
        //            }

        //            await MergeFilesAsync(dirtyFilePaths, dirtyOutputPath, dirtyResultFileName, cancellationToken); // Объединение и сохранение

        //            await botClient.SendMessage(chatId, "Теперь отправьте \"чистые\" данные.", cancellationToken: default);
        //        }

        //        // Все Чистые данные получены
        //        else if (callbackQuery.Data == "clean_files_done" && _userStates[chatId] == UserState.WaitingForCleanData)
        //        {
        //            _userStates[chatId] = UserState.Idle; // Заканчиваем прием файлов и переходим к обработке

        //            // Мержим чистые файлы
        //            var cleanFilePaths = _userCleanFiles[chatId];  // Получаем пути всех чистых файлов
        //            var cleanOutputPath = dataPath/*"Data"*/;  // Путь для сохранения объединенного файла
        //            var cleanResultFileName = "Чистые данные.xlsx";  // Имя объединенного файла

        //            // Проверяем, существует ли файл, и если да, то удаляем его
        //            if (System.IO.File.Exists(Path.Combine(cleanOutputPath, cleanResultFileName)))
        //            {
        //                System.IO.File.Delete(Path.Combine(cleanOutputPath, cleanResultFileName));
        //            }

        //            await MergeFilesAsync(cleanFilePaths, cleanOutputPath, cleanResultFileName, cancellationToken); // Объединение и сохранение

        //            await botClient.SendMessage(chatId, "Файлы получены. Начинаю обработку данных.", cancellationToken: default);

        //            _ = Task.Run(async () =>
        //            {
        //                await StartProccessingFiles(callbackQuery, cancellationToken); // Запуск алгоритма
        //            });
        //        }

        //        // Удаляем сообщение с кнопкой после нажатия
        //        await botClient.EditMessageReplyMarkup(chatId, callbackQuery.Message.MessageId, replyMarkup: null);
        //    }

        //    private async Task StartProccessingFiles(CallbackQuery callbackQuery, CancellationToken cancellationToken) // Запуск обработки алгоритма 
        //    {
        //        var chatId = callbackQuery.Message!.Chat.Id; // Получаем Id чата

        //        // Проверяем состояние пользователя
        //        if (_userStates[chatId] != UserState.Idle)
        //            return;

        //        Log.Information($"Starting to process files for the user: \"{callbackQuery.From}\".");
        //        //logger.LogInformation($"Starting to process files for the user: \"{callbackQuery.From}\", time: {DateTimeOffset.Now}");

        //        // Запускаем алгоритм
        //        try
        //        {
        //            // Указываем пути к объединенным файлам
        //            var dirtyFilePath = Path.Combine(/*"Data"*/dataPath, "Грязные данные.xlsx");
        //            var cleanFilePath = Path.Combine(/*"Data"*/dataPath, "Чистые данные.xlsx");

        //            // Проверка существования файлов
        //            if (!System.IO.File.Exists(dirtyFilePath) || !System.IO.File.Exists(cleanFilePath))
        //            {
        //                await botClient.SendMessage(chatId, "Не удалось найти загруженные файлы для обработки.", cancellationToken: cancellationToken);
        //                return;
        //            }

        //            // Установка путей файлов в сервис обработки
        //            fileProcessingService.SaveDirtyFilePath(dirtyFilePath);
        //            fileProcessingService.SaveCleanFilePath(cleanFilePath);

        //            // Запуск обработки файлов
        //            var resultFilePath = string.Empty;
        //            using (LogContext.PushProperty("SenderInfo", callbackQuery.From))
        //            {
        //                resultFilePath = await fileProcessingService.ProcessFilesAsync(cancellationToken);
        //            }

        //            Log.Information($"Files have been processed, sending the result to the user \"{callbackQuery.From}\".");
        //            //logger.LogInformation($"Files have been processed, sending the result to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");

        //            // Проверка размера файла и выбор способа отправки
        //            var fileInfo = !string.IsNullOrEmpty(resultFilePath) ? new FileInfo(resultFilePath) : throw new ArgumentException($"{resultFilePath} не должен быть пустым. Метод ProcessFilesAsync отработал некорректно.");
        //            if (fileInfo.Length > 49 * 1024 * 1024)
        //            {
        //                // Если файл слишком большой, разбить и отправить по частям
        //                await SplitAndSendLargeFileAsync(resultFilePath, chatId, cancellationToken);
        //            }
        //            else
        //            {
        //                // Отправить файл целиком
        //                await using var resultStream = System.IO.File.OpenRead(resultFilePath);
        //                var inputFile = new InputFileStream(resultStream, "Result.xlsx");
        //                await botClient.SendDocument(chatId, inputFile, cancellationToken: cancellationToken);
        //            }

        //            // Очистка состояния и сброс данных для следующей операции
        //            _userStates[chatId] = UserState.Idle;
        //            _userDirtyFiles.Remove(chatId);
        //            _userCleanFiles.Remove(chatId);

        //            // Удаляем временные файл после обработки
        //            System.IO.File.Delete(dirtyFilePath);
        //            System.IO.File.Delete(cleanFilePath);
        //            System.IO.File.Delete(resultFilePath);

        //            Log.Information($"Result file has been sent to the user \"{callbackQuery.From}\".");
        //            //logger.LogInformation($"Result file has been sent to the user \"{callbackQuery.From}\", time: {DateTimeOffset.Now}\n");
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error($"Error while processing files: {ex.Message}");
        //            //logger.LogError($"Error while processing files: {ex.Message}, time: {DateTimeOffset.Now}\n");
        //            await botClient.SendMessage(
        //                chatId,
        //                "Произошла ошибка при обработке файлов. Пожалуйста, попробуйте снова.",
        //                cancellationToken: cancellationToken
        //            );
        //        }
        //    }

        //    private async Task SplitAndSendLargeFileAsync(string filePath, long chatId, CancellationToken cancellationToken) // Если файл слишком большой, то разделяем его 
        //    {
        //        // Настройки
        //        const long maxFileSize = 49 * 1024 * 1024; // 49MB
        //        int partNumber = 1;
        //        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        //        // Пока файл не полностью отправлен
        //        while (fileStream.Position < fileStream.Length)
        //        {
        //            var partPath = $"{dataPath}/Result_Part{partNumber}.xlsx";

        //            // Создаем новую часть файла, пока она не достигнет лимита
        //            using var partStream = new FileStream(partPath, FileMode.Create, FileAccess.Write);

        //            int bytesRead;
        //            byte[] buffer = new byte[1024 * 1024]; // Буфер 1 МБ
        //            long partSize = 0;

        //            while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0 && partSize < maxFileSize)
        //            {
        //                await partStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        //                partSize += bytesRead;
        //            }

        //            // Отправка текущей части пользователю
        //            await using var sendStream = new FileStream(partPath, FileMode.Open, FileAccess.Read);
        //            await botClient.SendDocument(
        //                chatId: chatId,
        //                document: new InputFileStream(sendStream, $"Result_Part{partNumber}.xlsx"),
        //                cancellationToken: cancellationToken
        //            );

        //            partNumber++;
        //        }
        //    }

        //    private async Task MergeFilesAsync(IEnumerable<string> filePaths, string outputFilePath, string resultFileName, CancellationToken cancellationToken) //Объединение файлов 
        //    {
        //        try
        //        {
        //            // Используем IExcelMerger для объединения файлов
        //            IExcelMerger excelMerger = new NPOIMerger(); // Здесь можно внедрить через DI, если нужно.

        //            var fileList = filePaths.ToList(); // Преобразуем список в List<string> (если это необходимо)

        //            await excelMerger.MergeExcelFilesAsync(fileList, outputFilePath, resultFileName); // Выполняем объединение файлов

        //            Log.Information($"Files have been successfully merged and saved to: \"{Path.Combine(outputFilePath, resultFileName)}\".");
        //            //logger.LogInformation($"Files have been successfully merged and saved to: \"{Path.Combine(outputFilePath, resultFileName)}\", time: {DateTimeOffset.Now}\n");

        //            // Удаляем временные файлы после обработки
        //            foreach (var filePath in fileList)
        //                System.IO.File.Delete(filePath);
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error($"Error while merging files: {ex.Message}.");
        //            //logger.LogError($"Error while merging files: {ex.Message}, time: {DateTimeOffset.Now}\n");
        //        }
        //    }
