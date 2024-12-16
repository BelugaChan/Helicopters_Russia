using Algo.Interfaces.Handlers.ENS;

namespace Algo.Handlers.ENS
{
    public class ConnectionPartsHandler : IAdditionalENSHandler<ConnectionPartsHandler>
    {
        /// <summary>
        /// Части соединительные
        /// </summary>
        private Dictionary<string, string> connectionReplacements = new Dictionary<string, string>
        {
            { "КОНТРАГАЙКА ", "КОНТРГАЙКА "},
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in connectionReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
