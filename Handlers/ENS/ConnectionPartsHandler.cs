using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class ConnectionPartsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Части соединительные
        /// </summary>

        private HashSet<string> stopWords = new () { "Д","D" };
        private Dictionary<string, string> connectionReplacements = new ()
        {
            { "КОНТРАГАЙКА ", "КОНТРГАЙКА "},
        };

        private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public ConnectionPartsHandler(IReplacementsStrategy replacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => ["Части соединительные" ];
        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            var midRes = replacementsStrategy.ReplaceItems(processingContext.Input, connectionReplacements);
            var final = stopWordsStrategy.RemoveWords(midRes, stopWords);
            return final;

        }
    }
}
