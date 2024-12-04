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
        //protected static string pattern = @"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])|(?<=[А-Яа-я])(?=\d)|(?<=\d)(?=[А-Яа-я])";
        protected static Dictionary<string, string> pattern = new Dictionary<string, string>()
        {
            { @",0{1,3}", " " },
            { @"\.0{1,3}", " " },// \. - экранирование точки {1,3} - число длиной от 1 до 3 цифр
            { @"\b0[.,](\d+)\b", "$1" },
            { @"\b(\d+)[.,]\d+\b", "$1" },            
            { @"(\d)([a-zA-Zа-яА-Я])|([a-zA-Zа-яА-Я])(\d)", "$1$3 $2$4"},//добавление пробела в следующих случаях буква+цифра или цифра+буква без пробелов. $ означает группы, которые находятся в круглых () скобках
            //{ @"Г(\d+)", "ГOCT $1" }
            //{ @"Г\d+-\d+-\d+-\d+", @"ТУ\d+-\d+-\d+-\d+" }
        };
        protected static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "A" , "А" },
            { "B" , "В" },
            { "E" , "Е" },
            { "K" , "К" },
            { "M" , "М" },
            { "H" , "Н" },
            { "O" , "О" },
            { "P" , "Р" },
            { "C","С" },
            { "T","Т" },
            { "Y","У" },
            { "X","Х" },
            { "4", "Ч"},
            { "3", "З"},
            { "Х/Т", "" },
            { "Г/К", "" },
            { "В/КАЧ", "" },
            {"Н/УГЛЕР", "" },
            { "'", "" },
            {"\r\n", "" }
        };

        public string BaseStringHandle(string str) //базовая обработка строк
        {
            StringBuilder stringBuilder = new StringBuilder();

            //string upgradedGost = Regex.Replace(str, @"Г(\d+)", "ГOCT $1");

            var fixedStr = str.ToUpper();

            foreach (var pair in replacements)
            {
                fixedStr = Regex.Replace(fixedStr, pair.Key, pair.Value);
            }

            foreach (var item in pattern)
            {
                fixedStr = Regex.Replace(fixedStr, item.Key, item.Value);
            }
            
            fixedStr = fixedStr.TrimEnd(',').Replace(',','.');
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
