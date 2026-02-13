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
    
    public class StudentYearCountTest : ITest<StudentYearCountTestObject>
    {
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<Course, StudentYear> I_course_mainStudentYear { get; set; }
        private List<StudentYearCountTestObject> results;

        public IEnumerable<StudentYearCountTestObject> GiveTestResult()
        {
            Test();
            return results;
        }

        public void Test()
        {
            results = new List<StudentYearCountTestObject>();
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            var allStudentYears = I_schedule.Where(x => !x.IsEmpty()).Select(x => x.ExamClasses.First().StudyClass.StudentYear).ToHashSet();
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    Dictionary<StudentYear, int> studentYear_studentYearCount = new();
                    foreach (var studentYear in allStudentYears)
                    {
                        studentYear_studentYearCount.Add(studentYear, 0);
                    }
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            studentYear_studentYearCount[I_course_mainStudentYear[examClass.StudyClass.Course]] += 1;
                        }
                    }
                    results.Add(new StudentYearCountTestObject() { Date = dates[date], Shift = shifts[shift], StudentYears_Count = studentYear_studentYearCount.OrderBy(x => x.Key.Name).ToList() });
                }
            }
        }
    }
}
