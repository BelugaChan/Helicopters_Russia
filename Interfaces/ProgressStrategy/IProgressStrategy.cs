using Algo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.ProgressStrategy
{
    public interface IProgressStrategy
    {
        void UpdateProgress(Progress progress);

        void LogProgress();

        Progress GetCurrentProgress();
    }
}
