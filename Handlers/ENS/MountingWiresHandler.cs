using Algo.Interfaces.Handlers.ENS;

namespace Algo.Handlers.ENS
{
    /// <summary>
    /// Провода монтажные
    /// </summary>
    public class MountingWiresHandler : IAdditionalENSHandler<MountingWiresHandler>
    {
        private Dictionary<string, string> wiresReplacements = new Dictionary<string, string>
        {
            { "КАБЕЛЬ","ПРОВОД" }
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in wiresReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
