using Algo.Interfaces.Handlers.GOST;

namespace Algo.Handlers.GOST
{
    public class GostRemover : IGostRemove
    {
        public string RemoveGosts(string positionName, HashSet<string> gosts)
        {
            var upperStr = positionName.ToUpper();
            foreach (var gost in gosts)
            {
                upperStr = upperStr.Replace(gost, "");
            }        
            return upperStr;
        }
    }
}
