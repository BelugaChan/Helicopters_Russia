using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
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
        private IReplacementsStrategy replacementsStrategy;
        public RodHandler(IReplacementsStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }
        public string AdditionalStringHandle(string str)
        {
            if (Regex.IsMatch(str,pattern))
            {
                str = "ОЛОВО " + str;
            }
            var res = replacementsStrategy.ReplaceItems(str, rodsReplacements);
            return res;
            //foreach (var pair in rodsReplacements)
            //{
            //    str = str.Replace(pair.Key, pair.Value);
            //}
            //return str;
        }
    }
}
