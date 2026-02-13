using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataObjects;

namespace SapLichThiLib.AlgorithmsObjects.SpecialCourseInserter
{
    // Used to check and install Inserter.
    internal class SpecialCourseInserter : IAlgorithmObject
    {
        public List<SpecialCourse> I_specialCourses { get; set; }
        Dictionary<Course, List<StudyClass>> I_course_studyClasses {get;set; }
        Dictionary<StudyClass, List<ExamClass>> I_studyClasses_examClasses { get;set; }

        
        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            throw new NotImplementedException();
        }

        public void InitializeAllOutput()
        {
            throw new NotImplementedException();
        }

        public void ProcedureRun()
        {
            
        }

        public void CombineAllExamClassOfCourse(SpecialCourse specialCourse)
        {
            List<Course> courses = specialCourse.Courses;
            List<ExamClass> examClasses = courses.Select(course => I_course_studyClasses[course].Select(studyClass => I_studyClasses_examClasses[studyClass]).Aggregate((x, y) =>
            {
                x.ToList().AddRange(y);
                return x;
            })).Aggregate((x, y) =>
            {
                x.ToList().AddRange(y);
                return x;
            });
        }

        public void CombineShift(SpecialCourse specialCourse)
        {
            var shiftCount = specialCourse.ShiftCount;
            
        }


    }
}
