using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WashersHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Шайбы
        /// </summary>

        private Dictionary<string, string> washersReplacements = new Dictionary<string, string>
        {
            { @"\s\d{9}\b","" }
        };

        private IRegexReplacementStrategy regexReplacementsStrategy;
        public WashersHandler(IRegexReplacementStrategy regexReplacementsStrategy)
        {
            this.regexReplacementsStrategy = regexReplacementsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Шайбы" };
        public string AdditionalStringHandle(string str)
        {
            var res = regexReplacementsStrategy.ReplaceItemsWithRegex(str, washersReplacements,RegexOptions.None);
            return res;

        }
    }
}
