﻿using MinHash.Interfaces;
using MinHash.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Factory
{
    public class StandartFactory : IEntityFactory<Standart>
    {
        public Standart CreateFromRow(ExcelWorksheet worksheet, int row)
        {
            return new Standart()
            {
                Name = worksheet.Cells[row, 2].Value?.ToString()
            };
        }
    }
}
