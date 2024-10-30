using MinHash.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.ExcelReaders
{
    public class NPOIReader : IExcelReader
    {
        public List<T> CreateCollectionFromExcel<T>(string filePath, IEntityFactory<T> factory) where T : class
        {
            List<T> objects = new List<T>();

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            IWorkbook workbook = new XSSFWorkbook(fileStream);
            ISheet sheet = workbook.GetSheetAt(0);

            int rowCount = sheet.LastRowNum;

            for (int row = 1; row <= rowCount; row++)
            {
                IRow currentRow = sheet.GetRow(row);
                if (currentRow == null)
                    continue; 

                var obj = factory.CreateFromRow(currentRow);
                objects.Add(obj);
            }

            return objects;
        }
    }
}
