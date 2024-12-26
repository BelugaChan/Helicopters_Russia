using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;

namespace Algo.Handlers.ENS
{
    public class InsulatingTubesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Трубки изоляционные гибкие
        /// </summary>
        protected static Dictionary<string, string> tubesReplacements = new Dictionary<string, string>
        {
            { "В С ","ВЫСШИЙ СОРТ " },
            {"ТРУБКА", "ТРУБКИ" }
        };

        private IReplacementsStrategy replacementsStrategy;
        public InsulatingTubesHandler(IReplacementsStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Трубки изоляционные гибкие" };

        public string AdditionalStringHandle(string str)
        {
            var res = replacementsStrategy.ReplaceItems(str, tubesReplacements);
            return res;

        }
    }
}
