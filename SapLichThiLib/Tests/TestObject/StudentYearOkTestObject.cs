using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests.TestObject
{
    public class StudentYearOkTestObject
    {
        public DateOnly Date { get; set; }
        public int Shift { get; set; }
        public StudentYear StudentYear { get; set; }
        public bool Okness { get; set; }
    }
}
