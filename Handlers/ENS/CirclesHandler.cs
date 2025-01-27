using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CirclesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Круги, шестигранники, квадраты
        /// </summary>

        private HashSet<string> stopWords = new () { "КР","МЕХОБР", "ММ", "АТП", "АКБ", "БЕЗНИКЕЛ", "СОДЕРЖ", "КЛ", "КАЛИБР", "ЛЕГИР", "ОБРАВ", "СТ", "НА", "ТОЧН", "МЕХОБР", "КАЧ", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КОНСТР", "КОНСТРУКЦ", "ОЦИНК", "НИК", "ЛЕГИР", "ИНСТР" };
        private Dictionary<string, string> circleReplacements = new ()
        {
            { "СТАЛЬ", "КРУГ" }
            //{ "СТ ", "КРУГ "}
        };
        private Dictionary<string, string> circleRegex = new ()
        {
            { @"\s*СТ\s", @"КРУГ " },
            { @"(\d{1,3}) НД",@"НД $1"},
            { @"(Ø|АТП|КР)\s*(\d+)", @"НД $2"},
            { @"Ф\s*(\d+)$", @"НД $1"},
            { @"\sГ\s(\d{1,2})", @"НД $1"},
            { @"\sГР\s*(\d+)", @"$1 ГП"}
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
        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            string gostHandled = processingContext.Input;
            //try
            //{
            //    gostHandled = AdditionalStringHandleWithGost(processingContext.Input, processingContext.Gost);
            //}
            //catch (ArgumentException ex)
            //{
            //    gostHandled = processingContext.Input;
            //}
            

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
