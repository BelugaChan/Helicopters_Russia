using Abstractions.Interfaces;
using Algo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.Garbage
{
    public class GarbageHandler : IGarbageHandle
    {
        private List<string> resGosts = new List<string>();
        private List<string> patterns = new List<string>() 
        {
            @"\b(ГОСТ|ОСТ)\s*\d{3,5}-\d{2,4}\b"
            //@"ГОСТ \d{3,4,5}-\d{2,4}",
            //@"ОСТ \d{3,4,5}-\d{2,4}"
            //@"Г\d{3,4,5}-\d{2,4}"
        };
        public List<string> GetGOSTFromGarbageName(string name)
        {
            resGosts.Clear();
            var fixedName = name.ToUpper();
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(fixedName, pattern);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        resGosts.Add(match.Value);
                    }
                }
            }
            return resGosts;
        }
    }
}
