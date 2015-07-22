using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageModel.Formats;

namespace SageModel
{
    public class Employee
    {
         public string Matricule { get; set; }

        public Dictionary<string, IRubricValue> Values = new Dictionary<string, IRubricValue>();

        public Employee(string matricule)
        {
            Matricule = matricule;
        }

        public bool AddValue(IRubricValue m)
        {
            if (Values.ContainsKey(m.Rubric))
                return false;
            Values.Add(m.Rubric, m);
            return true;
        }

        public static IRubricValue ParseValue(string line)
        {
            var kind = line.Substring(0, 2);
            switch (kind)
            {
                case "BS": return new BsModel(line);
                case "VM": return new VmModel(line);
            }
            return null;
        }
    }
}
