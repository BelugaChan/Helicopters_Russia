
namespace Algo.Registry
{
    public class ENSHandlerRegistry
    {
        private readonly Dictionary<string, Func<string, string>> handlers = new();

        public void RegisterHandler(IEnumerable<string> keys, Func<string,string> handler) 
        {
            foreach (var key in keys)
            {
                handlers[key] = handler;
            }
        }
        public Func<string, string> GetHandler(string key)
        {
            return handlers.TryGetValue(key, out var handler) ? handler : null;
        }
    }
}
