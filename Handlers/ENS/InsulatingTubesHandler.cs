using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class InsulatingTubesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Трубки изоляционные гибкие
        /// </summary>
        protected static Dictionary<string, string> tubesReplacements = new ()
        {
            { "В С","ВЫСШИЙ СОРТ" }
        };

        private IReplacementsStrategy replacementsStrategy;
        public InsulatingTubesHandler(IReplacementsStrategy replacementsStrategy) => this.replacementsStrategy = replacementsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Трубки изоляционные гибкие" ];

        public string AdditionalStringHandle(ProcessingContext processingContext) => replacementsStrategy.ReplaceItems(processingContext.Input, tubesReplacements);
    }
}
