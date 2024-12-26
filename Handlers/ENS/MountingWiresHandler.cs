using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;

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
        public string AdditionalStringHandle(string str)
        {
            var res = replacementsStrategy.ReplaceItems(str, wiresReplacements);
            return res;

        }
    }
}
