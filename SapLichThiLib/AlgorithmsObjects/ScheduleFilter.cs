using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class ScheduleFilter
    {
        public ExamSchedule I_schedule { get; set; }
        public float FilterThreshold { get; set; }
        public void FilterSchedule()
        {
            foreach (var item in I_schedule)
            {
                item.ExamClasses = item.ExamClasses.Where(x => x.Count > 0).ToList();
            }
        }
    }
}
