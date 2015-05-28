using System;

namespace SageModel.Formats
{
    /// <summary>
    /// Bulletins salariés 
    /// </summary>
    public class BsModel : IRubricValue
    {
        public const string Number = "Number";

        private readonly string _line;
        public string Rubric { get; set; }

        public BsModel(string line)
        {
            _line = line;
            Rubric = line.Substring(62, 4).Trim();
        }

        public string GetValue(string fieldName)
        {
            Fields field;
            if (!Enum.TryParse(fieldName, out field))
                throw new FormatException("Invalid field, must be a SageModel.Formats.BsModel.Fields value");
            switch (field)
            {
                case Fields.MontantSalarial:
                    if (_line.Length < 134 + 12) return null;
                    return _line.Substring(134, 12).Trim();

                case Fields.MontantPatronal:
                    if (_line.Length < 158 + 12) return null;
                    return _line.Substring(158, 12).Trim();

                case Fields.Nombre:
                    if (_line.Length < 98 + 12) return null;
                    return _line.Substring(98, 12).Trim();


            }

            throw new NotImplementedException("The field " + fieldName + " is not implemented");
        }

        public enum Fields
        {
            Nombre,
            MontantSalarial,
            MontantPatronal
        }
    }
}
