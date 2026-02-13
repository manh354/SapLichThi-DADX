using SapLichThiCore.DataObjects;

namespace SapLichThiCore.DataStructures
{
    public class ClassGraph : Graph<ExamClass>
    {
        public ClassGraph(Dictionary<ExamClass, HashSet<ExamClass>> adjacencyList)
        {
            AdjacencyList = adjacencyList;
        }
        public ClassGraph(List<ExamClass> allClasses)
        {
            foreach (var item in allClasses)
            {
                AddVertex(item);
            }
        }
        public int NumberOfClasses => AdjacencyList.Count;
        public HashSet<ExamClass> this[ExamClass index]
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
                result += "Class id:" + item.Key.Id;
                foreach (var item2 in item.Value)
                {
                    result += " -> " + item2.Id;
                }
                result += "\n";
            }
            return result;
        }
    }
    public class Node
    {
        public ExamClass _Class;
    }
    public class Edge
    {
        public ExamClass _ToEdge;
    }
}
