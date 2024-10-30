using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Interfaces
{
    public interface IEntityFactory<T> where T : class
    {
        T CreateFromRow(ExcelWorksheet workSheet, int row);
    }
}
