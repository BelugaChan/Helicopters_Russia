using Algo.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class CalsibCirclesHandler : ENSHandler
    {
        private HashSet<string> stopWords = new HashSet<string> {"БE3HИKEЛ", "COДEPЖ", "KЛ", "KAЛИБP", "ЛEГИP", "OБPAB", "CT", "HA", "TOЧH", "TO4H", "MEXOБP", "KAЧ", "YГЛEP", "COPT", "HEPЖ", "HCPЖ", "KAЛИБP", "KOHCTP", "КОНСТРУКЦ", "KA4", "HAЗHA4", "OЦИHK", "HИK", "ЛEГИP", "ИHCTP"};

        protected static Dictionary<string, string> pattern = new Dictionary<string, string>
        {
            { @"ШГ\s?\d{1,2}","ШГ " }
        };

        protected static Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { " KP "," KPYГ " },
            { " ШГ ", " ШECTИГPAHHИK " }
        };


        public override string AdditionalStringHandle(string str)
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
