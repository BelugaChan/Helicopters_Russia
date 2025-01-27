using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WashersHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Шайбы
        /// </summary>

        private readonly HashSet<string> stopWords = new () { "ГРОВЕРА" };

        private Dictionary<string, string> washersReplacements = new ()
        {
            { @"\s\d{9}\b","" }
        };

        private IRegexReplacementStrategy regexReplacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public WashersHandler(IRegexReplacementStrategy regexReplacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.regexReplacementsStrategy = regexReplacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => [ "Шайбы" ];
        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            var res = regexReplacementsStrategy.ReplaceItemsWithRegex(processingContext.Input, washersReplacements,RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(res, stopWords);
            return final;

        }
    }
}
