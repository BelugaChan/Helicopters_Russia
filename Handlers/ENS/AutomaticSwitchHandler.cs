
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    internal class AutomaticSwitchHandler : IAdditionalENSHandler
    {
        private Dictionary<string, string> switchRegex = new Dictionary<string, string>
        {
            { @"(\d{1,2})\sП\s", @"$1 P " }
        };

        private IRegexReplacementStrategy regexReplacementStrategy;
        public AutomaticSwitchHandler(IRegexReplacementStrategy regexReplacementStrategy)
        {
            this.regexReplacementStrategy = regexReplacementStrategy;
        }
        public IEnumerable<string> SupportedKeys => throw new NotImplementedException();

        public string AdditionalStringHandle(string str)
        {
            var replaced = regexReplacementStrategy.ReplaceItemsWithRegex(str, switchRegex, RegexOptions.None);
            return replaced;
        }
    }
}
