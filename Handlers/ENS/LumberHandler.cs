using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;

namespace Algo.Handlers.ENS
{
    public class LumberHandler :  IAdditionalENSHandler
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
        public IEnumerable<string> SupportedKeys => new[] { "Пиломатериалы" };
        public string AdditionalStringHandle(string str)
        {
            var res = replacementsStrategy.ReplaceItems(str, lumberReplacements);
            return res;

        }
    }
}
