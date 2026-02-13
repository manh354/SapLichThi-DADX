using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests.TestObject
{
    public class StudentYearCountTestObject
    {
        public DateOnly Date { get; set; }
        public int Shift { get; set; }
        public List<KeyValuePair<StudentYear,int>> StudentYears_Count { get; set; }
    }
}
