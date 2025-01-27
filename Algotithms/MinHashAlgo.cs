using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    //public class MinHashAlgo : SimilarityCalculator
    //{
    //    private int hashFuncCount;
        
    //    public MinHashAlgo(int hashFuncCount = 20)
    //    {
    //        this.hashFuncCount = hashFuncCount;
    //    }

    //    public int[] MinHashFunction(string str)
    //    {
    //        int[] signatures = new int[hashFuncCount];

    //        var shingles = GetShingles(str);

    //        for (int i = 0; i < hashFuncCount; i++)
    //        {
    //            var minHash = int.MaxValue;
    //            foreach (var s in shingles)
    //            {
    //                int hashValue = GenerateHashFunc(s, i + 1);
    //                if (hashValue < minHash)
    //                {
    //                    minHash = hashValue;
    //                }
    //            }
    //            signatures[i] = minHash;
    //        }
    //        return signatures;
    //    }

    //    public HashSet<string> GetShingles(string str)
    //    {
    //        var fixedStr = str.ToUpper();
    //        string result = Regex.Replace(fixedStr, pattern, " ");
    //        foreach (var pair in replacements)
    //        {
    //            result = result.Replace(pair.Key, pair.Value);
    //        }
    //        result = result.TrimEnd('.');
    //        var tokens = result.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
    //        var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
    //        if ("AEЁИOYЭЫЯ".IndexOf(filteredTokens[0][filteredTokens[0].Length - 1]) >= 0)
    //        {
    //            filteredTokens[0] = filteredTokens[0].Substring(0, filteredTokens[0].Length - 1);
    //        }
    //        HashSet<string> shingles = new HashSet<string>();

    //        List<int> shingleLengths = new List<int> { 1/*, 2*//*, 3 */};

    //        foreach (var k in shingleLengths)
    //        {
    //            for (int i = 0; i <= filteredTokens.Count - k; i++)
    //            {
    //                shingles.Add(string.Join(" ", filteredTokens.Skip(i).Take(k)));
    //            }
    //        }
    //        return shingles;
    //    }

    //    public int GenerateHashFunc(string value, int seed)
    //    {
    //        var inputBytes = Encoding.UTF8.GetBytes(value + seed.ToString());
    //        var hashBytes = MD5.HashData(inputBytes);
    //        int hash = BitConverter.ToInt32(hashBytes, 0);
    //        return Math.Abs(hash);
    //    }

    //    public double JaccardSimilarity(int[] s1, int[] s2)
    //    {
    //        HashSet<int> set1 = new HashSet<int>(s1);
    //        HashSet<int> set2 = new HashSet<int>(s2);
    //        int intersectCount = set1.Intersect(set2).Count();
    //        return (double)intersectCount / (set1.Count + set2.Count - intersectCount);
    //    }

    //    public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>(ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> standartDict, List<TGarbageData> garbageData)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> HandleStandarts<TStandart>(List<TStandart> standarts, IUpdatedEntityFactory<TStandart> factory)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public override (HashSet<TGarbageData> worst, HashSet<TGarbageData> mid, HashSet<TGarbageData> best) CalculateCoefficent<TStandart, TGarbageData>
    //    //    (List<TStandart> standarts,
    //    //    List<TGarbageData> garbageData) //функция получения трёх коллекций с данными
    //    //{
    //    //    var worst = new HashSet<TGarbageData>();
    //    //    var mid = new HashSet<TGarbageData>();
    //    //    var best = new HashSet<TGarbageData>();

    //    //    totalGarbageDataItems = garbageData.Count;
    //    //    int processedItems = 0;

    //    //    ConcurrentDictionary<TStandart, int[]> standartSignatures = new ConcurrentDictionary<TStandart, int[]>();
    //    //    Parallel.ForEach(standarts, item =>
    //    //    {
    //    //        var signature = MinHashFunction(item.Name);
    //    //        standartSignatures[item] = signature;
    //    //    });

    //    //    // используем ConcurrentBag для безопасной работы с коллекциями из нескольких потоков
    //    //    ConcurrentBag<TGarbageData> worstBag = new ConcurrentBag<TGarbageData>();
    //    //    ConcurrentBag<TGarbageData> midBag = new ConcurrentBag<TGarbageData>();
    //    //    ConcurrentBag<TGarbageData> bestBag = new ConcurrentBag<TGarbageData>();

    //    //    //var parallelOptions = new ParallelOptions
    //    //    //{
    //    //    //    MaxDegreeOfParallelism = Environment.ProcessorCount // Ограничение на количество потоков
    //    //    //};

    //    //    Parallel.ForEach(garbageData, parallelOptions, (garbageItem, state) =>
    //    //    {
    //    //        double bestValue = -1;
    //    //        var garbageSignature = MinHashFunction(garbageItem.ShortName);
    //    //        foreach (var (standart, signature) in standartSignatures)
    //    //        {
    //    //            var jaccardSimilarity = JaccardSimilarity(signature, garbageSignature);
    //    //            if (jaccardSimilarity > bestValue)
    //    //            {
    //    //                bestValue = jaccardSimilarity;
    //    //            }
    //    //        }
    //    //        if (bestValue < 0.05)
    //    //            worstBag.Add(garbageItem);
    //    //        else if (bestValue < 0.9)
    //    //            midBag.Add(garbageItem);
    //    //        else
    //    //            bestBag.Add(garbageItem);
    //    //        currentProgress = Interlocked.Increment(ref processedItems);
    //    //        //if (currentProgress % 10 == 0)
    //    //        //{
    //    //        //    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.95), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
    //    //        //}
    //    //    });

    //    //    foreach (var item in worstBag)
    //    //    {
    //    //        worst.Add(item);
    //    //    }
    //    //    foreach (var item in midBag)
    //    //    {
    //    //        mid.Add(item);
    //    //    }
    //    //    foreach (var item in bestBag)
    //    //    {
    //    //        best.Add(item);
    //    //    }
    //    //    return (worst, mid, best);
    //    //}
    //}
}
