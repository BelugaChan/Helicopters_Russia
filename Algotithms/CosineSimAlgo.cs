using Algo.Abstract;
using F23.StringSimilarity;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        private int shigleLength = 3;
        public CosineSimAlgo(int shigleLength)
        {
            this.shigleLength = shigleLength;
        }
        public string StringHandler(string str) //обработка строк
        {
            StringBuilder stringBuilder = new StringBuilder();

            var fixedStr = str.ToUpper();
            string result = Regex.Replace(fixedStr, pattern, " ");
            foreach (var pair in replacements)
            {
                result = result.Replace(pair.Key, pair.Value);
            }
            result = result.TrimEnd('.');
            var tokens = result.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            if ("AEЁИOYЭЫЯ".IndexOf(filteredTokens[0][filteredTokens[0].Length - 1]) >= 0)
            {
                filteredTokens[0] = filteredTokens[0].Substring(0, filteredTokens[0].Length - 1);
            }
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append(filteredTokens[i]);
            }
            return stringBuilder.ToString();
        }

        public override (HashSet<TGarbageData> worst, HashSet<TGarbageData> mid, HashSet<TGarbageData> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<TStandart> standarts, 
            List<TGarbageData> garbageData)
        {
            var cosine = new Cosine(shigleLength);

            HashSet<TGarbageData> worst = new();
            HashSet<TGarbageData> mid = new();
            HashSet<TGarbageData> best = new();
            var standartList = new List<Dictionary<string, int>>();

            //var parallelOptions = new ParallelOptions
            //{
            //    MaxDegreeOfParallelism = Environment.ProcessorCount
            //};
            Parallel.ForEach(standarts, parallelOptions, (standartItem, state) =>
            {
                standartList.Add(cosine.GetProfile(StringHandler(standartItem.Name)).ToDictionary());
            });
            //foreach (var item in standarts)
            //{

            //}

            ConcurrentBag<TGarbageData> worstBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> midBag = new ConcurrentBag<TGarbageData>();
            ConcurrentBag<TGarbageData> bestBag = new ConcurrentBag<TGarbageData>();
            int processedItems = 0;

            Parallel.ForEach(garbageData, parallelOptions, (item, state) =>
            {
                double bestValue = -1;
                var profileGarbge = cosine.GetProfile(StringHandler(item.ShortName));
                foreach (var profile in standartList)
                {
                    var res = cosine.Similarity(profile, profileGarbge);
                    if (res > bestValue)
                        bestValue = res;
                    if (bestValue == 1)
                        break;
                }

                if (bestValue < 0.1)
                    worstBag.Add(item);
                else if (bestValue < 0.7)
                    midBag.Add(item);
                else
                    bestBag.Add(item);
                if (currentProgress % 5 == 0)
                {
                    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.7), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
                }
                currentProgress = Interlocked.Increment(ref processedItems);
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
    }
}
