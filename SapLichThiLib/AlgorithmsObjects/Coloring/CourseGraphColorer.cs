using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.Coloring
{
    public class CourseGraphColorer
    {
        /// <summary>
        /// Input
        /// </summary>
        public CourseGraph? I_graph { get; set; }
        /// <summary>
        /// Output
        /// </summary>
        public Dictionary<int, HashSet<Course>>? O_color_courses { get; set; }
        public void ColorGraph()
        {
            if (I_graph == null)
                throw new Exception("Graph is null");
            O_color_courses = new();
            var sortedVertexByOrder = CreateSortedListHighestOrder(I_graph);
            /*HashSet<Class> nonOrder = sortedVertexByOrder.ToHashSet();*/
            O_color_courses = new Dictionary<int, HashSet<Course>>();
            int color = 0;
            Dictionary<string, int> nameBothCourseAndSchool_nameCount = new();
            foreach (var course in sortedVertexByOrder)
            {
                var key = course.Name + course.School.ID;
                nameBothCourseAndSchool_nameCount.TryAdd(key, 0);
                nameBothCourseAndSchool_nameCount[key] += 1;
            }
            while (sortedVertexByOrder.Count > 0)
            {
                sortedVertexByOrder = sortedVertexByOrder.OrderByDescending(x => I_graph.AdjacencyList[x].Count).ToList();

                HashSet<Course> thisColoredClasses = new();
                var highestOrderVertex = sortedVertexByOrder[0];
                thisColoredClasses.Add(highestOrderVertex);
                sortedVertexByOrder.RemoveAt(0);
                var currentCourseGroupHash = new HashSet<string>
                {
                    highestOrderVertex.Name
                };

                var locallyVisited = new HashSet<Course>();
                // Ta sử dụng thuật toán tô màu sao cho chọn tô màu những môn thuộc cùng trường với nhau trước...
                sortedVertexByOrder = sortedVertexByOrder.OrderByDescending(x => nameBothCourseAndSchool_nameCount[x.Name + x.School.ID]).ThenByDescending(x => currentCourseGroupHash.Contains(x.Name)).ToList();
                var locallySortedVertexByOrder = sortedVertexByOrder.ToList();
                // Đoạn code xét tất cả các đỉnh thuộc đồ thi, tìm ra những 
                // đỉnh mà có chung cạnh với các đỉnh thuộc tập hợp đỉnh đã
                // tô màu.
                while (locallySortedVertexByOrder.Count != 0)
                {
                    var consideringVertex = locallySortedVertexByOrder[0];
                    var edgeVertices = I_graph.AdjacencyList[consideringVertex];

                    // Biến kiểm tra đỉnh có cạnh kề với tập hợp các đỉnh đã được tô màu sẵn hay không
                    bool haveCommonVertex = false;
                    foreach (var edgeVertex in edgeVertices)
                    {
                        if (thisColoredClasses.Contains(edgeVertex))
                        {
                            haveCommonVertex = true;
                            locallySortedVertexByOrder.Remove(consideringVertex);
                            break;
                        }
                    }
                    if (!haveCommonVertex)
                    {
                        thisColoredClasses.Add(consideringVertex);
                        Console.WriteLine($"{consideringVertex}, color : {color}");
                        /*Console.WriteLine("RemoveCount"+ sortedVertexByOrder.RemoveAll(x => x.Course == vertex.Course));*/
                        sortedVertexByOrder.Remove(consideringVertex);
                        locallySortedVertexByOrder.Remove(consideringVertex);
                        currentCourseGroupHash.Add(consideringVertex.Name);
                        locallySortedVertexByOrder.OrderByDescending(x => currentCourseGroupHash.Contains(x.Name)).ToList();
                    }
                }
                color++;
                O_color_courses.Add(color, thisColoredClasses);
            }

            /*while (sortedVertexByOrder.Count > 0)
            {
                HashSet<StudyClass> thisColoredClasses = new();
                var highestOrderVertex = sortedVertexByOrder[0];
                thisColoredClasses.Add(highestOrderVertex);
                sortedVertexByOrder.RemoveAt(0);
                var currentCourseGroupHash = new HashSet<string>
                {
                    highestOrderVertex.Course.Name
                };
                var locallyVisited = new HashSet<StudyClass>();
                // Ta sử dụng thuật toán tô màu sao cho chọn tô màu những môn thuộc cùng trường với nhau trước...
                sortedVertexByOrder = sortedVertexByOrder.OrderByDescending(x => currentCourseGroupHash.Contains(x.Course.Name)).ToList();
                var locallySortedVertexByOrder = sortedVertexByOrder.ToList();
                // Đoạn code xét tất cả các đỉnh thuộc đồ thi, tìm ra những 
                // đỉnh mà có chung cạnh với các đỉnh thuộc tập hợp đỉnh đã
                // tô màu.
                while (locallySortedVertexByOrder.Count() != 0)
                {
                    var consideringVertex = locallySortedVertexByOrder[0];
                    var edgeVertices = I_graph.AdjacencyList[consideringVertex];

                    // Biến kiểm tra đỉnh có cạnh kề với tập hợp các đỉnh đã được tô màu sẵn hay không
                    bool haveCommonVertex = false;
                    foreach (var edgeVertex in edgeVertices)
                    {
                        if (thisColoredClasses.Contains(edgeVertex))
                        {
                            haveCommonVertex = true;
                            locallySortedVertexByOrder.Remove(consideringVertex);
                            break;
                        }
                    }
                    if (!haveCommonVertex)
                    {
                        thisColoredClasses.Add(consideringVertex);
                        *//*Console.WriteLine("RemoveCount"+ sortedVertexByOrder.RemoveAll(x => x.Course == vertex.Course));*//*
                        sortedVertexByOrder.RemoveAll(x => x.ID == consideringVertex.ID);
                        locallySortedVertexByOrder.Remove(consideringVertex);
                    }
                }
                color++;
                O_color_studyClasses.Add(color, thisColoredClasses);
            }*/
        }



        public static List<Course> CreateSortedListHighestOrder(CourseGraph courseGraph)
        {
            return courseGraph.AdjacencyList.Keys.OrderByDescending(x => courseGraph.AdjacencyList[x].Count).ToList();
        }
    }

}
