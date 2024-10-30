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
    public class NPOIWriter : IExcelWriter
    {
        public void WriteCollectionsToExcel<TGarbageData>(HashSet<TGarbageData> bad, HashSet<TGarbageData> mid, HashSet<TGarbageData> high, string savePath) where TGarbageData : IGarbageData
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Data Report");

            IRow headerRow = sheet.CreateRow(1);
            headerRow.CreateCell(0).SetCellValue("Best");
            headerRow.CreateCell(1).SetCellValue("Mid");
            headerRow.CreateCell(2).SetCellValue("Bad");

            int row = 2;
            foreach (var item in high)
            {
                IRow currentRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
                currentRow.CreateCell(0).SetCellValue(item.ShortName);
                row++;
            }

            row = 2;
            foreach (var item in mid)
            {
                IRow currentRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
                currentRow.CreateCell(1).SetCellValue(item.ShortName);
                row++;
            }

            row = 2; // Сброс номера строки
            foreach (var item in bad)
            {
                IRow currentRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
                currentRow.CreateCell(2).SetCellValue(item.ShortName);
                row++;
            }

            string filePath = Path.Combine(savePath);
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fileStream);
            }
        }
    }
}
