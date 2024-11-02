## Взаимодействие с библиотеками классов
__ __
Для корректной работы библиотек необходимо установить NuGet пакет NPOI и подключить ссылки на dll в рабочий проект через
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
*Инициализация переменных*
```
List<Standart> standarts = new List<Standart>();
List<GarbageData> garbageData = new List<GarbageData>();
```
*Получение коллекций с выделением для них отдельного потока*
```
await Task.Run(() => 
{
    standarts = reader.CreateCollectionFromExcel(combinedFilePath, new StandartFactory());
    garbageData = reader.CreateCollectionFromExcel(garbageFilePath, new GarbageDataFactory());
});
```
*Подсчёт коэффициентов идентичности*
```
var (worst, mid, best) = await Task.Run(() =>
    similarityCalculator.CalculateCoefficent(standarts, garbageData));
```
*Путь для сохранения отчёта*
```
string savePath = @"some_path_to_report";
```
*Запись данных в отчёт*
```
await writer.WriteCollectionsToExcelAsync(worst, mid, best, savePath);
```
*Получение процента выполнения алгоритма (на стадии тестирования)*
```
double progress = similarityCalculator.GetProgress();
```
## Объединение Excel-файлов
```
IExcelMerger excelMerger = new NPOIMerger();
await excelMerger.MergeExcelFilesAsync(new List<string>() { "path_to_standarts_1",
"path_to_standarts_1"}, "path_to_result_file", "result_fileName");
```
