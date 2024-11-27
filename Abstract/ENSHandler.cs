using Abstractions.Interfaces;
using Algo.Interfaces;
using MathNet.Numerics.Statistics;
using Pullenti.Morph;
using Pullenti.Ner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Abstract
{
    public abstract class ENSHandler : IENSHandler
    {
        //protected static HashSet<string> stopWords = new HashSet<string> { "СТ", "НА", "И", "ИЗ", "С", "СОДЕРЖ", "ТОЧН", "КЛ", "ШГ", "МЕХОБР", "КАЧ", "Х/Т", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КАЛИБР", "ХОЛ", "ПР", "ПРУЖ", "АВИАЦ", "КОНСТР", "КОНСТРУКЦ", "ПРЕЦИЗ", "СПЛ", "ПРЕСС", "КА4", "ОТВЕТСТВ", "НАЗНА4", "ОЦИНК", "НИК", "БЕЗНИКЕЛ", "ЛЕГИР", "АВТОМАТ", "Г/К", "КОРРОЗИННОСТОЙК", "Н/УГЛЕР", "ПРЕСС", "АЛЮМИН", "СПЛАВОВ" };

        //protected static string pattern = @"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])|(?<=[А-Яа-я])(?=\d)|(?<=\d)(?=[А-Яа-я])";
        protected static string pattern = @",0{1,3}$";
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
            { "OCT1","OCT 1" }
        };

        public abstract string AdditionalStringHandle(string str);
        public virtual string BaseStringHandle(string str) //базовая обработка строк
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            var fixedStr = str.ToUpper();
            string result = Regex.Replace(fixedStr, pattern, " ");
            foreach (var pair in replacements)
            {
                result = result.Replace(pair.Key, pair.Value);
            }
            result = result.TrimEnd(',');
            var tokens = result.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);

            //нормализация первого слова (приведение его к ед. числу)
            Processor processor = ProcessorService.CreateProcessor();
            AnalysisResult res = processor.Process(new SourceOfAnalysis(str));
            var firstWord = res.FirstToken;

            if (firstWord.Morph.Class.IsNoun)
            {
                tokens[0] = firstWord.GetNormalCaseText(MorphClass.Noun, MorphNumber.Singular);
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
