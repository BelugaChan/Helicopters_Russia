using MinHash.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.ExcelMergers
{
    internal class NPOIMerger : IExcelMerger
    {
        public void MergeExcelFiles(List<string> pathsToSourceFiles, string pathToResultFile, string fileName)
        {

            IWorkbook resultWorkbook = new XSSFWorkbook();
            ISheet resultSheet = resultWorkbook.CreateSheet("Merged Data");

            int currentRow = 0;

            foreach (string filePath in pathsToSourceFiles)
            {

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook sourceWorkbook = new XSSFWorkbook(fileStream);
                    ISheet sourceSheet = sourceWorkbook.GetSheetAt(0);


                    for (int rowIndex = 0; rowIndex <= sourceSheet.LastRowNum; rowIndex++)
                    {
                        IRow sourceRow = sourceSheet.GetRow(rowIndex);
                        if (sourceRow == null) continue;


                        IRow resultRow = resultSheet.CreateRow(currentRow);

                        for (int colIndex = 0; colIndex < sourceRow.LastCellNum; colIndex++)
                        {
                            ICell sourceCell = sourceRow.GetCell(colIndex);
                            if (sourceCell == null) continue;

                            // Создаем ячейку в результирующем файле и копируем значение
                            ICell resultCell = resultRow.CreateCell(colIndex);
                            CopyCell(sourceCell, resultCell);
                        }

                        currentRow++;
                    }
                }
            }

            string resultFilePath = Path.Combine(pathToResultFile, fileName);
            using (var fileStream = new FileStream(resultFilePath, FileMode.Create, FileAccess.Write))
            {
                resultWorkbook.Write(fileStream);
            }
        }

        private void CopyCell(ICell sourceCell, ICell resultCell)
        {
            switch (sourceCell.CellType)
            {
                case CellType.String:
                    resultCell.SetCellValue(sourceCell.StringCellValue);
                    break;
                case CellType.Numeric:
                    resultCell.SetCellValue(sourceCell.NumericCellValue);
                    break;
                case CellType.Boolean:
                    resultCell.SetCellValue(sourceCell.BooleanCellValue);
                    break;
                case CellType.Formula:
                    resultCell.SetCellFormula(sourceCell.CellFormula);
                    break;
                case CellType.Blank:
                    resultCell.SetBlank();
                    break;
                default:
                    resultCell.SetCellValue(sourceCell.ToString());
                    break;
            }
        }
    }
}
