using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
namespace Algo.Handlers.ENS
{
    public class BarsAndTiresHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Прутки, шины из алюминия и сплавов
        /// Прутки, шины из меди и сплавов
        /// Прутки из титана и сплавов
        /// </summary>
        private readonly HashSet<string> stopWords = new () { "ПРЕСС", "ИЗ", "АЛЮМИНИЯ", "АЛЮМИНИЕВЫХ", "СПЛАВОВ", "АЛЮМИН","И", "КАТ", "КРУПНОГАБАРИТ", "ТИТАНОВЫЕ", "ТИТАНОВЫХ", "КОВАНЫЕ" };

        private IStopWordsStrategy stopWordsStrategy;

        public BarsAndTiresHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Прутки, шины из алюминия и сплавов", "Прутки, шины из меди и сплавов", "Прутки из титана и сплавов" ];

        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
    }
}
