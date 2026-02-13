using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataObjects;

namespace SapLichThiLib.DataStructures
{
    public class ClassGraph : Graph<StudyClass>
    {
        public ClassGraph(Dictionary<StudyClass, HashSet<StudyClass>> adjacencyList)
        {
            AdjacencyList = adjacencyList;
        }
        public ClassGraph(List<StudyClass> allClasses)
        {
            foreach (var item in allClasses)
            {
                AddVertex(item);
            }
        }
        public int NumberOfClasses => AdjacencyList.Count;
        public HashSet<StudyClass> this[StudyClass index]
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
            string[] results =new string[ AdjacencyList.Count];
            int     ind = 0;
            foreach (var item in AdjacencyList)
            {
                result += "Class id:" + item.Key.ID;
                foreach (var item2 in item.Value)
                {
                    result += " -> " + item2.ID;
                }
                result += "\n";
            }
            return result;
        }
    }
    public class Node
    {
        public StudyClass _Class;
    }
    public class Edge
    {
        public StudyClass _ToEdge;
    }
}
