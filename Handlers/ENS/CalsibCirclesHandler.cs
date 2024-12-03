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
        private HashSet<string> stopWords = new HashSet<string> {"AKБ","БE3HИKEЛ", "COДEPЖ", "KЛ", "KAЛИБP", "ЛEГИP", "OБPAB", "CT", "HA", "TOЧH", "TO 4H", "MEXOБP", "KAЧ", "YГЛEP", "COPT", "HEPЖ", "HCPЖ", "KAЛИБP", "KOHCTP", "КОНСТРУКЦ", "KA4", "HAЗHA4", "OЦИHK", "HИK", "ЛEГИP", "ИHCTP"};

        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { " KP "," KPYГ " },
            { " ШГ ", " ШECTИГPAHHИK " },
            { "KPYГ B I", "KPYГ B 1 I"}
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
