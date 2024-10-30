## Взаимодействие с MinHash

*Создание экземпляра класса MinHash. Интерфейс был добавлен для возможной реализации иных алгоритмов сравнения*
```
ISimilarityCalculator similarityCalculator = new Algo();
```
*Создание экземпляра EPPlusReader для считывания данных из Excel файлов*
```
IExcelReader reader = new NPOIReader();
```
*Пути к исходным файлам*
```
string combinedFilePath = @"some_path_to_combined_standard_file";
string garbageFilePath = @"some_path_to_garbage_data_file";
```
*Получение коллекций*
```
var standarts = reader.CreateCollectionFromExcel(combinedFilePath, new StandartFactory());
var garbageData = reader.CreateCollectionFromExcel(garbageFilePath, new GarbageDataFactory());
```
*Путь для сохранения отчёта*
```
string savePath = @"some_path_to_report";
```
*Подсчёт коэффициентов и запись в результата работы алгоритма в новый Excel файл*
```
similarityCalculator.CalculateCoefficent(standarts, garbageData, savePath);
```

## Объединение Excel-файлов
```
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
IExcelMerger excelMerger = new NPOIMerger();
excelMerger.MergeExcelFiles(new List<string>() { "path_to_standarts_1",
"path_to_standarts_1"}, "path_to_result_file", "result_fileName");
```
