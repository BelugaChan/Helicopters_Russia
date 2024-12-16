namespace Algo.Interfaces.Handlers.GOST
{
    public interface IGostRemove
    {
        string RemoveGosts(string positionName, HashSet<string> gosts);
    }
}
