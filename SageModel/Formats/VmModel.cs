
using System;

namespace SageModel.Formats
{
    /// <summary>
    /// Valeur de base
    /// </summary>
    public class VmModel : IRubricValue
    {
        private readonly string _line;
        public string Rubric { get; set; }

        public VmModel(string line)
        {
            _line = line;
            Rubric = line.Substring(15, 10).Trim();
        }

        public string GetValue(string fieldName)
        {
            Fields field;
            if (!Enum.TryParse(fieldName, out field))
                throw new FormatException("Invalid field, must be a SageModel.Formats.BsModel.Fields value");

            switch (field)
            {
                case Fields.Valeur:
                    if (_line.Length < 25 + 12) return null;
                    return _line.Substring(25, 12).Trim();

            }


            throw new NotImplementedException("The field " + fieldName + " is not implemented");
        }

        public enum Fields
        {
            Valeur
        }
    }
}
