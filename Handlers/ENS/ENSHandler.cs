using AbstractionsAndModels.Interfaces.Handlers.ENS;
using Pullenti.Morph;
using Pullenti.Ner;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class ENSHandler : IENSHandler
    {
        protected static Dictionary<string, string> pattern = new ()
        {
            { @",0{1,3}", " " },
            { @"\.0{1,3}", " " },// \. - экранирование точки {1,3} - число длиной от 1 до 3 цифр
            //{ @"\b0[.,](\d+)\b", "$1" },
            /*{ @"(\d)([a-zA-Zа-яА-Я])|([a-zA-Zа-яА-Я])(\d)", "$1$3 $2$4"}*/
            { @"(?<=[a-zA-Zа-яА-Я])(?=\d)|(?<=\d)(?=[a-zA-Zа-яА-Я])", " "},//добавление пробела в следующих случаях буква+цифра или цифра+буква без пробелов. $ означает группы, которые находятся в круглых () скобках
        };
        protected static Dictionary<string, string> replacements = new ()
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
            { "*", "Х" },
            { "Х/Т", "" },
            { "Х/К", "" },
            { "Г/К", "" },
            { "Н/Д", ""},
            { "В/КА4", "" },
            { "НАЗНА4", "" },
            { "НА3НА4", ""},
            { "КА4", ""},
            { "ТО4Н", "" },
            { "2АКЛ", "" },
            { "5КЛ", ""},
            { "И3", "" },
            { "БЕ3НИКЕЛ", ""},
            {"Н/УГЛЕР", "" },
            {"Н/СОДЕРЖ","" },
            { "В/КАЧ", "" },
            { "ГВО3ДИ", "ГВОЗДИ" },
            { "НЕТ ПО ГОСТУ", "" },
            { "'", "" },
            { "\"", ""},
            {"\r\n", "" },
            {"\u0098", "" }
            //{"Х", "-" }
        };

        public string BaseStringHandle(string str) //базовая обработка строк
        {
            StringBuilder stringBuilder = new();

            var fixedStr = str.ToUpper();

            foreach (var pair in replacements)
            {
                fixedStr = fixedStr.Replace(pair.Key, pair.Value);
            }

            foreach (var item in pattern)
            {
                fixedStr = Regex.Replace(fixedStr, item.Key, item.Value);
            }
            
            fixedStr = fixedStr.TrimEnd(',').Replace(',','.');
            var tokens = fixedStr.Split([' ', '.', '-','/' ], StringSplitOptions.RemoveEmptyEntries);

            //нормализация первого слова (приведение его к ед. числу)
            Processor processor = ProcessorService.CreateProcessor();
            AnalysisResult res = processor.Process(new SourceOfAnalysis(str));
            var firstWord = res.FirstToken;

            if (firstWord.Morph.Class.IsNoun)
            {
                var normalizedName = firstWord.GetNormalCaseText(MorphClass.Noun, MorphNumber.Singular);//приведение существительного на первой позиции к единственному числу
                foreach (var item in replacements)
                {
                    fixedStr = fixedStr.Replace(item.Key, item.Value);
                }
                tokens[0] = normalizedName;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                stringBuilder.Append($"{tokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
