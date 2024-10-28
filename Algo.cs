namespace MinHash
{
    public class Algo
    {
        private static int k = 3; //shingle length
        private static int hashFuncCount = 10;
        public static HashSet<int> MinHashFunc(string str) //функция подсчёта минимального хэша для каждого шингла в строке
        {
            HashSet<int> signatures = new HashSet<int>();

            HashSet<string> shingles = GetShingles(str);
            foreach (var s in shingles)
            {
                var minHash = int.MaxValue;
                for (int i = 0; i < hashFuncCount; i++)
                {
                    int hashValue = GenerateHashFunc(s, i + 1);
                    if (hashValue < minHash)
                    {
                        minHash = hashValue;
                    }
                }
                signatures.Add(minHash);
            }

            return signatures;
        }

        public static HashSet<string> GetShingles(string str) //функция получения шинглов у строки
        {
            var shingles = new HashSet<string>();
            for (int i = 0; i < str.Length - k + 1; i++)
            {
                shingles.Add(str.Substring(i, k));
            }
            return shingles;
        }

        public static int GenerateHashFunc(string value, int seed) //функция создания хэша для отдельного шингла
        {
            int hash = Math.Abs((value.GetHashCode() * seed + 170) % 2147483647);
            return hash;       
        }

        public static double JaccardSimilarity(HashSet<int> s1, HashSet<int> s2) //функция подсчёта коэффициента Жаккарда
        {
            int intersectionsCount = 0;
            foreach (var item in s1)
            {
                if (s2.Remove(item))
                {
                    intersectionsCount++;
                }
            }
            int unionCount = s1.Count + s2.Count + intersectionsCount;
            return (double)intersectionsCount / unionCount;
        }
    }
}
