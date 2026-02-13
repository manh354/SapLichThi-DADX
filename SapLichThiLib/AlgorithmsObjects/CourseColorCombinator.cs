using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class CourseColorCombinator : IAlgorithmObject
    {
        public List<ExamGroup> I_examGroups { get;set; }
        public Dictionary<int, HashSet<Course>>? I_color_courses { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_studyClasses { get; set; }
        public Dictionary<int, HashSet<StudyClass>> O_color_studyClasses { get; set; }
        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            if(I_color_courses == null || I_color_courses.Count == 0 || I_examGroups == null || I_examGroups.Count == 0)
            {
                throw new Exception();
            }
        }

        public void InitializeAllOutput()
        {
            O_color_studyClasses = new();
        }

        public void ProcedureRun()
        {
            var allCommonCourse = I_examGroups.Select(x => x.Courses).Aggregate((x,y)=> x.Concat(y).ToList()).ToHashSet();
            foreach (var (color, courses) in I_color_courses)
            {
                HashSet<StudyClass> studyClasses = new();
                foreach (var course in courses)
                {
                    if (allCommonCourse.Contains(course))
                        continue;
                    foreach (var studyClass in I_course_studyClasses[course])
                    {
                        studyClasses.Add(studyClass);
                    }
                }
                if(studyClasses.Count>0)
                    O_color_studyClasses.Add(color, studyClasses);
            }
        }
    }
}
