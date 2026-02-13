using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    // Đại diện cho một Group của course
    public class ExamGroup
    {
        public List<Course> Courses { get; set; }
        public int? Date { get; set; }
        public int NumShift { get; set; }
        public int? DefaultShift { get; set; }
        public List<RoomType> PrioritizedRooms { get; set; }
        public string Mode { get; set; }
        public ExamGroup(List<Course> course, int? date, int numshift,int? defaultShift,List<RoomType> prioritizedRooms, string mode) 
        {
            Courses = course;
            Date = date;
            NumShift = numshift;
            DefaultShift = defaultShift;
            PrioritizedRooms = prioritizedRooms;
            Mode = mode;
        }
    }
}
