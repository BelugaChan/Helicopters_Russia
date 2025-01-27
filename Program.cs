using OfficeOpenXml;
using System.Security.Cryptography.X509Certificates;

namespace NumberOfStandards
{
    public class Standarts
    {
        public string standartName { get; set; }
        public int referenceCode { get; set; }
        public double matchLevel { get; set; }

        public Standarts(string standartName, int referenceCode, double matchLevel)
        {
            this.standartName = standartName;
            this.referenceCode = referenceCode;
            this.matchLevel = matchLevel;
        }
    }

    public class Garbages
    {
        public string garbageName { get; set; }
        public List<Standarts> standarts { get; set; }

        public Garbages(string garbageName)
        {
            this.garbageName = garbageName;
            this.standarts = new List<Standarts>();
        }

        public void AddStandarts(Standarts standart)
        {
            this.standarts.Add(standart);
        }
    }

    public static class Processor
    {
        public static List<List<string>> ExcelReader(this string filePath, int sheet) //путь, лист
        {
            List<List<string>> result = new();
            List<string> temp = new();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл Excel не найден! ");
                return null;
            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                Console.WriteLine($"Чтение excel файла {filePath}");

                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet]; // Получаем первый лист
                int rowCount = worksheet.Dimension.Rows; // Получаем количество строк
                int colCount = worksheet.Dimension.Columns; // Получаем количество колонок

                Console.WriteLine($"Файл {filePath} считан, начинаю обработку..");

                for (int row = 2; row <= rowCount; row++)
                {
                    temp = new();
                    for (int col = 1; col <= colCount; col++)
                    {
                        // Читаем значения ячеек
                        string cellValue = worksheet.Cells[row, col].Text;

                        // Записываем значение во временный List<string>
                        temp.Add(cellValue);
                    }

                    result.Add(temp);
                }
            }

            Console.WriteLine($"Файл {filePath} обработан.");

            return result;
        }

        public static void WriteToExcel(string filePath, List<List<string>> data)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Results");

                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        worksheet.Cells[i + 1, j + 1].Value = data[i][j];
                    }
                }


                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Старый {filePath} был удален");
                }

                package.SaveAs(new FileInfo(filePath));
                Console.WriteLine($"Результаты сохранены в {filePath}");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            const string algoResultFilePath = "Algo.xlsx"; // Результаты алгоритма
            List<List<string>> garbages = algoResultFilePath.ExcelReader(1); //номер листа в подборке (0 - идеальное, 1 - требует уточнений, 2 - не сопоставленно)

            List<Garbages> garbagesList = new List<Garbages>();
            Garbages currentGarbage = null;

            foreach (var garbage in garbages)
            {
                // Проверяем, является ли первое значение пустым
                if (!string.IsNullOrEmpty(garbage[0]))
                {
                    // Если это не пустое значение, создаем новый объект Garbages
                    currentGarbage = new Garbages(garbage[0]);
                    garbagesList.Add(currentGarbage);
                }

                // Теперь добавляем стандарты, начиная со второго элемента
                // Предполагается, что значения для Garbage идут с индекса 1
                if (currentGarbage != null)
                {
                    // Проверяем на тип чисел для referenceCode и matchLevel
                    if (int.TryParse(garbage[2], out int referenceCode) &&
                        double.TryParse(garbage[3], out double matchLevel))
                    {
                        // Создаем новый объект Standarts и добавляем его в текущий объект Garbages
                        Standarts newStandart = new Standarts(garbage[1], referenceCode, matchLevel);
                        currentGarbage.AddStandarts(newStandart);
                    }
                    else
                    {
                        Console.WriteLine("Pizdec");
                    }
                }
            }
            Console.WriteLine("Считали, все норм");

            const string answersFilePath = "Answers 2.xlsx"; // Правильные ответы
            List<List<string>> answers = answersFilePath.ExcelReader(0); 

            const string incorrectlyMatchedFilePath = "Неверное сопоставление.xlsx"; //Неверно сопоставленно
            List<List<string>> incorrectlyMatched = new();

            for (int i = garbagesList[0].standarts.Count; i > 0; i--)
            //for (int i = garbagesList[0].standarts.Count; i > 0; i--)
                {
                bool found = false;
                double check = 0;
                foreach (var garbage in garbagesList)
                {
                    foreach (var answer in answers)
                    {
                        if (garbage.garbageName == answer[1])
                        {
                            for (int j = 0; j < i; j++)
                            {
                                var standart = garbage.standarts[j];
                                if (standart.standartName == answer[3])
                                {
                                    found = true;
                                    check++;
                                    break;
                                }
                            }
                            if (!found && i == garbagesList[0].standarts.Count)
                            {
                                var combinedRow = new List<string> { garbage.garbageName };

                                foreach (var standart in garbage.standarts)
                                {
                                    combinedRow.Add(standart.standartName);
                                    combinedRow.Add(standart.matchLevel.ToString());
                                }

                                // Добавляем пустую строку и элементы из массива answer
                                combinedRow.Add("");  // Пустая строка
                                combinedRow.Add(answer[2]);
                                combinedRow.Add(answer[3]);

                                // Добавляем в список incorrectlyMatched
                                incorrectlyMatched.Add(combinedRow);
                            }
                            found = false;
                            break;
                        }
                    }
                }
                double percentage = (check / (double)garbagesList.Count) * 100; // Приведение к double для точности
                Console.WriteLine($"Результат для {i} - {percentage:F2}%");
                //incorrectlyMatched.Add(new List<string>());
            }
            Processor.WriteToExcel(incorrectlyMatchedFilePath, incorrectlyMatched);
        }
    }
}
