using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class MainStudentYearGenerator : IAlgorithmObject
    {
        public List<Course> I_courses { get; set; } 
        public List<StudentYear> I_studentYear { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_studyClasses { get; set; }
        public Dictionary<StudyClass, List <ExamClass>> I_studyClass_examClasses { get; set; }
        public Dictionary<Course, StudentYear> O_course_mainStudentYear { get; set; }

        public void CheckAllInput()
        {
            if (I_course_studyClasses == null || I_course_studyClasses.Count == 0)
                throw new Exception("I_course_studyClasses is empty");
        }

        public void InitializeAllOutput()
        {
            O_course_mainStudentYear = new();
        }

        public void ProcedureRun()
        {
            foreach (var (course, studyClasses) in I_course_studyClasses)
            {
                Dictionary<StudentYear, int> studentYear_count = new();
                foreach (var studentYear in I_studentYear)
                {
                    studentYear_count.Add(studentYear, 0);
                }
                foreach (var studyClass in studyClasses)
                {
                    studentYear_count[studyClass.StudentYear] += I_studyClass_examClasses[studyClass].Count;
                }
                O_course_mainStudentYear.Add(course, studentYear_count.MaxBy(x => x.Value).Key);
            }
        }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }
    }
}
