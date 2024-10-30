using MinHash.Interfaces;
using OfficeOpenXml;

namespace MinHash.ExcelWriters
{
    public class EPPlusWriter : IExcelWriter
    {
        public void WriteCollectionsToExcel<TGarbageData>(HashSet<TGarbageData> bad, HashSet<TGarbageData> mid, HashSet<TGarbageData> high, string savePath)
            where TGarbageData : IGarbageData
        {
            var package = new ExcelPackage();

            var sheet = package.Workbook.Worksheets.Add("Data Report");

            sheet.Cells[2, 1, 2, 3].LoadFromArrays(new object[][] { new[] { "Best", "Mid", "Bad" } });
            var column = 1;
            var row = 3;
            foreach (var item in high)
            {
                sheet.Cells[row, column].Value = item.ShortName;
                row++;
            }
            row = 3;
            foreach (var item in mid)
            {
                sheet.Cells[row, column + 1].Value = item.ShortName;
                row++;
            }
            row = 3;
            foreach (var item in bad)
            {
                sheet.Cells[row, column + 2].Value = item.ShortName;
                row++;
            }

            File.WriteAllBytes(savePath, package.GetAsByteArray());
            package.Dispose();
        }
    }
}
