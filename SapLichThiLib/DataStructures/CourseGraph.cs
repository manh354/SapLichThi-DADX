using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataStructures
{
    public class CourseGraph : Graph<Course>
    {
        public CourseGraph(Dictionary<Course, HashSet<Course>> adjacencyList)
        {
            AdjacencyList = adjacencyList;
        }
        public CourseGraph(List<Course> allClasses)
        {
            foreach (var item in allClasses)
            {
                AddVertex(item);
            }
        }
        public int NumberOfClasses => AdjacencyList.Count;
        public HashSet<Course> this[Course index]
        {
            get
            {
                return AdjacencyList[index];
            }
            set
            {
                AdjacencyList[index] = value;
            }
        }

        public override string ToString()
        {
            string result = string.Empty;
            string[] results = new string[AdjacencyList.Count];
            int ind = 0;
            foreach (var item in AdjacencyList)
            {
                result += "Course id:" + item.Key.ID;
                foreach (var item2 in item.Value)
                {
                    result += " -> " + item2.ID;
                }
                result += "\n";
            }
            return result;
        }
    }
}
