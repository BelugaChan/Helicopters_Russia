using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WireHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Проволока
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> {"ОЛОВО","ПРЕЦИЗ", "ИЗ","СПЛ", "ЭЛЕКТР", "СОПР", "ВЫСОКИМ", "СЕРЕБР", "СТ", "ПРУЖ", "УГЛЕР", "КАЧ", "ОТВЕТСТВ", "НАЗНАЧ", "ОЦИНК", "НЕРЖ", "ХОЛОДНО", "ТЯНУТАЯ"};
        private Dictionary<string, string> wireReplacements = new Dictionary<string, string>
        {
            {" С ", " " },
            {"КА Ч ", "" },
            {"НАЗНА Ч ", "" },
            {"2 АКЛ ", ""},
            { "(", "" },
            { ")", "" }
        };
        private Dictionary<string, string> regexReplacements = new Dictionary<string, string>
        {
            {@"(\b|\s)ПР\s", "$1ПРОВОЛОКА " },
            {@"(\b|\s)ПРВ\s", "$1ПРОВОЛОКА " },
            {@"\b\s*Д\s*(\d+)\b"," $1 " }
        };

        private IReplacementsStrategy replacementsStrategy;
        private IRegexReplacementStrategy regexReplacementStrategy;
        private IStopWordsStrategy stopWordsStrategy;

        public WireHandler(IReplacementsStrategy replacementsStrategy, IRegexReplacementStrategy regexReplacementStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.regexReplacementStrategy = regexReplacementStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Проволока" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var replaced = replacementsStrategy.ReplaceItems(processingContext.Input, wireReplacements);
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced, regexReplacements, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;

        }
    }
}
