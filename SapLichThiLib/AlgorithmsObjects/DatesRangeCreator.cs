using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class DatesRangeCreator
    {
        public DateOnly Start { get; set; }
        public DateOnly End { get; set; }
        public List<DateOnly> Dates;
        public DatesRangeCreator(DateOnly Start, DateOnly End)
        {
            this.Start = Start;
            this.End = End;
        }
        public void MakeDatesRange()
        {
            Dates = new List<DateOnly>();
            for(DateOnly i = Start; i!= End.AddDays(1) ; i = i.AddDays(1))
            {
                Dates.Add(i);
            }    
        }

    }
}
