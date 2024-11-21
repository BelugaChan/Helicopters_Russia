using Algo.Factory;
using Algo.Interfaces;
using Algo.Models;
using ExcelHandler.Interfaces;
using Helicopters_Russia.Interfaces;
using Helicopters_Russia.Services;

public class FileProcessingService
{
    private readonly ISimilarityCalculator _similarityCalculator;
    private readonly IHandle _handle;
    private readonly IExcelReader _excelReader;
    private readonly IExcelWriter _excelWriter;
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IStandartService _standartService;

    private string? _dirtyFilePath;
    private string? _cleanFilePath;

    public FileProcessingService(ISimilarityCalculator similarityCalculator, IExcelReader excelReader, IExcelWriter excelWriter, ILogger<FileProcessingService> logger, IHandle handle, IStandartService standartService)
    {
        _similarityCalculator = similarityCalculator;
        _excelReader = excelReader;
        _excelWriter = excelWriter;
        _logger = logger;
        _handle = handle;
        _standartService = standartService;
    }

    public void SaveDirtyFilePath(string path) => _dirtyFilePath = path;
    public void SaveCleanFilePath(string path) => _cleanFilePath = path;

    public async Task<string> ProcessFilesAsync(CancellationToken cancellationToken)
    {
        if (_dirtyFilePath == null/* || _cleanFilePath == null*/)
        {
            _logger.LogError("Both files are required for processing.");
            throw new InvalidOperationException("Both files are required for processing.");
        }

        // Обработка файлов алгоритмом Cosine
        _logger.LogInformation("Запуск алгоритма Cosine");
        var resultFilePath = await RunProcessingAlgorithm(_dirtyFilePath, _cleanFilePath);
        return resultFilePath;
    }

    private async Task<string> RunProcessingAlgorithm(string dirtyFilePath, string cleanFilePath)
    {
        List<Standart> standarts = new();
        List<GarbageData> garbageData = new();

        // Создаём путь для сохранения результата
        var resultFilePath = Path.Combine("Data", "Result.xlsx");
        try
        {
            // Считываем данные из "чистого" и "грязного" файлов
            _logger.LogInformation("Считывание грязных данных");
            await Task.Run(() => //выделение отдельного потока для функций CreateCollectionFromExcel. Основной поток может выполнять другие действия, пока не завершится считывание файлов
            {                
                //standarts = _excelReader.CreateCollectionFromExcel(cleanFilePath, new StandartFactory());//need to merge clean files
                garbageData = _excelReader.CreateCollectionFromExcel<GarbageData,GarbageDataFactory>(dirtyFilePath, new GarbageDataFactory());
            });
            _logger.LogInformation("Считывание грязных данных завершено, вызываю метод получения стандартов из БД");
            standarts = await _standartService.GetStandartsAsync<Standart>();
            _logger.LogInformation("Стандарты получены из базы данных");
            
            var handledStandarts = _handle.HandleStandarts(standarts);
            _logger.LogInformation("Обработан вид коллекции стандартов");

            // Вычисляем коэффициенты схожести и разделяем на категории
            var (worst, mid, best) = await Task.Run(() => //выделение отдельного потока для функций CalculateCoefficent. Основной поток может выполнять другие действия, пока не завершится выполнение алгоритма
                _similarityCalculator.CalculateCoefficent(handledStandarts, garbageData));            

            // Записываем результаты в Excel
            await _excelWriter.WriteCollectionsToExcelAsync(worst, mid, best, resultFilePath);            
        }
        catch (Exception ex)
        {
            _logger.LogError($"На этапе обработки запроса возникла ошибка: {ex.Message}");
        }
        return resultFilePath;
    }
}
