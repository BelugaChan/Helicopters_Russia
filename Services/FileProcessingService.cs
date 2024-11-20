using Algo.Factory;
using Algo.Interfaces;
using Algo.Models;
using ExcelHandler.Interfaces;
using Helicopters_Russia;
using Microsoft.Extensions.Options;

public class FileProcessingService(ISimilarityCalculator similarityCalculator, IExcelReader excelReader, IExcelWriter excelWriter, ILogger<FileProcessingService> logger)
{
    private string? _dirtyFilePath;
    private string? _cleanFilePath;

    public void SaveDirtyFilePath(string path) => _dirtyFilePath = path;
    public void SaveCleanFilePath(string path) => _cleanFilePath = path;

    public async Task<string> ProcessFilesAsync(CancellationToken cancellationToken)
    {
        if (_dirtyFilePath == null || _cleanFilePath == null)
            throw new InvalidOperationException("Both files are required for processing.");

        // Обработка файлов алгоритмом MinHash
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
            await Task.Run(() => //выделение отдельного потока для функций CreateCollectionFromExcel. Основной поток может выполнять другие действия, пока не завершится считывание файлов
            {
                standarts = excelReader.CreateCollectionFromExcel(cleanFilePath, new StandartFactory());//need to merge clean files
                garbageData = excelReader.CreateCollectionFromExcel(dirtyFilePath, new GarbageDataFactory());
            });

            // Вычисляем коэффициенты схожести и разделяем на категории
            var (worst, mid, best) = await Task.Run(() => //выделение отдельного потока для функций CalculateCoefficent. Основной поток может выполнять другие действия, пока не завершится выполнение алгоритма
                similarityCalculator.CalculateCoefficent(standarts, garbageData));            

            // Записываем результаты в Excel
            await excelWriter.WriteCollectionsToExcelAsync(worst, mid, best, resultFilePath);            
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"На этапе обработки запроса возникла ошибка: {ex.Message}, Время - {DateTimeOffset.Now}");
        }
        return resultFilePath;
    }
}
