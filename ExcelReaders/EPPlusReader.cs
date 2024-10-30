using MinHash.Interfaces;
using OfficeOpenXml;

namespace MinHash.ExcelReaders
{
    public class EPPlusReader : IExcelReader
    {
        public List<T> CreateCollectionFromExcel<T>(string filePath, IEntityFactory<T> factory)
            where T : class
        {
            List<T> objects = new List<T>();
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row < rowCount; row++)
            {
                var obj = factory.CreateFromRow(worksheet, row);
                objects.Add(obj);
            }
            return objects;
        }
    }
}
