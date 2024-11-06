using Abstractions.Interfaces;
using Algo.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    public class MinHashAlgo : ISimilarityCalculator
    {
        private int hashFuncCount;
        private int totalGarbageDataItems = 0;
        private int currentProgress = 0;

        private static HashSet<string> stopWords = new HashSet<string> { "СТ", "НА", "И", "ИЗ", "С", "СОДЕРЖ", "ТОЧН", "КЛ", "ШГ", "МЕХОБР", "КАЧ", "Х/Т", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КАЛИБР", "ХОЛ", "ПР", "ПРУЖ", "АВИАЦ", "КОНСТР", "КОНСТРУКЦ", "ПРЕЦИЗ", "СПЛ", "ПРЕСС", "КА4", "ОТВЕТСТВ", "НАЗНА4", "ОЦИНК", "НИК", "БЕЗНИКЕЛ", "ЛЕГИР", "АВТОМАТ", "Г/К", "КОРРОЗИННОСТОЙК", "Н/УГЛЕР", "ПРЕСС", "АЛЮМИН", "СПЛАВОВ" };

        private static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "А","A" },
            { "В","B" },
            { "Е","E" },
            { "К", "K" },
            { "М", "M" },
            { "Н", "H" },
            { "О", "O" },
            { "Р", "P" },
            { "С", "C" },
            { "Т", "T" },
            { "У", "Y" },
            { "Х", "X" },
            { "OCT1","OCT 1" }
        };

        public MinHashAlgo(int hashFuncCount = 20)
        {
            this.hashFuncCount = hashFuncCount;
        }

        public int[] MinHashFunction(string str)
        {
            int[] signatures = new int[hashFuncCount];

            var shingles = GetShingles(str);

            for (int i = 0; i < hashFuncCount; i++)
            {
                var minHash = int.MaxValue;
                foreach (var s in shingles)
                {
                    int hashValue = GenerateHashFunc(s, i + 1);
                    if (hashValue < minHash)
                    {
                        minHash = hashValue;
                    }
                }
                signatures[i] = minHash;
            }
            return signatures;
        }

        public HashSet<string> GetShingles(string str)
        {
            var fixedStr = str.ToUpper().Replace("\"", "").Replace("\r", "").Replace("\t", "").Replace("\n", "");
            fixedStr = fixedStr.TrimEnd(',');
            foreach (var pair in replacements)
            {
                fixedStr = fixedStr.Replace(pair.Key, pair.Value);
            }
            var tokens = fixedStr.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            HashSet<string> shingles = new HashSet<string>();

            List<int> shingleLengths = new List<int> { 1, 2};

            foreach (var k in shingleLengths)
            {
                for (int i = 0; i <= filteredTokens.Count - k; i++)
                {
                    shingles.Add(string.Join(" ", filteredTokens.Skip(i).Take(k)));
                }
            }
            return shingles;
        }

        public int GenerateHashFunc(string value, int seed)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(value + seed.ToString());
                var hashBytes = sha256.ComputeHash(inputBytes);
                int hash = BitConverter.ToInt32(hashBytes, 0);
                return Math.Abs(hash);
            }     
        }

        public double JaccardSimilarity(int[] s1, int[] s2)
        {
            HashSet<int> set1 = new HashSet<int>(s1);
            HashSet<int> set2 = new HashSet<int>(s2);
            int intersectCount = set1.Intersect(set2).Count();
            return (double)intersectCount / (set1.Count + set2.Count - intersectCount);
        }

        public (HashSet<TGarbageData> worst, HashSet<TGarbageData> mid, HashSet<TGarbageData> best) CalculateCoefficent<TStandart, TGarbageData>(
            List<TStandart> standarts,
            List<TGarbageData> garbageData) //функция получения трёх коллекций с данными
            where TStandart : IStandart
            where TGarbageData : IGarbageData
        {
            var worst = new HashSet<TGarbageData>();
            var mid = new HashSet<TGarbageData>();
            var best = new HashSet<TGarbageData>();

            totalGarbageDataItems = garbageData.Count;
            int processedItems = 0;

            Dictionary<TStandart, int[]> standartSignatures = new Dictionary<TStandart, int[]>();
            foreach (var item in standarts)
            {
                standartSignatures.Add(item, MinHashFunction(item.Name));
            }

            // используем ConcurrentBag для безопасной работы с коллекциями из нескольких потоков
            ConcurrentBag<TGarbageData> worstBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> midBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> bestBag = new ConcurrentBag<TGarbageData>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // Ограничение на количество потоков
            };

            Parallel.ForEach(garbageData, parallelOptions, (garbageItem, state) =>
            {
                double bestValue = -1;
                var garbageSignature = MinHashFunction(garbageItem.ShortName);
                foreach (var (standart, signature) in standartSignatures)
                {
                    var jaccardSimilarity = JaccardSimilarity(signature, garbageSignature);
                    if (jaccardSimilarity > bestValue)
                    {
                        bestValue = jaccardSimilarity;
                    }
                }
                if (bestValue < 0.05)
                    worstBag.Add(garbageItem);
                else if (bestValue < 0.6)
                    midBag.Add(garbageItem);
                else
                    bestBag.Add(garbageItem);
                currentProgress = Interlocked.Increment(ref processedItems);
                //if (currentProgress % 10 == 0)
                //{
                //    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.95), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
                //}
            });

            foreach (var item in worstBag)
            {
                worst.Add(item);
            }
            foreach (var item in midBag)
            {
                mid.Add(item);
            }
            foreach (var item in bestBag)
            {
                best.Add(item);
            }
            return (worst, mid, best);
        }

        public double GetProgress() //alpha testing
        {
            return (double)currentProgress * 100 / totalGarbageDataItems;
        }
    }
}
