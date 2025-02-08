using System.Text;
using System.Xml.Linq;


namespace InjectData
{
    internal class Program
    {
        private static readonly Dictionary<string, int> InstituteMapping = new()
        {
            { "ИТИ", 11 },
            { "ИНПО", 14 },
            { "ИИП", 15 },
            { "ИЕНИМ", 16 },
            { "МИ", 17 },
            { "ИФИ", 23 },
            { "ИМЭА", 24 },
            { "КПОИП", 27 },
            { "МедК", 26 },
            { "СХК", 28 },
            { "Аспирантура", 50 },
            { "Ординатура", 51 }
        };

        private static readonly Dictionary<int, int> EducationFormMapping = new()
        {
            { 1, 1 },
            { 3, 2 },
            { 2, 3 }
        };

        private static List<string> GetXmlFiles(string folderPath)
        {
            List<string> xmlFiles = new List<string>();

            foreach (string file in Directory.GetFiles(folderPath, "*.plx"))
            {
                xmlFiles.Add(file);
            }

            foreach (string subfolder in Directory.GetDirectories(folderPath))
            {
                xmlFiles.AddRange(GetXmlFiles(subfolder));
            }

            return xmlFiles;
        }

        private static void ProcessXmlFile(string filePath, ApiToUpload api)
        {
            var pathData = ParseFilePath(filePath);
            var educationFormCode = GetEducationFormCodeFromXml(filePath);

            if (!EducationFormMapping.TryGetValue(educationFormCode, out int educationFormId))
            {
                throw new KeyNotFoundException($"Не найден ID для кода формы обучения '{educationFormCode}'.");
            }


            var xmlContent = File.ReadAllText(filePath);
            var check = api.DublicateCheck(pathData.InstituteId, educationFormId, pathData.FilePathFromPlans);
            Console.WriteLine(check);
            if (check.HasValue)
            {
                throw new Exception($"Запись уже существует в базе данных: id:'{check}', path:'{pathData.FilePathFromPlans}'.");
            }
            else
            {
                api.AddXml(pathData.InstituteId, educationFormId, xmlContent, pathData.FilePathFromPlans);
                Console.WriteLine($"Файл {filePath} успешно обработан.");
            }
        }

        private static int GetEducationFormCodeFromXml(string filePath)
        {
            XDocument xmlDocument = XDocument.Load(filePath);

            var codeAttribute = xmlDocument.Root?.Attribute("КодФормыОбучения")?.Value;

            if (string.IsNullOrEmpty(codeAttribute) || !int.TryParse(codeAttribute, out int educationFormCode))
            {
                throw new InvalidOperationException("Атрибут 'КодФормыОбучения' не найден или имеет некорректное значение.");
            }

            return educationFormCode;
        }

        private static (int InstituteId, string FilePathFromPlans) ParseFilePath(string filePath)
        {
            var parts = filePath.Split(Path.DirectorySeparatorChar);
            var plansIndex = Array.IndexOf(parts, "Планы");

            if (plansIndex == -1 || parts.Length < plansIndex + 2)
            {
                throw new ArgumentException("Неверный формат пути. Отсутствует 'Планы' в пути.");
            }

            var institute = parts[plansIndex + 1];
            var filePathFromPlans = string.Join(Path.DirectorySeparatorChar, parts[plansIndex..]);

            if (!InstituteMapping.TryGetValue(institute, out int instituteId))
            {
                throw new KeyNotFoundException($"Не найден ID для института '{institute}'.");
            }

            return (instituteId, filePathFromPlans);
        }

        static void MainLoop(ApiToUpload api)
        {
            while (true)
            {
                Console.WriteLine("Введите путь к папке 'Планы':");
                var input = Console.ReadLine();


                if (string.IsNullOrWhiteSpace(input) || !Directory.Exists(input))
                {
                    Console.WriteLine("Указанный файл не существует.");
                    return;
                }
                try
                {
                    foreach (var filePath in GetXmlFiles(input))
                    {
                        try
                        {
                            ProcessXmlFile(filePath, api);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обработке файла {filePath}: {ex.Message}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                }
            }
        }
                
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;

            using (var api = new ApiToUpload())
            {
                MainLoop(api);
            }
        }
    }
}

