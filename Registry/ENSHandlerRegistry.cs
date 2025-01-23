
using Algo.Models;

namespace Algo.Registry
{
    public class ENSHandlerRegistry
    {
        private readonly Dictionary<string, Func<ProcessingContext, string>> handlers = new();

        public void RegisterHandler(IEnumerable<string> keys, Func<ProcessingContext, string> handler) 
        {
            foreach (var key in keys)
            {
                handlers[key] = handler;
            }
        }
        public Func<ProcessingContext, string> GetHandler(string key)
        {
            return handlers.TryGetValue(key, out var handler) ? handler : null;
        }
    }
}
