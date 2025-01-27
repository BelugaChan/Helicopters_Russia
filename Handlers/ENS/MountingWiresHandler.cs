using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{   
    public class MountingWiresHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Провода монтажные
        /// </summary>
        private Dictionary<string, string> wiresReplacements = new ()
        {
            { "КАБЕЛЬ","ПРОВОД" }
        };

        private IReplacementsStrategy replacementsStrategy;

        public MountingWiresHandler(IReplacementsStrategy replacementsStrategy) => this.replacementsStrategy = replacementsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Провода монтажные" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => replacementsStrategy.ReplaceItems(processingContext.Input, wiresReplacements);
    }
}
