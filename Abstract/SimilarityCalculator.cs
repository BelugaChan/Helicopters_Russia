using Abstractions.Interfaces;
using Algo.Interfaces;
using Algo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Abstract
{
    public abstract class SimilarityCalculator : ISimilarityCalculator
    {
        protected int totalGarbageDataItems = 0;

        protected int currentProgress = 0;

        //remove it from here!
        //protected static HashSet<string> stopWords = new HashSet<string> { "СТ", "НА", "И", "ИЗ", "С", "СОДЕРЖ", "ТОЧН", "КЛ", "ШГ", "МЕХОБР", "КАЧ", "Х/Т", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КАЛИБР", "ХОЛ", "ПР", "ПРУЖ", "АВИАЦ", "КОНСТР", "КОНСТРУКЦ", "ПРЕЦИЗ", "СПЛ", "ПРЕСС", "КА4", "ОТВЕТСТВ", "НАЗНА4", "ОЦИНК", "НИК", "БЕЗНИКЕЛ", "ЛЕГИР", "АВТОМАТ", "Г/К", "КОРРОЗИННОСТОЙК", "Н/УГЛЕР", "ПРЕСС", "АЛЮМИН", "СПЛАВОВ" };

        //protected static string pattern = @"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])|(?<=[А-Яа-я])(?=\d)|(?<=\d)(?=[А-Яа-я])";

        //protected static Dictionary<string, string> replacements = new Dictionary<string, string>
        //{
        //    { "А","A" },
        //    { "В","B" },
        //    { "Е","E" },
        //    { "К", "K" },
        //    { "М", "M" },
        //    { "Н", "H" },
        //    { "О", "O" },
        //    { "Р", "P" },
        //    { "С", "C" },
        //    { "Т", "T" },
        //    { "У", "Y" },
        //    { "Х", "X" },
        //    { "OCT1","OCT 1" }
        //};

        public abstract (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary</*ConcurrentDictionary<string, int>*/string, TStandart>>>> data)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        public double GetProgress() 
        {
            return (double)currentProgress * 100 / totalGarbageDataItems;
        }

        //public abstract ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> HandleStandarts<TStandart>(List<TStandart> standarts)
        //    where TStandart : IStandart;
    }
}
