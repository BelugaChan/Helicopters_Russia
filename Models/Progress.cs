using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Models
{
    public class Progress
    {
        public string Step { get; set; } = "Алгоритм не начал работу или идёт обработка Excel файлов";

        public double CurrentProgress { get; set; } = 0;
    }
}
