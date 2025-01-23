using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;

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
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = replacementsStrategy.ReplaceItems(processingContext.Input, lumberReplacements);
            return res;

        }
    }
}
