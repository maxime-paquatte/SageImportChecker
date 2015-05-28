using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageModel
{
    public interface IRubricValue
    {
        string Rubric { get; }
        string GetValue(string fieldName);
    }
}
