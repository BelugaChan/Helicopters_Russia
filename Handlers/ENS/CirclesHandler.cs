using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CirclesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Круги, шестигранники, квадраты
        /// </summary>
        
        private HashSet<string> stopWords = new HashSet<string> {"АТП" };
        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { "СТАЛЬ", "КРУГ" },
            { "СТ", "КРУГ"}
        };
        private Dictionary<string, string> circleRegex = new Dictionary<string, string>
        {
            { @"ГР\s*\d{1,2}", ""},
            { @"Ф\s*(\d+)", @"НД $1"}
        };

        private IReplacementsStrategy replacementsStrategy;
        private IRegexReplacementStrategy regexReplacementStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public CirclesHandler(IReplacementsStrategy replacementsStrategy, IRegexReplacementStrategy regexReplacementStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.regexReplacementStrategy = regexReplacementStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Круги, шестигранники, квадраты" };
        public string AdditionalStringHandle(string str)
        {
            var replaced = replacementsStrategy.ReplaceItems(str, circleReplacements);
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced, circleRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;

        }
    }
}
