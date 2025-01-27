using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class RodCopperHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Катанка, проволока из меди и сплавов
        /// </summary>
        public string AdditionalStringHandle(ProcessingContext processingContext) => processingContext.Input;// заглушка, так как наименование "Катанка, проволока" более короткое, чем "Катанка, проволока из меди и сплавов" => "Катанка, проволока из меди и сплавов" - будет частным случаем при текущей логике регистрации обработчиков

        public IEnumerable<string> SupportedKeys => new[] { "Катанка, проволока из меди и сплавов" };
    }
}
