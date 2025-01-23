using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using NPOI.SS.Formula.Functions;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class PlatesAndRubberTapes : IAdditionalENSHandler
    {
        /// <summary>
        /// Пластины, ленты резиновые
        /// </summary>

        private HashSet<string> stopWords = new HashSet<string> { "РЕЗИН" };
        private Dictionary<string, string> plateRegex = new Dictionary<string, string>
        {
            {@"Н\s1", @"H I" },
            { @"([МЛ])\s*(\d+)?\s*Л\s*(\d+)", "$1 $2 $3"}
        };

        private IRegexReplacementStrategy regexReplacementStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public PlatesAndRubberTapes(IRegexReplacementStrategy regexReplacementStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.regexReplacementStrategy = regexReplacementStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => ["Пластины, ленты резиновые"];

        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(processingContext.Input, plateRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;
        }
    }
}
