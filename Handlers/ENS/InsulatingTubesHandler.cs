using Algo.Interfaces.Handlers.ENS;

namespace Algo.Handlers.ENS
{
    public class InsulatingTubesHandler : IAdditionalENSHandler<InsulatingTubesHandler>
    {
        /// <summary>
        /// Трубки изоляционные гибкие
        /// </summary>
        protected static Dictionary<string, string> tubesReplacements = new Dictionary<string, string>
        {
            { "В С ","ВЫСШИЙ СОРТ " },
            {"ТРУБКА", "ТРУБКИ" }
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in tubesReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

            return str;
        }
    }
}
