using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class LumberHandler :  IAdditionalENSHandler
    {
        /// <summary>
        /// Пиломатериалы
        /// </summary>
        protected static Dictionary<string, string> lumberReplacements = new ()
        {
            { "ХВОЙН","ХВ" },
            { "ХВОЙНЫЙ","ХВ" },
        };

        private IReplacementsStrategy replacementsStrategy;
        public LumberHandler(IReplacementsStrategy replacementsStrategy) => this.replacementsStrategy = replacementsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Пиломатериалы" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => replacementsStrategy.ReplaceItems(processingContext.Input, lumberReplacements);
    }
}
