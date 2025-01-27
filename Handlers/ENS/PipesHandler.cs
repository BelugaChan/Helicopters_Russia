using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class PipesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Трубы бесшовные
        /// Трубы сварные
        /// Трубы, трубки из алюминия и сплавов
        /// Трубы, трубки из меди и сплавов
        /// </summary>
        private HashSet<string> stopWords = new () { "КОНСТРУКЦ", "И", "ХОЛ", "ТЕПЛОДЕФОРМИРОВАННЫЕ", "КВАДРАТНАЯ", "ПРЯМОУГОЛЬНАЯ", "ПРОФИЛЬНАЯ","СВАРНЫЕ","ЭЛЕКТРОСВАРНЫЕ","ТР" };

        private IStopWordsStrategy stopWordsStrategy;
        public PipesHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Трубы бесшовные", "Трубы сварные", "Трубы, трубки из алюминия и сплавов", "Трубы, трубки из меди и сплавов" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
    }
}
