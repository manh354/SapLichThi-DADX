using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class Curriculum
    {
        public string curriculumName;
        public string CurriculumName => curriculumName;
        private HashSet<Course> courses = new HashSet<Course>();
        public HashSet<Course> Courses => courses;
        public Curriculum(string curriculumName, IEnumerable<Course> courses)
        {
            this.curriculumName = curriculumName;
            this.courses = courses.ToHashSet();
        }
    }
}
