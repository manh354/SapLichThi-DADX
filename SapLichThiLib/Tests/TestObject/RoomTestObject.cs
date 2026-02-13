using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests.TestObject
{
    public class RoomTestObject
    {
        public ArrayCell ArrayCell { get; set; }
        public DateOnly Date { get; set; }
        public int Shift { get; set; }
        public Room Room { get; set; }
        public bool Condition { get; set; }
        public float Ratio { get; set; }
    }
}
