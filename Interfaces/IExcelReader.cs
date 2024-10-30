using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Interfaces
{
    public interface IExcelReader
    {
        List<T> CreateCollectionFromExcel<T>(string filePath, IEntityFactory<T> factory)
            where T : class;
    }
}
