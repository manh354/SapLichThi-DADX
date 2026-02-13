using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiCore.DataObjects
{
    public class Period
    {
        public Period(int id, DateOnly date, int shift, int duration)
        {
            Index = id;
            Date = date;
            Shift = shift;
            Duration = duration;
        }
        public int Index { get; set; }
        public DateOnly Date { get; set; }
        public int Shift { get; set; }
        public int Duration { get; set; }

        public override string ToString()
        {
            return $"Id= {Index},Date={Date}, Shift={Shift}, Duration={Duration})";
        }
    }
}
