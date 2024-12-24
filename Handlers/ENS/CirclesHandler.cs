using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CirclesHandler : IAdditionalENSHandler<CirclesHandler>
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
        public string AdditionalStringHandle(string str)
        {
            var replaced = replacementsStrategy.ReplaceItems(str, circleReplacements);
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced, circleRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;
            //StringBuilder sb = new StringBuilder();

            //foreach (var pair in circleRegex)
            //{
            //    str = Regex.Replace(str, pair.Key, pair.Value);
            //}

            //foreach (var pair in circleReplacements)
            //{
            //    str = str.Replace(pair.Key, pair.Value);
            //}

            //var tokens = str.Split(new[] { ' ', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);

            //var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            //for (int i = 0; i < filteredTokens.Count; i++)
            //{
            //    sb.Append($"{filteredTokens[i]} ");
            //}
            //return sb.ToString().TrimEnd(' ');
        }
    }
}
