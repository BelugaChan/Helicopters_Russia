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

*Создание экземпляра IExcelReader для считывания данных из Excel файлов
и IExcelWriter для записи данных в Excel фалйы*
```
IExcelReader reader = new NPOIReader();
IExcelWriter excelWriter = new NPOIWriter();
```
*Инструмент отслеживания прогресса работы алгоритма*
```
IProgressStrategy progressStrategy = new ConsoleProgressStrategy();
```
*Библиотека Cosine, добавленная из nuget-пакета F23.StringSimilarity. В конструктор передаётся длина шингла.*
```
Cosine cosine = new Cosine(3);
```
*Базовый обработчик для эталонов и грязных позиций*
```
IENSHandler eNSHandler = new ENSHandler();
```
*Дополнительные обработчики по классификаторам ЕНС*
```
var registry = new ENSHandlerRegistry();
registry.RegisterHandler([ "Наименование класса" ], new Func<string, string>((str) => new Handler().AdditionalStringHandle(str)));
...
```
*Обработчик ГОСТов*
```
IGostHandle gostHandle = new GostHandler();
```
*Инструмент удаления ГОСТов*
```
IGostRemove gostRemove = new GostRemover();
```
*Обработчик наименований стандартов*
```
IStandartHandle<Standart> standartHandle = new StandartHandler<Standart>(eNSHandler, gostRemove, standartFactory,gostHandle, progressStrategy);
```
*Создание экземпляра класса CosineSimAlgo.*
```
ISimilarityCalculator similarityCalculator = new CosineSimAlgo(eNSHandler, progressStrategy, registry, cosine);
```
*Создание фасада для упрощённого взаимодействия с библиотекой*
```
AlgoFacade<Standart, GarbageData> algoFacade = new AlgoFacade<Standart, GarbageData>(gostHandle, standartHandle, gostRemove, progressStrategy);
```
*Пути к исходным файлам*
```
string combinedFilePath = @"some_path_to_combined_standard_file";
string garbageFilePath = @"some_path_to_garbage_data_file";
```
*Инициализация коллекций*
```
HashSet<Standart> standarts = new();
HashSet<GarbageData> garbageData = new();
```
*Получение коллекций с выделением для них отдельного потока*
```
await Task.Run(() => 
{
    standarts = excelReader.CreateCollectionFromExcel<Standart, StandartFactory>(cleanFilePath, standartFactory);//need to merge clean files
    garbageData = excelReader.CreateCollectionFromExcel<GarbageData, GarbageDataFactory>(dirtyFilePath, new GarbageDataFactory());
});        
```
*Предобработка эталонов и грязных позиций*
```
var algoresult = algoFacade.AlgoWrap(standarts, garbageData);
```
*Подсчёт коэффициентов идентичности двух массивов данных*
```
var (worst, mid, best) = await Task.Run(() =>
        similarityCalculator.CalculateCoefficent(algoresult));
```
*Путь для сохранения отчёта*
```
string savePath = @"some_path_to_report";
```
*Запись данных в отчёт*
```
await excelWriter.WriteCollectionsToExcelAsync(worst, mid, best, savePath);
```
*Получение процента выполнения алгоритма (на стадии тестирования)*
```
var progress = progressStrategy.GetCurrentProgress();
```
## Объединение Excel-файлов
```
IExcelMerger excelMerger = new NPOIMerger();
await excelMerger.MergeExcelFilesAsync(new List<string>() { "path_to_standarts_1",
"path_to_standarts_1"}, "path_to_result_file", "result_fileName");
```
