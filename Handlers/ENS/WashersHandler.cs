using Algo.Interfaces.Handlers.ENS;
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
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in washersReplacements)
            {
                str = Regex.Replace(str, pair.Key, pair.Value);
            }
            return str;
        }
    }
}
