using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class RopesAndCablesHandler : IAdditionalENSHandler<RopesAndCablesHandler>
    {
        private HashSet<string> stopWords = new HashSet<string> {"CT","ABИAЦ","KOHCTP", "TИПA","TK" };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var tokens = str.Split(new[] { ' ', '/', '.',',' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
