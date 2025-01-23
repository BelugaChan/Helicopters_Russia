using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class RivetsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Заклепки
        /// </summary>

        private readonly HashSet<string> stopWords = new HashSet<string> { "С", "СЕРДЕЧ", "СЕРДЕЧНИКОМ"};

        private IStopWordsStrategy stopWordsStrategy;
        public RivetsHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Заклепки" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return res;

        }
    }
}
