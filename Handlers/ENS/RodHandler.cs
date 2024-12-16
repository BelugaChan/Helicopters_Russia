using Algo.Interfaces.Handlers.ENS;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class RodHandler : IAdditionalENSHandler<RodHandler>
    {
        /// <summary>
        /// Катанка, проволока
        /// Катанка, проволока из меди и сплавов
        /// </summary>

        private string pattern = @"О.*\b\d{1,2}\b";

        private Dictionary<string, string> rodsReplacements = new Dictionary<string, string>
        {
            { "ПРОВОЛОКА", "ПРВ" },
        };
        public string AdditionalStringHandle(string str)
        {
            if (Regex.IsMatch(str,pattern))
            {
                str = "ОЛОВО " + str;
            }
            foreach (var pair in rodsReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
