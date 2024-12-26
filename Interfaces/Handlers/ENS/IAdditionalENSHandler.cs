namespace Algo.Interfaces.Handlers.ENS
{
    public interface IAdditionalENSHandler
    {
        IEnumerable<string> SupportedKeys { get; }
        string AdditionalStringHandle(string str);
    }
}
