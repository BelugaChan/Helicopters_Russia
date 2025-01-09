using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CalsibCirclesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Калиброванные круги, шестигранники, квадраты
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> {"Ф","ММ","АТП","АКБ","БЕЗНИКЕЛ", "СОДЕРЖ", "КЛ", "КАЛИБР", "ЛЕГИР", "ОБРАВ", "СТ", "НА", "ТОЧН", "МЕХОБР", "КАЧ", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КОНСТР", "КОНСТРУКЦ", "ОЦИНК", "НИК", "ЛЕГИР", "ИНСТР"};

        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { "ПРУТОК", "КРУГ"},
            { " КР ", " КРУГ "},
            { " ШГ ", " ШЕСТИГРАННИК " },
            { " ШЕСТИГРАН ", " ШЕСТИГРАННИК " },
            { "КРУГ В I", "КРУГ В 1 I"}
        };

        private Dictionary<string, string> circleRegex = new Dictionary<string, string>
        {
            { @"\b\d{1,2}\sКЛ\b", ""},
            { @"(КР.*)(КР)", "$1"},//удаление повторений КР и ШГ
            { @"(ШГ.*)(ШГ)", "$1"},
            { @"\bКР\s*\d+","КРУГ" },
            { @"\bШГ\s*\d+","ШЕСТГРАННИК" },
            { @"\bВ\s*Н\s*(\d+)\s*АТП?\s*Ф?\s*(\d+)\b", "Н $1 НД $2" }
        };

        private IReplacementsStrategy replacementsStrategy;
        private IRegexReplacementStrategy regexReplacementStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public CalsibCirclesHandler(IReplacementsStrategy replacementsStrategy, IRegexReplacementStrategy regexReplacementStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.regexReplacementStrategy = regexReplacementStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Калиброванные круги, шестигранники, квадраты" };
        public string AdditionalStringHandle(string str)
        {
            var replaced = replacementsStrategy.ReplaceItems(str, circleReplacements);
            var regexReplaced = regexReplacementStrategy.ReplaceItemsWithRegex(replaced, circleRegex, RegexOptions.None);
            var final = stopWordsStrategy.RemoveWords(regexReplaced, stopWords);
            return final;

        }
    }
}
