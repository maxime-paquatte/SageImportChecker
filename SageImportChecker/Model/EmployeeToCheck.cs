using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageModel;

namespace SageImportChecker.Model
{
    class EmployeeToCheck : Employee
    {

        public bool Checked { get; set; }

        public EmployeeToCheck(string matricule) : base(matricule)
        {
        }
    }
}
