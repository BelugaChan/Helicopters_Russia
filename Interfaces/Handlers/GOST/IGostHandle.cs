namespace Algo.Interfaces.Handlers.GOST
{
    public interface IGostHandle
    {
        HashSet<string> GetGOSTFromPositionName(string name);

        HashSet<string> GostsPostProcessor(HashSet<string> gosts);

        HashSet<string> RemoveLettersAndOtherSymbolsFromGosts(HashSet<string> gosts);
    }
}
