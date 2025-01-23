using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WashersHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Шайбы
        /// </summary>

        private readonly HashSet<string> stopWords = new HashSet<string> { "ГРОВЕРА" };

        private Dictionary<string, string> washersReplacements = new Dictionary<string, string>
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
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = regexReplacementsStrategy.ReplaceItemsWithRegex(processingContext.Input, washersReplacements,RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(res, stopWords);
            return final;

        }
    }
}
