using MinHash.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Models
{
    public class GarbageData : IGarbageData
    {
        public string ShortName { get; set; }
    }
}
