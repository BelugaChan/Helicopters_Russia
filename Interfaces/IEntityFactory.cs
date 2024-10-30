using NPOI.SS.UserModel;
using OfficeOpenXml;

namespace MinHash.Interfaces
{
    public interface IEntityFactory<T> where T : class
    {
        T CreateFromRow(IRow row);
    }
}
