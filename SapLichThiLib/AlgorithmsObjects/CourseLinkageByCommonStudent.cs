using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class CourseLinkageByCommonStudent
    {
        public List<Course> courses;
        public Dictionary<Course, HashSet<StudyClass>> I_courses_studyClasses { get; set; }
        public List<StudyClass> I_studyClasses { get; set; }
        public Dictionary<Course, HashSet<Course>> O_courseLinkage {get;set;}
        public void LinkCourses()
        {
            O_courseLinkage = new();
            Dictionary<Course, HashSet<Student>> allStudentsOfCourse = new();
            foreach (var (course, studyClasses) in I_courses_studyClasses)
            {
                List<Student> students = new();
                foreach (var studyClass in studyClasses)
                {
                    if (studyClass.Students == null)
                        continue;
                    students.AddRange(studyClass.Students);
                }
                allStudentsOfCourse.Add(course, students.ToHashSet());
            }
            foreach (var (course1,students1) in allStudentsOfCourse)
            {
                foreach (var (course2, students2) in allStudentsOfCourse)
                {
                    bool linkage = false;
                    if (course1 == course2)
                        continue;
                    foreach (var student in students1)
                    {
                        if(students2.Contains(student))
                        {
                            linkage = true;
                            break;
                        }
                    }
                    if (linkage)
                    {
                        if (!O_courseLinkage.ContainsKey(course1))
                        {
                            O_courseLinkage.Add(course1, new());
                        }
                        if (!O_courseLinkage.ContainsKey(course2))
                        {
                            O_courseLinkage.Add(course2, new());
                        }
                        O_courseLinkage[course1].Add(course2);
                        O_courseLinkage[course2].Add(course1);
                    }
                }
            }
        }
    }
}
