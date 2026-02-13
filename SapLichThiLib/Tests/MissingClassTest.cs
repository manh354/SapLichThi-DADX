using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.Tests.TestObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests
{
    internal class MissingClassTest : ITest<ExamClass>
    {
        public IEnumerable<ExamClass> I_allExamClasses { get; set; }
        public ExamSchedule I_schedule { get; set; }
        private List<ExamClass> result;

        public IEnumerable<ExamClass> GiveTestResult()
        {

            Test();
            return result;
        }

        public void Test()
        {
            var examClassHashSet = I_allExamClasses.ToHashSet();
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;

            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            examClassHashSet.Remove(examClass);
                        }
                    }
                }
            }
            result = examClassHashSet.ToList();
        }

    }
}
