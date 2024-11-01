using Algo.Factory;
using Algo.Interfaces;
using Algo.Models;
using ExcelHandler.Interfaces;

public class FileProcessingService
{
    private readonly ISimilarityCalculator _similarityCalculator;
    private readonly IExcelReader _excelReader;
    private readonly IExcelWriter _excelWriter;

    private string? _dirtyFilePath;
    private string? _cleanFilePath;

    public FileProcessingService(ISimilarityCalculator similarityCalculator, IExcelReader excelReader, IExcelWriter excelWriter)
    {
        _similarityCalculator = similarityCalculator;
        _excelReader = excelReader;
        _excelWriter = excelWriter;
    }

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
        // Считываем данные из "чистого" и "грязного" файлов
        var standarts = _excelReader.CreateCollectionFromExcel(cleanFilePath, new StandartFactory());
        var garbageData = _excelReader.CreateCollectionFromExcel(dirtyFilePath, new GarbageDataFactory());

        // Вычисляем коэффициенты схожести и разделяем на категории
        _similarityCalculator.CalculateCoefficent(
            standarts,
            garbageData,
            out HashSet<GarbageData> worst,
            out HashSet<GarbageData> mid,
            out HashSet<GarbageData> best
        );

        // Создаём путь для сохранения результата
        var resultFilePath = Path.Combine("Data", "Result.xlsx");

        // Записываем результаты в Excel
        _excelWriter.WriteCollectionsToExcel(worst, mid, best, resultFilePath);

        return resultFilePath;
    }
}
