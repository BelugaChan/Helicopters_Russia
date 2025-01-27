using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    internal class AutomaticSwitchHandler : IAdditionalENSHandler
    {
        private Dictionary<string, string> switchRegex = new ()
        {
            { @"(\d{1,2})\sП\s", @"$1 P " }
        };

        private IRegexReplacementStrategy regexReplacementStrategy;
        public AutomaticSwitchHandler(IRegexReplacementStrategy regexReplacementStrategy) => this.regexReplacementStrategy = regexReplacementStrategy;

        public IEnumerable<string> SupportedKeys => throw new NotImplementedException();

        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/) => regexReplacementStrategy.ReplaceItemsWithRegex(processingContext.Input, switchRegex, RegexOptions.None);
    }
}
