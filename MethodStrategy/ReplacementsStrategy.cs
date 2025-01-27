using AbstractionsAndModels.Interfaces.ProgressStrategy;

namespace Algo.MethodStrategy
{
    public class ReplacementsStrategy : IReplacementsStrategy
    {
        public string ReplaceItems(string str, Dictionary<string, string> replacements)
        {
            foreach (var pair in replacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
