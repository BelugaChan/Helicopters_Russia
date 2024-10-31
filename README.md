## Взаимодействие с Библиотекой классов Algo 
__ __
Для корректной работы библиотеки необходимо установить NuGet пакет NPOI и подключить ссылки на dll через
dependencies -> Add Project Reference. 
Список необходимых библиотек:
+ Algo(основной алгоритм)
+ ExcelHandler(работа с xlsx файлами)
+ Abstractions(интерфейсы)
Данные библиотеки находятся в папке DLL
__ __

*Создание экземпляра класса MinHash. Интерфейс был добавлен для возможной реализации иных алгоритмов сравнения*
```
ISimilarityCalculator similarityCalculator = new MinHash();
```
*Создание экземпляра IExcelReader для считывания данных из Excel файлов
и IExcelWriter для записи данных в Excel фалйы*
```
IExcelReader reader = new NPOIReader();
IExcelWriter excelWriter = new NPOIWriter();
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
similarityCalculator.CalculateCoefficent(standarts, garbageData, out HashSet<GarbageData> worst, out HashSet<GarbageData> mid, out HashSet<GarbageData> best);
```
*Запись данных в отчёт*
```
excelWriter.WriteCollectionsToExcel(worst, mid, best, savePath);
```
## Объединение Excel-файлов
```
IExcelMerger excelMerger = new NPOIMerger();
excelMerger.MergeExcelFiles(new List<string>() { "path_to_standarts_1",
"path_to_standarts_1"}, "path_to_result_file", "result_fileName");
```
