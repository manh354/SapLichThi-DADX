using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiCore.ExportableObjects
{
    public interface IExportableObject
    {
        string[] GetHeaders();
        IEnumerable<string[]> GetValuesAsString();
    }
}
