using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Interfaces
{
    public interface IExcelMerger
    {
        void MergeExcelFiles(List<string> pathsToSourceFiles, string pathToResultFile, string fileName);
    }
}
