using Algo.Interfaces.Strategy;

namespace Algo.MethodStrategy.Circles
{
    public class Gost5950Strategy : IGostStrategy
    {
        private string[] steelNamesGroup1 = ["13 Х", "8 ХФ", "9 ХФ", "11 ХФ", "9 ХФМ", "Х", "9 Х 1", "12 Х 1", "120 Х", "ЭП 430"];
        public string HandleWithExactGost(string str)
        {
            return str;//заглушка
        }
    }
}
