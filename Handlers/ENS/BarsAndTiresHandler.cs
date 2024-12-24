using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.MethodStrategy;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class BarsAndTiresHandler : IAdditionalENSHandler<BarsAndTiresHandler>
    {
        /// <summary>
        /// Прутки, шины из алюминия и сплавов
        /// Прутки, шины из меди и сплавов
        /// Прутки из титана и сплавов
        /// </summary>
        private readonly HashSet<string> stopWords = new HashSet<string> { "ПРЕСС", "ИЗ", "АЛЮМИНИЯ", "АЛЮМИНИЕВЫХ", "СПЛАВОВ", "АЛЮМИН","И", "КАТ", "КРУПНОГАБАРИТ", "ТИТАНОВЫЕ", "ТИТАНОВЫХ", "КОВАНЫЕ" };

        private IStopWordsStrategy stopWordsStrategy;

        public BarsAndTiresHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public string AdditionalStringHandle(string str)
        {
            var res = stopWordsStrategy.RemoveWords(str, stopWords);
            return res;
        }
    }
}
