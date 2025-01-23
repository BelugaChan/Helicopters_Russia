using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class RodHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Катанка, проволока
        /// </summary>

        private string pattern = @"О.*\b\d{1,2}\b";

        private Dictionary<string, string> rodsReplacements = new Dictionary<string, string>
        {
            { @"ПРОВОЛОКА\s(\w{1,2}\s\d{1,2})", @"$1 ПРВ" },
        };

        private IRegexReplacementStrategy replacementsStrategy;
        public RodHandler(IRegexReplacementStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }

        public IEnumerable<string> SupportedKeys => [ "Катанка, проволока" ];
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            if (Regex.IsMatch(processingContext.Input, pattern))
            {
                processingContext.Input = "ОЛОВО " + processingContext.Input;
            }
            var res = replacementsStrategy.ReplaceItemsWithRegex(processingContext.Input, rodsReplacements,RegexOptions.None);
            return res;
        }
    }
}
