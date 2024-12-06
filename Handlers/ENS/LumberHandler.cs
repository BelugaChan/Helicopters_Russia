using Algo.Interfaces.Handlers.ENS;

namespace Algo.Handlers.ENS
{
    public class LumberHandler :  IAdditionalENSHandler<LumberHandler>
    {
        /// <summary>
        /// Пиломатериалы
        /// </summary>
        protected static Dictionary<string, string> lumberReplacements = new Dictionary<string, string>
        {
            { "ХВОЙН","ХВ" },
            { "ХВОЙНЫЙ","ХВ" },
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in lumberReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
