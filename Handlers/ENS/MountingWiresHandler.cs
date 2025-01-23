using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;

namespace Algo.Handlers.ENS
{   
    public class MountingWiresHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Провода монтажные
        /// </summary>
        private Dictionary<string, string> wiresReplacements = new Dictionary<string, string>
        {
            { "КАБЕЛЬ","ПРОВОД" }
        };

        private IReplacementsStrategy replacementsStrategy;

        public MountingWiresHandler(IReplacementsStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Провода монтажные" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = replacementsStrategy.ReplaceItems(processingContext.Input, wiresReplacements);
            return res;

        }
    }
}
