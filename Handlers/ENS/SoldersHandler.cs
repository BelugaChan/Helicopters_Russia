﻿using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class SoldersHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Припои (прутки, проволока, трубки)
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "ОЛОВЯННОСВИНЦОВЫЕ", "В", "ПРОВОЛОКЕ", "ОЛОВЯННО", "СВИНЦОВЫЙ"};

        private Dictionary<string, string> soldersReplacements = new Dictionary<string, string>
        {
            { "ПРВ КР", "ПРВКР" },
        };

        private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public SoldersHandler(IReplacementsStrategy replacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Припои (прутки, проволока, трубки)" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var mid = replacementsStrategy.ReplaceItems(processingContext.Input, soldersReplacements);
            var final = stopWordsStrategy.RemoveWords(mid, stopWords);
            return final;

        }
    }
}
