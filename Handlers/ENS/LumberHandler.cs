using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;

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
        private IReplacementsStrategy replacementsStrategy;
        public LumberHandler(IReplacementsStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }
        public string AdditionalStringHandle(string str)
        {
            var res = replacementsStrategy.ReplaceItems(str,lumberReplacements); 
            return res;
            //foreach (var pair in lumberReplacements)
            //{
            //    str = str.Replace(pair.Key, pair.Value);
            //}
            //return str;
        }
    }
}
