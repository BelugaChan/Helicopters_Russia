namespace MinHash.Interfaces
{
    public interface IExcelReader
    {
        List<T> CreateCollectionFromExcel<T>(string filePath, IEntityFactory<T> factory)
            where T : class;
    }
}
