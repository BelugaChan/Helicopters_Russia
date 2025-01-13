using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
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

        //private Dictionary<string, string> barsReplacements = new Dictionary<string, string>
        //{
        //    { "ТРУБА ТР","ТРУБА " },
        //};

        //private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public PipesHandler(/*IReplacementsStrategy replacementsStrategy,*/ IStopWordsStrategy stopWordsStrategy)
        {
            //this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Трубы бесшовные", "Трубы сварные", "Трубы, трубки из алюминия и сплавов", "Трубы, трубки из меди и сплавов" };
        public string AdditionalStringHandle(string str)
        {
            //var midRes = replacementsStrategy.ReplaceItems(str, barsReplacements);
            var final = stopWordsStrategy.RemoveWords(str/*midRes*/, stopWords);
            return final;

        }
    }
}
