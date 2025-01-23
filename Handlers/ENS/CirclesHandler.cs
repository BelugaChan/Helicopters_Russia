using Algo.Interfaces.Factory;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CirclesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Круги, шестигранники, квадраты
        /// </summary>

        private HashSet<string> stopWords = new HashSet<string> { "КР","МЕХОБР","Ф", "ММ", "АТП", "АКБ", "БЕЗНИКЕЛ", "СОДЕРЖ", "КЛ", "КАЛИБР", "ЛЕГИР", "ОБРАВ", "СТ", "НА", "ТОЧН", "МЕХОБР", "КАЧ", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КОНСТР", "КОНСТРУКЦ", "ОЦИНК", "НИК", "ЛЕГИР", "ИНСТР" };
        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { "СТАЛЬ", "КРУГ" }
            //{ "СТ ", "КРУГ "}
        };
        private Dictionary<string, string> circleRegex = new Dictionary<string, string>
        {
            { @"\s*СТ\s", @"КРУГ " },
            { @"(\d{1,3}) НД",@"НД $1"},
            { @"(Ф|Ø|АТП|КР)\s*(\d+)", @"НД $2"},
            { @"Г\s(\d{1,2})", @"НД $1"},
            { @"ГР\s*(\d+)", @"$1 ГП"}
        };

        private IReplacementsStrategy replacementsStrategy;
        private IRegexReplacementStrategy regexReplacementStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        private IProcessingGostStrategyFactory strategyFactory;
        public CirclesHandler(IReplacementsStrategy replacementsStrategy, IRegexReplacementStrategy regexReplacementStrategy, IStopWordsStrategy stopWordsStrategy, IProcessingGostStrategyFactory strategyFactory)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.regexReplacementStrategy = regexReplacementStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
            this.strategyFactory = strategyFactory;
        }

        public IEnumerable<string> SupportedKeys => [ "Круги, шестигранники, квадраты" ];
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            string gostHandled = "";
            try
            {
                gostHandled = AdditionalStringHandleWithGost(processingContext.Input, processingContext.Gost);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                gostHandled = processingContext.Input;
            }
            

            var replaced = replacementsStrategy.ReplaceItems(gostHandled, circleReplacements);
            //string specialRegexReplacement = Regex.Replace(replaced, @"ГР\s*(1|2|3)",
            //    match => match.Groups[1].Value == "1" ? "I" :
            //             match.Groups[1].Value == "2" ? "II" : "III");
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced, circleRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;

        }

        public string AdditionalStringHandleWithGost(string str, string gost)
        {
            var strategy = strategyFactory.GetStrategy(gost);
            return strategy.HandleWithExactGost(str);
        }
    }
}
