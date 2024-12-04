using Algo.Abstract;
using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class CalsibCirclesHandler : IAdditionalENSHandler<CalsibCirclesHandler>
    {
        /// <summary>
        /// Калиброванные круги, шестигранники, квадраты
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> {"АКБ","БЕЗНИКЕЛ", "СОДЕРЖ", "КЛ", "КАЛИБР", "ЛЕГИР", "ОБРАВ", "СТ", "НА", "ТОЧН", "МЕХОБР", "КАЧ", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КОНСТР", "КОНСТРУКЦ", "ОЦИНК", "НИК", "ЛЕГИР", "ИНСТР"};

        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { " КР "," КРУГ " },
            { " ШГ ", " ШЕСТИГРАННИК " },
            { "КРУГ В I", "КРУГ В 1 I"}
        };


        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in circleReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

            var tokens = str.Split(new[] { ' ', '/', '.'}, StringSplitOptions.RemoveEmptyEntries);
            
            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
