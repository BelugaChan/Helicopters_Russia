namespace MinHash.Interfaces
{
    public interface IExcelWriter
    {
        void WriteCollectionsToExcel<TGarbageData>(HashSet<TGarbageData> bad, HashSet<TGarbageData> mid, HashSet<TGarbageData> high, string savePath)
            where TGarbageData : IGarbageData;
    }
}
