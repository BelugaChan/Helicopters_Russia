using System.Text.RegularExpressions;

namespace MinHash
{
    public class Algo
    {
        private int k; //shingle length
        private int hashFuncCount;

        public Algo(int k = 2, int hashFuncCount = 20)
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
    }
}
