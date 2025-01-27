using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Algo.Simpled
{

    public class CosineSimpled
    {
        private readonly Regex SPACE_REG = new Regex("\\s+");
        private int k = 3;
        public IDictionary<string, int> GetProfile(string s)
        {
            var shingles = new Dictionary<string, int>();

            var string_no_space = SPACE_REG.Replace(s, " ");

            for (int i = 0; i < (string_no_space.Length - k + 1); i++)
            {
                var shingle = string_no_space.Substring(i, k);

                if (shingles.TryGetValue(shingle, out var old))
                {
                    shingles[shingle] = old + 1;
                }
                else
                {
                    shingles[shingle] = 1;
                }
            }

            return new ReadOnlyDictionary<string, int>(shingles);
        }

        private double Norm(IDictionary<string, int> profile)
        {
            double agg = 0;

            foreach (var entry in profile)
            {
                agg += Math.Pow(entry.Value, 2.5);/*1.0 * entry.Value * entry.Value;*/
            }

            return Math.Sqrt(agg);
        }

        private double DotProduct(IDictionary<string, int> profile1,
            IDictionary<string, int> profile2)
        {
            // Loop over the smallest map
            var small_profile = profile2;
            var large_profile = profile1;

            if (profile1.Count < profile2.Count)
            {
                small_profile = profile1;
                large_profile = profile2;
            }

            double agg = 0;
            foreach (var entry in small_profile)
            {
                if (!large_profile.TryGetValue(entry.Key, out var i)) continue;

                agg += 1.0 * entry.Value * i;
            }

            return agg;
        }

        public double Similarity(string s1, string s2)
        {
            if (s1 == null)
            {
                throw new ArgumentNullException(nameof(s1));
            }

            if (s2 == null)
            {
                throw new ArgumentNullException(nameof(s2));
            }

            if (s1.Equals(s2))
            {
                return 1;
            }

            if (s1.Length < k || s2.Length < k)
            {
                return 0;
            }

            var profile1 = GetProfile(s1);
            var profile2 = GetProfile(s2);

            return DotProduct(profile1, profile2) / (Norm(profile1) * Norm(profile2));
        }
    }
}
