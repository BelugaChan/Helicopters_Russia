
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class EnamelsHandler : IAdditionalENSHandler
    {
        private Dictionary<string, string> enamelsRegexReplacements = new Dictionary<string, string>
        {
            { @"\d\sБЛЕДНО\sЖЕЛТ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ БЛЕДНО ЖЕЛТЫЙ"},
            { @"\d\sСВЕТЛО\sЖЕЛТ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ СВЕТЛО ЖЕЛТЫЙ"},
            { @"\d\sСВЕТЛО\sБЕЖЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ СВЕТЛО БЕЖЕВЫЙ"},
            { @"\d\sСВЕТЛО\sСЕР(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ СВЕТЛО СЕРЫЙ"},
            { @"\d\sСЕРО\sГОЛУБ(?:АЯ|ОЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ СЕРО ГОЛУБОЙ"},
            { @"\d\sКРАСНО\sКОРИЧНЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ КРАСНО КОРИЧНЕВЫЙ" },
            { @"\d\sКРАСНО\sОРАНЖЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ КРАСНО ОРАНЖЕВЫЙ" },
            { @"\d\sТЕМНО\sЗЕЛЕН(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ТЕМНО ЗЕЛЕНЫЙ" },
            { @"\d\sЖЕЛТ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ЖЕЛТЫЙ"},
            { @"\d\sСЕР(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ СЕРЫЙ"},
            { @"\d\sСИН(?:ЯЯ|ИЙ|ЮЮ|ИЕ)$", "ПЕРВЫЙ СОРТ СИНИЙ"},
            { @"\d\sБЕЛ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ БЕЛЫЙ" },           
            { @"\d\sБЕЖЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ БЕЖЕВЫЙ" },
            { @"\d\sВИШНЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ВИШНЕВЫЙ" },
            { @"\d\sЗЕЛЕН(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ЗЕЛЕНЫЙ" },            
            { @"\d\sКОРИЧНЕВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ КОРИЧНЕВЫЙ" },
            { @"\d\sКРАСН(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ КРАСНЫЙ" },
            { @"\d\sКРЕМОВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ КРЕМОВЫЙ" },
            { @"\d\sФИСТАШКОВ(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ФИСТАШКОВЫЙ" },
            { @"\d\sЧЕРН(?:АЯ|ЫЙ|УЮ|ЫЕ)$", "ПЕРВЫЙ СОРТ ЧЕРНЫЙ" }
        };

        private IRegexReplacementStrategy regexReplacementStrategy;
        public EnamelsHandler(IRegexReplacementStrategy regexReplacementStrategy)
        {
            this.regexReplacementStrategy= regexReplacementStrategy;
        }
        public IEnumerable<string> SupportedKeys => ["Эмали"];

        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            var res = regexReplacementStrategy.ReplaceItemsWithRegex(processingContext.Input, enamelsRegexReplacements, RegexOptions.None);
            return res;
        }
    }
}
