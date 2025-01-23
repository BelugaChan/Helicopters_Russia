using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private HashSet<string> stopWords = new HashSet<string> { "КОНСТРУКЦ", "И", "ХОЛ", "ТЕПЛОДЕФОРМИРОВАННЫЕ", "КВАДРАТНАЯ", "ПРЯМОУГОЛЬНАЯ", "ПРОФИЛЬНАЯ","СВАРНЫЕ","ЭЛЕКТРОСВАРНЫЕ","ТР" };

        private IStopWordsStrategy stopWordsStrategy;
        public PipesHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Трубы бесшовные", "Трубы сварные", "Трубы, трубки из алюминия и сплавов", "Трубы, трубки из меди и сплавов" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var final = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return final;

        }
    }
}
