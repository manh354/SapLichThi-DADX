using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;

namespace SapLichThiLib.Extensions
{
    public static class WriteLineExtension
    {
        public static void WriteLine<T>(this List<T> list)
        {
            Console.WriteLine("All elements in this list: ");
            int count = list.Count;
            foreach (T item in list)
            {
                Console.Write("{0}\n", item.ToString());
            }
        }
        public static void WriteTab<T>(this List<T> list)
        {
            Console.WriteLine("All elements in this list: ");
            int count = list.Count;
            foreach (T item in list)
            {
                Console.Write("{0}\t", item.ToString());
            }
        }
        public static void WriteGraph<T>(this LinkedList<T>[] adjacentList)
        {
            throw new NotImplementedException();
        }
        public static void WriteLineClassGraph(this ClassGraph classGraph)
        {
            foreach (var adjacentVertexes in classGraph.AdjacencyList)
            {
                Console.WriteLine(adjacentVertexes.Value.WriteAdjacentClassListToString());
            }
        }

        public static void WriteLineClassGraphComponent(this ClassGraph classGraph)
        {
            foreach (var adjacentVertexes in classGraph.AdjacencyList)
            {
                Console.WriteLine(adjacentVertexes.Value.WriteAdjacentClassListToString());
                break;
            }
        }

        public static string WriteAdjacentClassListToString(this HashSet<StudyClass> adjacentClassList)
        {
            string result = "";
            foreach (StudyClass item in adjacentClassList)
            {
                result += string.Format("Id: {0,-8} --> ", item.ID);
            }
            return result;
        }

        public static string ClassGraphColoringToString(this Dictionary<int, HashSet<StudyClass>> coloring)
        {
            string result = string.Empty;
            foreach (var colorGroup in coloring)
            {
                result += string.Format("Color: {0}\n", colorGroup.Key);
                foreach (var individualVertex in colorGroup.Value)
                {
                    result += string.Format("Class: {0,-8}\t", individualVertex.ID);
                }
            }
            return result;
        }

        public static string ListClassGraphColoringToString(this List<Dictionary<int, HashSet<StudyClass>>> listOfColoringSet)
        {
            string result = string.Empty;
            foreach (var item in listOfColoringSet)
            {
                result += item.ClassGraphColoringToString();
                result += "=================================================\n";
            }
            return result;
        }

        public static void WriteLineListClassGraphColoring(this List<Dictionary<int, HashSet<StudyClass>>> listOfColoringSet)
        {
            foreach (var item in listOfColoringSet)
            {
                Console.WriteLine( item.ClassGraphColoringToString());
                Console.WriteLine( "=================================================\n");
            }
        }    
    }
}
