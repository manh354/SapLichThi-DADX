using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class SpecialCourse
    {
        public List<Course> Courses { get; set; }
        public int ShiftCount { get; set; }
        public List<SpecialRoom> SpecialRooms { get; set; }
        public SpecialCourse(List<Course> courses, int shiftCount, List<SpecialRoom> specialRooms) 
        {
            Courses = courses;
            ShiftCount = shiftCount;
            SpecialRooms = specialRooms;
        }
    }
}
