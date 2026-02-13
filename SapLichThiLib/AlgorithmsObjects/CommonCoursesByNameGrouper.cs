using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class CommonCoursesByNameGrouper
    {
        //Input 
        List<Course> courses;
        // Output
        Dictionary<string, HashSet<Course>> Courses_groupby_Name = new();
        public void Make()
        {
            foreach (var item in courses)
            {
                if (Courses_groupby_Name.TryGetValue(item.Name, out HashSet<Course> courseWithThisName))
                {
                    courseWithThisName.Add(item);
                    continue;
                }
                Courses_groupby_Name.Add(item.Name, new HashSet<Course>());
            }
        }
    }
}
