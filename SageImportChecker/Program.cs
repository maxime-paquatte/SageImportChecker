using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using SageImportChecker.Model;
using SageModel;

namespace SageImportChecker
{
    class Program
    {

        private static string _logPath;


        static void Main(string[] args)
        {

            var warningBuilder = new StringBuilder();
            _logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Erreurs.txt");
            if (File.Exists(_logPath)) File.Delete(_logPath);

            var config = LoadConfig();
            if (config != null)
            {
                var models = LoadSageImportFile();

                if (!_hasError)
                {

                    Regex matriculeRegex = null;
                    if (!string.IsNullOrEmpty(config.MatriculeRegex))
                        matriculeRegex = new Regex(config.MatriculeRegex);

                    string csvFile = PromptVerifCsvFile();
                    using (TextReader textReader = File.OpenText(csvFile))
                    {
                        textReader.ReadLine();//Read periode line
                        var csv = new CsvReader(textReader, new CsvConfiguration { Delimiter = config.CsvDelimiter ?? ";" });
                        while (csv.Read())
                        {
                            var mField = csv.GetField<String>(0);
                            var matricule = matriculeRegex != null
                                ? matriculeRegex.Match(mField).Groups[1].Value
                                : mField;

                            if (models.ContainsKey(matricule))
                            {

                                var model = models[matricule];

                                foreach (var c in config.Values)
                                {
                                    var record = csv.GetField<String>(c.ExcelColumnIdx);
                                    string val = string.Empty;
                                    if (model.Values.ContainsKey(c.Rubrique))
                                        val = model.Values[c.Rubrique].GetValue(c.FieldName) ?? string.Empty;
                                    else warningBuilder.AppendFormat(Resources.RubricNotFound, c.Rubrique, matricule).AppendLine();

                                    float a, b;
                                    if (!c.IsNumeric && val != record ||
                                        (float.TryParse(val.Replace(',', '.'), NumberStyles.Number, CultureInfo.GetCultureInfo(9), out a) ? a : 0) !=
                                        (float.TryParse(record.Replace(',', '.'), NumberStyles.Number, CultureInfo.GetCultureInfo(9), out b) ? b : 0))
                                    {
                                        if (!_hasError)
                                        {
                                            WriteError(string.Join(" | ", "Matricule".PadRight(10), "Rubrique".PadRight(40), "Csv".PadRight(8), "Text".PadRight(8)));
                                            WriteError(new string('-', 4 + 10 + 40 + 8 + 8));
                                        }
                                        WriteError(string.Join(" | ", matricule.PadRight(10), (csv.FieldHeaders[c.ExcelColumnIdx] + " (" + c.Rubrique + ")").PadRight(40), record.PadRight(8), val.PadRight(8)));
                                    }

                                }
                                model.Checked = true;
                            }
                            else WriteError(Resources.MatriculeNotFoundInTxt + matricule);
                        }


                        foreach (var model in models.Values.Where(m => !m.Checked))
                        {
                            WriteError(Resources.MatriculeNotFoundInCsv + model.Matricule);
                        }
                    }
                }

               

                if (!_hasError && warningBuilder.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Resources.SuccessMessage);
                }
                WriteWarning(warningBuilder.ToString());

            }
            Console.WriteLine(Resources.PressKeyToQuit);
            Console.ReadLine();
        }

        static Config LoadConfig()
        {

            var cPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "config.xml");
            Console.WriteLine(Resources.LoadConfig, cPath);
            var configs = Config.LoadConfigs(cPath);
            if (configs == null)
            {
                WriteError(Resources.ConfigFileNotFound + ": " + cPath);
                Console.Write(Resources.ConfigAskToWriteSample);
                if (Console.ReadLine() == "o")
                {
                    Config.WriteSampleConfig(cPath);
                    Console.WriteLine(Resources.ConfigWriteSampleSuccess + cPath);
                }
            }
            else
            {
                for (int i = 0; i < configs.Length; i++)
                {
                    Console.WriteLine("\t[" + i + "] : " + configs[i].Name);
                }


                int idx = -1;
                while (idx == -1)
                {
                    Console.Write(Resources.PromptConfigIdx);
                    idx = int.TryParse(Console.ReadLine(), out  idx) ? idx : 0;
                }

                var config = configs[idx];

                Console.WriteLine(Resources.NbValuesToCheck, config.Values.Length);
                return config;
            }
            return null;
        }

        private static Dictionary<string, EmployeeToCheck> LoadSageImportFile()
        {
            string file = PromptSageImportFile();
            Console.WriteLine(Resources.LoadTextFile);
            var employees = new Dictionary<string, EmployeeToCheck>();
            foreach (var line in File.ReadAllLines(file))
            {
                var matricule = line.Substring(2, 10).Trim();

                EmployeeToCheck employee;
                if (!employees.TryGetValue(matricule, out employee))
                    employees.Add(matricule, employee = new EmployeeToCheck(matricule));

                var m = Employee.ParseValue(line);
                if (m == null) continue;
                if (!employee.AddValue(m))
                    WriteError(string.Format(Resources.UnableToAddRubric, employee.Matricule, m.Rubric));
            }

            Console.WriteLine(Resources.NbMatriculeLoaded, employees.Count);

            return employees;
        }

        static string PromptSageImportFile()
        {
            string file = null;
            while (string.IsNullOrEmpty(file))
            {
                Console.Write(Resources.PromptTextFile);
                var a = (Console.ReadLine() ?? "").Trim('"');
                if (!string.IsNullOrEmpty(a) && File.Exists(a))
                    file = a;
                else WriteError(Resources.FileDoesNotExists);
            }
            return file;
        }

        static string PromptVerifCsvFile()
        {
            string file = null;
            while (string.IsNullOrEmpty(file))
            {
                Console.Write(Resources.PromptCsvFile);
                var a = (Console.ReadLine() ?? "").Trim('"');
                if (!string.IsNullOrEmpty(a) && File.Exists(a))
                    file = a;
                else WriteError(Resources.FileDoesNotExists);
            }
            return file;
        }

        private static bool _hasError = false;
        static void WriteError(string message)
        {
            _hasError = true;

            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);
            Console.ForegroundColor = c;

            File.AppendAllText(_logPath, message + Environment.NewLine);
        }

        static void WriteWarning(string message)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine(message);
            Console.ForegroundColor = c;
        }
    }
}
