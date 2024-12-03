using Algo.Interfaces.Handlers.GOST;

namespace Algo.Handlers.GOST
{
    public class GostRemover : IGostRemove
    {
        public string RemoveGosts(string positionName, HashSet<string> gosts)
        {
            foreach (var gost in gosts)
            {
                positionName = positionName.Replace(gost, "");
            }        
            return positionName;
        }
    }
}
