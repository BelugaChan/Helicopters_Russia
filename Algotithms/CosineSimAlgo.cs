using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Handlers.ENS;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Models;
using F23.StringSimilarity;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        //private int shigleLength;
        private Cosine cosine;
        private IENSHandler eNSHandler;
        private IAdditionalENSHandler<LumberHandler> lumberHandler;
        private IAdditionalENSHandler<CalsibCirclesHandler> calsibCirclesHandler;
        private IAdditionalENSHandler<RopesAndCablesHandler> ropesAndCablesHandler;
        private IAdditionalENSHandler<MountingWiresHandler> mountingWiresHandler;
        private IAdditionalENSHandler<WireHandler> wireHandler;
        private IAdditionalENSHandler<BarsAndTiresHandler> barsHandler;
        private IAdditionalENSHandler<PipesHandler> pipesHandler;
        private IAdditionalENSHandler<WashersHandler> washersHandler;
        private IAdditionalENSHandler<RodHandler> rodsHandler;
        private IAdditionalENSHandler<ScrewsHandler> screwsHandler;
        private IAdditionalENSHandler<SoldersHandler> soldersHandler;
        private IAdditionalENSHandler<NailsHandler> nailsHandler;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            IAdditionalENSHandler<LumberHandler> lumberHandler, 
            IAdditionalENSHandler<CalsibCirclesHandler> calsibCirclesHandler, 
            IAdditionalENSHandler<RopesAndCablesHandler> ropesAndCablesHandler,
            IAdditionalENSHandler<MountingWiresHandler> mountingWiresHandler,
            IAdditionalENSHandler<WireHandler> wireHandler,
            IAdditionalENSHandler<BarsAndTiresHandler> barsHandler,
            IAdditionalENSHandler <PipesHandler> pipesHandler,
            IAdditionalENSHandler<WashersHandler> washersHandler,
            IAdditionalENSHandler<RodHandler> rodsHandler,
            IAdditionalENSHandler<ScrewsHandler> screwsHandler,
            IAdditionalENSHandler<SoldersHandler> soldersHandler,
            IAdditionalENSHandler<NailsHandler> nailsHandler,
            Cosine cosine)
        {
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
            this.lumberHandler = lumberHandler;
            this.calsibCirclesHandler = calsibCirclesHandler;
            this.ropesAndCablesHandler = ropesAndCablesHandler;
            this.mountingWiresHandler = mountingWiresHandler;
            this.wireHandler = wireHandler;
            this.barsHandler = barsHandler;
            this.pipesHandler = pipesHandler;
            this.washersHandler = washersHandler;
            this.rodsHandler = rodsHandler;
            this.screwsHandler = screwsHandler;
            this.soldersHandler = soldersHandler;
            this.nailsHandler = nailsHandler;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>> data, ConcurrentDictionary<TStandart, string> standarts, ConcurrentBag<(TGarbageData,HashSet<string>)> garbageDataWithoutComparedStandarts)
        {
            currentProgress = 0;          
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<(TGarbageData, TStandart?), double> mid = new();
            Dictionary<(TGarbageData, TStandart?), double> best = new();

            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> bestBag = new();
           
            Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
                int commonElementsCount = 0;
                double similarityCoeff = -1;
                
                var (garbageDataHandeledName, garbageDataItem, garbageDataGosts) = item.Keys.FirstOrDefault();
                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName/*garbageDataItem.ShortName*/);
                var tokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                foreach (var gost in garbageDataGosts)
                {
                    var gostTokens = gost.Split(new char[] {' ', '-'}).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                    foreach (var gostToken in gostTokens)
                    {
                        tokens.Add(gostToken); //добавление в список токенов всех чисел из ГОСТов, найденных для данной грязной позиции.
                    }
                }
                HashSet<int> tokenSet = new HashSet<int>(tokens);
                string improvedProcessedGarbageName = "";
                var standartStuff = item.Values; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    //if (standartGroups.Count == 0)//null reference!!!!
                    //{
                    //    dataForPostProcessing.Add((garbageDataItem, baseProcessedGarbageName,garbageDataGosts));
                    //    //worstBag.TryAdd((garbageDataItem, bestStandart), 0);
                    //    break;
                    //}

                    var groupClassificationName = standartGroups.Keys.FirstOrDefault();
                    //персональные обработчики для классификаторов ЕНС
                    improvedProcessedGarbageName = SelectHandler(groupClassificationName, improvedProcessedGarbageName, baseProcessedGarbageName);
                    foreach (var standart in standartGroups.Values) //стандарты в каждой отдельной группе
                    {
                        foreach (var standartItem in standart)
                        {
                            var similarity = cosine.Similarity(improvedProcessedGarbageName/*garbageProfile*/, standartItem.Value);
                            if (similarity > similarityCoeff)
                            {
                                similarityCoeff = similarity;
                                bestStandart = standartItem.Key;
                                var standartTokens = standartItem.Value.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                var standartGosts = new HashSet<string>() {standartItem.Key.MaterialNTD, standartItem.Key.NTD };
                                foreach (var handledGost in standartGosts)
                                {
                                    var handledGostTokens = handledGost.Split(new char[] {' ', '-'}).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                    foreach (var handledToken in handledGostTokens)
                                    {
                                        standartTokens.Add(handledToken);
                                    }
                                }
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                commonElementsCount = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                            }
                            else if (similarity == similarityCoeff)
                            {
                                var standartTokens = standartItem.Value.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                var standartGosts = new HashSet<string>() { standartItem.Key.MaterialNTD, standartItem.Key.NTD };
                                foreach (var handledGost in standartGosts)
                                {
                                    var handledGostTokens = handledGost.Split(new char[] { ' ', '-' }).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                    foreach (var handledToken in handledGostTokens)
                                    {
                                        standartTokens.Add(handledToken);
                                    }
                                }
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                int commonElementsCountNow = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                                if (commonElementsCountNow > commonElementsCount)
                                {
                                    bestStandart = standartItem.Key;
                                }
                            }
                        }
                        
                    }
                }
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.05)
                {
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName,garbageDataGosts));
                    //worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else if (similarityCoeff < 0.6)
                    midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                else
                    bestBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                if (currentProgress % 100 == 0)
                {
                    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.7), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });
            currentProgress = 0;
            Parallel.ForEach(garbageDataWithoutComparedStandarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var (garbageData, gosts) = item;
                dataForPostProcessing.Add((garbageData, eNSHandler.BaseStringHandle(garbageData.ShortName),gosts));
            });
            //дополнительный прогон по позициям с для которых не были найдены подходящие стандарты
            Parallel.ForEach(dataForPostProcessing, new ParallelOptions { MaxDegreeOfParallelism = 1/*Environment.ProcessorCount*/ }, (item, state) =>
            {
                var (garbageDataItem, garbageName, gosts) = item;               
                TStandart? bestStandart = default;
                double similarityCoeff = -1;
                foreach (var (standart, standartName) in standarts)
                {
                    double coeff = cosine.Similarity(garbageName, standartName);
                    if (coeff > similarityCoeff)
                    {
                        similarityCoeff = coeff;
                        bestStandart = standart;
                    }
                }
                if ((gosts.Count == 0 || gosts.All(t => t.Length == 0)) && similarityCoeff > 0.05)//если у позиции отсутствует ГОСТ, то она переносится в коллекцию требует уточнения
                {
                    midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else
                {
                    if (similarityCoeff < 0.05)
                    {
                        worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                    }
                    else if (similarityCoeff < 0.6)
                        midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                    else
                        bestBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }               
                if (currentProgress % 100 == 0)
                {
                    Console.WriteLine($"Additional Checkin' текущий прогресс: {Math.Round((double)currentProgress / dataForPostProcessing.Count,2) * 100}%");
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });



            foreach (var ((item, standart), bestValue) in worstBag)
            {
                worst.Add((item, standart), bestValue);
            }
            foreach (var ((item, standart), bestValue) in midBag)
            {
                mid.Add((item, standart), bestValue);
            }
            foreach (var ((item, standart), bestValue) in bestBag)
            {
                best.Add((item, standart), bestValue);
            }
            return (worst, mid, best);
        }


        public string SelectHandler(string groupClassificationName, string improvedProcessedGarbageName, string baseProcessedGarbageName)
        {
            switch (groupClassificationName)
            {
                case string name when name.Contains("Круги, шестигранники, квадраты") ||
                                      name.Contains("Калиброванные круги, шестигранники, квадраты"):
                    {
                        improvedProcessedGarbageName = calsibCirclesHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Пиломатериалы"):
                    {
                        improvedProcessedGarbageName = lumberHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Канаты, Тросы"):
                    {
                        improvedProcessedGarbageName = ropesAndCablesHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Провода монтажные"):
                    {
                        improvedProcessedGarbageName = mountingWiresHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Проволока"):
                    {
                        improvedProcessedGarbageName = wireHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Прутки из титана и сплавов")
                                   || name.Contains("Прутки, шины из алюминия и сплавов")
                                   || name.Contains("Прутки, шины из меди и сплавов"):
                    {
                        improvedProcessedGarbageName = barsHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Трубы бесшовные")
                                   || name.Contains("Трубы сварные")
                                   || name.Contains("Трубы, трубки из алюминия и сплавов")
                                   || name.Contains("Трубы, трубки из меди и сплавов"):
                    {
                        improvedProcessedGarbageName = pipesHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Шайбы"):
                    {
                        improvedProcessedGarbageName = washersHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Катанка, проволока из меди и сплавов"): //need to fix
                    {
                        improvedProcessedGarbageName = baseProcessedGarbageName;
                        break;
                    }
                case string name when name.Contains("Катанка, проволока"):
                    {
                        improvedProcessedGarbageName = rodsHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Шурупы"):
                    {
                        improvedProcessedGarbageName = screwsHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Припои (прутки, проволока, трубки)"):
                    {
                        improvedProcessedGarbageName = soldersHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                case string name when name.Contains("Гвозди, Дюбели"):
                    {
                        improvedProcessedGarbageName = nailsHandler.AdditionalStringHandle(baseProcessedGarbageName);
                        break;
                    }
                default:
                    {
                        improvedProcessedGarbageName = baseProcessedGarbageName;
                        break;
                    }
            }
            return improvedProcessedGarbageName;
        }
       
    }
}
