using CsvHelper;
using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests
{
    public class CurriculumLinkage
    {
        public Curriculum Curriculum { get; set; }
        public Dictionary<Course, double> Course_Dates { get; set; }
    }
    public class ScheduleSeparationTest : ITest<CurriculumLinkage>
    {
        public ExamSchedule I_schedule { get; set; }
        public List<Curriculum> I_curricula { get; set; }
        public List<CurriculumLinkage> O_allLinkages { get; set; }
        public Dictionary<Course, int> O_course_dates { get; set; }

        public IEnumerable<CurriculumLinkage> GiveTestResult()
        {
            Test();
            return O_allLinkages;
        }

        public void Test()
        {
            O_allLinkages = new();
            int dateLength = I_schedule.dates.Length;
            int shiftLength = I_schedule.shifts.Length;
            int roomLength = I_schedule.rooms.Length;
            Dictionary<Course, List<int>> courses_dates = new();
            for (int date = 0; date < dateLength; date++)
            {
                for (var shift = 0; shift < shiftLength; shift++)
                {
                    for (int room = 0; room < roomLength; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            var thisCourse = examClass.StudyClass.Course;
                            courses_dates.TryAdd(thisCourse, new List<int>());
                            courses_dates[thisCourse].Add(date);
                        }
                    }
                }
            }

            Dictionary<Course, double> courses_date_nomalized = new();
            var preprocessed_course_date = courses_dates.Select(x => (x.Key, x.Value.Average(y => y)));
            foreach (var course_date in preprocessed_course_date)
            {
                courses_date_nomalized.Add(course_date.Key, course_date.Item2);
            }

            foreach (var curriculum in I_curricula)
            {
                Dictionary<Course, double> curriculum_courses_date = new(); 
                foreach (var course in curriculum.Courses)
                {
                    if(courses_date_nomalized.ContainsKey(course))
                        curriculum_courses_date.Add(course, courses_date_nomalized[course]);
                }
                O_allLinkages.Add(new CurriculumLinkage()
                {
                    Curriculum = curriculum,
                    Course_Dates = curriculum_courses_date,
                });
            }
        }
    }
}
