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
    public class StudentYearOkTest : ITest<StudentYearOkTestObject>
    {
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<Course, StudentYear> I_course_mainStudentYear { get; set; }
        public Dictionary<(int date, int shift), StudentYear> I_slot_largestYear { get; set; }
        public Dictionary<StudentYear, int> I_studentYear_prioritizedShift { get; set; }
        public List<StudentYearOkTestObject> results { get; set; }
        public IEnumerable<StudentYearOkTestObject> GiveTestResult()
        {
            Test();
            return results;
        }

        public void Test()
        {
            results = new List<StudentYearOkTestObject>();
            if(I_slot_largestYear == null)
                return;
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            var allStudentYears = I_schedule.Where(x => !x.IsEmpty()).Select(x => x.ExamClasses.First().StudyClass.StudentYear).ToHashSet();
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    var thisSlotStudentYear = I_slot_largestYear[(date, shift)];
                    bool isOk = false;
                    if (shifts[shift] == I_studentYear_prioritizedShift[thisSlotStudentYear])
                        isOk = true;
                    results.Add(new StudentYearOkTestObject() { 
                        Date = dates[date], 
                        Shift = shifts[shift], 
                        StudentYear = thisSlotStudentYear, 
                        Okness = isOk });
                }
            }
        }
    }
}
