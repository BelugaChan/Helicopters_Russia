using Abstractions.Interfaces;
using Algo.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    public class MinHashAlgo : ISimilarityCalculator
    {
        private int k; //shingle length
        private int hashFuncCount;
        private int totalGarbageDataItems = 0;
        private int currentProgress = 0;
        public MinHashAlgo(int k = 2, int hashFuncCount = 20)
        {
            this.k = k;
            this.hashFuncCount = hashFuncCount;
        }

        public int[] MinHashFunction(string str) //функция подсчёта минимального хэша для каждого шингла в строке
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

        public HashSet<string> GetShingles(string str) //функция получения шинглов у строки
        {
            var fixedStr = Regex.Replace(str, @"[^а-яА-Я0-9\s]", "").ToLower(); //удаление посторонних символов, кроме цифр, пробелов и букв
            var tokens = fixedStr.Split(' ', StringSplitOptions.RemoveEmptyEntries); //преобразование строки в массив токенов
            var shingles = new HashSet<string>();

            for (int i = 0; i <= tokens.Length - k; i++)
            {
                string shingle = string.Join(" ", tokens.Skip(i).Take(k));
                shingles.Add(shingle);
            }
            return shingles;
        }

        public int GenerateHashFunc(string value, int seed) //функция создания хэша для отдельного шингла
        {
            int hash = Math.Abs((value.GetHashCode() * seed + 170) % 2147483647);
            return hash;
        }

        public double JaccardSimilarity(int[] s1, int[] s2) //функция подсчёта коэффициента Жаккарда
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

            Dictionary<TStandart, int[]> standartSignatures = new Dictionary<TStandart, int[]>();//сигнатуры для эталонных позиций
            foreach (var item in standarts)
            {
                standartSignatures.Add(item, MinHashFunction(item.Name));
            }

            // используем ConcurrentBag для безопасной работы с коллекциями из нескольких потоков
            ConcurrentBag<TGarbageData> worstBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> midBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> bestBag = new ConcurrentBag<TGarbageData>();

            Parallel.ForEach(garbageData, (garbageItem, state) =>
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

                if (bestValue == 0)
                    worstBag.Add(garbageItem);
                else if (bestValue < 0.3)
                    midBag.Add(garbageItem);
                else
                    bestBag.Add(garbageItem);

                currentProgress = Interlocked.Increment(ref processedItems);//индексатор текущего прогресса работы алгоритма
                //завершение циклом выполняющихся операций при достижении индексатора в 300 единиц
                //if (currentProgress == 300)
                //{
                //    state.Stop();
                //}
            });

            // Переносим элементы из ConcurrentBag в обычные HashSet
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
