using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class RodCopperHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Катанка, проволока из меди и сплавов
        /// </summary>
        public string AdditionalStringHandle(string str) // заглушка, так как наименование "Катанка, проволока" более короткое, чем "Катанка, проволока из меди и сплавов" => "Катанка, проволока из меди и сплавов" - будет частным случаем при текущей логике регистрации обработчиков
        {
            return str;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Катанка, проволока из меди и сплавов" };
    }
}
