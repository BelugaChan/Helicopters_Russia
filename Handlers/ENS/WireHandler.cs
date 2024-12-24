using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WireHandler : IAdditionalENSHandler<WireHandler>
    {
        /// <summary>
        /// Проволока
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> {"ОЛОВО","ПРЕЦИЗ", "ИЗ","СПЛ", "ЭЛЕКТР", "СОПР", "ВЫСОКИМ", "СЕРЕБР", "СТ", "ПРУЖ", "УГЛЕР", "КАЧ", "ОТВЕТСТВ", "НАЗНАЧ", "ОЦИНК", "НЕРЖ", "ХОЛОДНО", "ТЯНУТАЯ"};
        private Dictionary<string, string> wireReplacements = new Dictionary<string, string>
        {
            {" С ", " " },
            {"КА Ч ", "" },
            {"НАЗНА ч ", "" },
            {"2 АКЛ ", ""},
            { "(", "" },
            { ")", "" }
        };
        private Dictionary<string, string> regexReplacements = new Dictionary<string, string>
        {
            {"ПР", "ПРОВОЛОКА" },
            {"ПРВ", "ПРОВОЛОКА" }
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
        public string AdditionalStringHandle(string str)
        {
            var replaced = replacementsStrategy.ReplaceItems(str, wireReplacements);
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced,regexReplacements,RegexOptions.IgnoreCase);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;
        }
    }
}
