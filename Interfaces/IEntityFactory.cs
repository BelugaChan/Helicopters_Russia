using OfficeOpenXml;

namespace MinHash.Interfaces
{
    public interface IEntityFactory<T> where T : class
    {
        T CreateFromRow(ExcelWorksheet workSheet, int row);
    }
}
