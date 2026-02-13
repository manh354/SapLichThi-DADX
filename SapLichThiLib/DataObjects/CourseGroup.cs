using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class CourseGroup
    {
        public string Name { get; set; }
        public List<Course> Courses { get; set; }
    }
}
