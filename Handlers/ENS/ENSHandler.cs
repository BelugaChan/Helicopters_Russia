using Abstractions.Interfaces;
using Algo.Interfaces.Handlers.ENS;
using MathNet.Numerics.Statistics;
using Pullenti.Morph;
using Pullenti.Ner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class ENSHandler : IENSHandler
    {
        //protected static HashSet<string> stopWords = new HashSet<string> { "СТ", "НА", "И", "ИЗ", "С", "СОДЕРЖ", "ТОЧН", "КЛ", "ШГ", "МЕХОБР", "КАЧ", "Х/Т", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КАЛИБР", "ХОЛ", "ПР", "ПРУЖ", "АВИАЦ", "КОНСТР", "КОНСТРУКЦ", "ПРЕЦИЗ", "СПЛ", "ПРЕСС", "КА4", "ОТВЕТСТВ", "НАЗНА4", "ОЦИНК", "НИК", "БЕЗНИКЕЛ", "ЛЕГИР", "АВТОМАТ", "Г/К", "КОРРОЗИННОСТОЙК", "Н/УГЛЕР", "ПРЕСС", "АЛЮМИН", "СПЛАВОВ" };

        //protected static string pattern = @"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])|(?<=[А-Яа-я])(?=\d)|(?<=\d)(?=[А-Яа-я])";
        protected static Dictionary<string, string> pattern = new Dictionary<string, string>()
        {
            { @",0{1,3}", " " },
            { @"\.0{1,3}", " " },// \. - экранирование точки {1,3} - число длиной от 1 до 3 цифр
            { @"(?<=\.)\d{1,2}\b", "" }, //\d - любое число (digit?)
            { @"(\d)([a-zA-Zа-яА-Я])|([a-zA-Zа-яА-Я])(\d)", "$1$3 $2$4"},//добавление пробела в следующих случаях буква+цифра или цифра+буква без пробелов. $ означает группы, которые находятся в круглых () скобках
            //{ @"Г(\d+)", "ГOCT $1" }
            //{ @"Г\d+-\d+-\d+-\d+", @"ТУ\d+-\d+-\d+-\d+" }
        };
        //protected static string pattern = @",0{1,3}$";
        protected static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "А","A" },
            { "В","B" },
            { "Е","E" },
            { "К", "K" },
            { "М", "M" },
            { "Н", "H" },
            { "О", "O" },
            { "Р", "P" },
            { "С", "C" },
            { "Т", "T" },
            { "У", "Y" },
            { "Х", "X" },
            { "X/T", "" },
            { "Г/K", "" },
            { "В/КА4", "" },
            { "В/КАЧ", "" },
            //{ "OCT1","OCT 1" },
            { "'", "" },
            {"\r\n", "" }
        };

        public string BaseStringHandle(string str) //базовая обработка строк
        {
            StringBuilder stringBuilder = new StringBuilder();

            //string upgradedGost = Regex.Replace(str, @"Г(\d+)", "ГOCT $1");

            var fixedStr = str.ToUpper();


            foreach (var item in pattern)
            {
                fixedStr = Regex.Replace(fixedStr, item.Key, item.Value);
            }
            //string result = Regex.Replace(fixedStr, pattern, " ");
            foreach (var pair in replacements)
            {
                fixedStr = Regex.Replace(fixedStr, pair.Key, pair.Value);
            }
            fixedStr = fixedStr.TrimEnd(',');
            var tokens = fixedStr.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);

            //нормализация первого слова (приведение его к ед. числу)
            Processor processor = ProcessorService.CreateProcessor();
            AnalysisResult res = processor.Process(new SourceOfAnalysis(str));
            var firstWord = res.FirstToken;

            if (firstWord.Morph.Class.IsNoun/* && firstWord.Morph.Number == MorphNumber.Plural*/)
            {
                var normalizedName = firstWord.GetNormalCaseText(MorphClass.Noun, MorphNumber.Singular);//приведение существительного на первой позиции к единственному числу
                foreach (var item in replacements)
                {
                    fixedStr = Regex.Replace(normalizedName, item.Key, item.Value);
                }
                tokens[0] = normalizedName;/*firstWord.GetNormalCaseText(MorphClass.Noun, MorphNumber.Singular);*/
            }

            //var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();

            //if ("AEЁИOYЭЫЯ".IndexOf(tokens[0][tokens[0].Length - 1]) >= 0)
            //{
            //    tokens[0] = tokens[0].Substring(0, tokens[0].Length - 1);
            //}
            for (int i = 0; i < tokens.Length; i++)
            {
                stringBuilder.Append($"{tokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
