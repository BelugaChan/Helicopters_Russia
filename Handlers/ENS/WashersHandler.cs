using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class WashersHandler : IAdditionalENSHandler<WashersHandler>
    {
        /// <summary>
        /// Шайбы
        /// </summary>

        private Dictionary<string, string> washersReplacements = new Dictionary<string, string>
        {
            { @"\s\d{9}$","" },
        };
        private IReplacementsStrategy replacementsStrategy;
        public WashersHandler(IReplacementsStrategy replacementsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
        }
        public string AdditionalStringHandle(string str)
        {
            var res = replacementsStrategy.ReplaceItems(str, washersReplacements);
            return res;
            //foreach (var pair in washersReplacements)
            //{
            //    str = Regex.Replace(str, pair.Key, pair.Value);
            //}
            //return str;
        }
    }
}
