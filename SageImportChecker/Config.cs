using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SageModel.Formats;

namespace SageImportChecker
{
    [Serializable]
    public class Config
    {
        [XmlAttribute]
        public string Name { get; set; }


        [XmlAttribute]
        public string CsvDelimiter { get; set; }

        [XmlAttribute]
        public string MatriculeRegex { get; set; }

        public Value[] Values { get; set; }


        public static Config[] LoadConfigs(string path)
        {
            var xs = new XmlSerializer(typeof(Config[]));
            if (!File.Exists(path)) return null;
            using (var s = File.OpenRead(path))
            {
               return (Config[])xs.Deserialize(s);
            }
        }

        public static void WriteSampleConfig(string path)
        {
            var config1 = new Config
            {
                Name = "Company1",
                Values = new[]
                {
                    new Value{ Kind = "VM", Rubrique = "CL05", FieldName = VmModel.Fields.Valeur.ToString(), ExcelColumnIdx = 2, IsNumeric = true},
                    new Value{ Kind = "BS", Rubrique = "8770", FieldName = BsModel.Fields.Nombre.ToString(), ExcelColumnIdx = 3, IsNumeric = true},
                    new Value{ Kind = "BS", Rubrique = "8770", FieldName = BsModel.Fields.MontantSalarial.ToString(), ExcelColumnIdx = 4, IsNumeric = true},
                    new Value{ Kind = "BS", Rubrique = "8770", FieldName = BsModel.Fields.MontantPatronal.ToString(), ExcelColumnIdx = 5, IsNumeric = true},
                }
            };

            var config2 = new Config
            {
                Name = "Company2",
                Values = new[]
                {
                    new Value{ Kind = "VM", Rubrique = "CL05", FieldName = VmModel.Fields.Valeur.ToString(), ExcelColumnIdx = 3, IsNumeric = true},
                    new Value{ Kind = "BS", Rubrique = "8770", FieldName = BsModel.Fields.MontantSalarial.ToString(), ExcelColumnIdx = 5, IsNumeric = true},
                }
            };

            var xs = new XmlSerializer(typeof(Config[]));
            using (var s = File.OpenWrite(path))
                xs.Serialize(s, new [] { config1, config2 });
        }
    }

    [Serializable]
    public class Value
    {
        [XmlAttribute]
        public string Kind { get; set; }

        [XmlAttribute]
        public string Rubrique { get; set; }

        [XmlAttribute]
        public string FieldName { get; set; }

        [XmlAttribute]
        public int ExcelColumnIdx { get; set; }

        [XmlAttribute]
        public bool IsNumeric { get; set; }
    }

}
