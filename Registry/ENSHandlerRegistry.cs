using System.Collections.Concurrent;

namespace Algo.Registry
{
    public class ENSHandlerRegistry
    {
        private readonly ConcurrentDictionary<HashSet<string>, Delegate> handlers = new();

        public void RegisterHandler(string[] keys, Func<string,string> handler) 
        {
            var keySet = new HashSet<string>(keys);
            handlers[keySet] = handler;
        }

        public Func<string, string> GetHandler(string key)
        {
            var localKeys = handlers.Keys.ToList();
            foreach (var handlerKey in localKeys)
            {
                if (handlerKey.Contains(key))
                {
                    return (Func<string, string>)handlers[handlerKey];
                }
            }
            return null;
        }
    }
}
