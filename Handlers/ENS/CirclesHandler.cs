using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
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
            { @"Ф\s*(\d+)", @"B 1 НД $1"}
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
            string specialRegexReplacement = Regex.Replace(replaced, @"ГР\s*(1|2|3)",
                match => match.Groups[1].Value == "1" ? "I" :
                         match.Groups[1].Value == "2" ? "II" : "III");
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(specialRegexReplacement, circleRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;

        }
    }
}
