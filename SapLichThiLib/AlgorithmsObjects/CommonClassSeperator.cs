using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class CommonClassSeparator
    {
        // Input
        public List<StudyClass> AllStudyClasses { get; set; }
        public List<ExamGroup> HardRails { get; set; }
        // Output
        //public List<StudyClass> AllCommonClasses { get; set; }
        public List<StudyClass> O_allNonCommonClasses { get; set; }
        public HashSet<StudyClass> O_allCommonClasses { get; set; }
        public void SeperateClasses()
        {
            ConcatCourses(HardRails, out var coursesHashSet);
            O_allNonCommonClasses = new();
            foreach (var thisStudyClass in AllStudyClasses)
            {
                if (!coursesHashSet.Contains( thisStudyClass.Course))
                    O_allNonCommonClasses.Add(thisStudyClass);
            }
        }

        public void ConcatCourses(List<ExamGroup> hardRails, out HashSet<Course> courses)
        {
            courses = new HashSet<Course>();
            foreach (var hardRail in hardRails)
            {
                foreach (var course in hardRail.Courses)
                {
                    courses.Add(course);
                }
            }
        }
    }
}
