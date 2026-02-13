using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.Tests;
using System.Drawing.Printing;

namespace SapLichThiLib.AlgorithmsObjects.Coloring
{
    public class ClassGraphColorer
    {
        /// <summary>
        /// Input
        /// </summary>
        public ClassGraph? I_graph { get; set; }
        /// <summary>
        /// Output
        /// </summary>
        public Dictionary<int, HashSet<StudyClass>>? O_color_studyClasses { get; set; }
        public Dictionary<Course, StudentYear> O_course_mainStudentYear { get; set; }
        public void ColorGraph()
        {
            if (I_graph == null)
                throw new Exception("Graph is null");
            O_color_studyClasses = new();
            var sortedVertexByOrder = CreateSortedListHighestOrder(I_graph);
            /*HashSet<Class> nonOrder = sortedVertexByOrder.ToHashSet();*/
            O_color_studyClasses = new Dictionary<int, HashSet<StudyClass>>();
            int color = 0;
            Dictionary<string, int> nameBothCourseAndSchool_nameCount = new();
            Dictionary<Course, Dictionary<StudentYear, int>> course_allStudentYear = new();
            Dictionary<Course, StudentYear> course_mainStudentYear = new();
            // Dictionary<string, HashSet<Course>> nameBothCourseAndSchool_courseList = new();
            foreach ( var studyClass in sortedVertexByOrder )
            {
                Course course = studyClass.Course;
                var key = course.Name + course.School.ID;
                nameBothCourseAndSchool_nameCount.TryAdd(key, 0);
                nameBothCourseAndSchool_nameCount[key] += 1;

                course_allStudentYear.TryAdd(course, new());
                course_allStudentYear[course].TryAdd(studyClass.StudentYear, 0);
                course_allStudentYear[course][studyClass.StudentYear] += 1;
                // nameBothCourseAndSchool_courseList.TryAdd(key, new());
                // nameBothCourseAndSchool_courseList[key].Add(course);
            }
            foreach (var (course, studentYear_count) in course_allStudentYear)
            {
                course_mainStudentYear.Add(course, studentYear_count.MaxBy(x => x.Value).Key);
            }
            while (sortedVertexByOrder.Count > 0)
            {
                sortedVertexByOrder = sortedVertexByOrder.OrderByDescending(x => nameBothCourseAndSchool_nameCount[x.Course.Name + x.Course.School.ID]).ToList();

                HashSet<StudyClass> thisColoredClasses = new();
                var highestOrderVertex = sortedVertexByOrder[0];
                thisColoredClasses.Add(highestOrderVertex);
                sortedVertexByOrder.RemoveAt(0);
                var currentCourseGroupHash = new HashSet<string>
                {
                    highestOrderVertex.Course.Name + highestOrderVertex.Course.School.ID
                };
                var currentSchoolHash = new HashSet<School>
                {
                    highestOrderVertex.Course.School
                };
                var currentStudentYearHash = new HashSet<StudentYear>
                {
                    course_mainStudentYear[highestOrderVertex.Course]
                };

                // var locallyVisited = new HashSet<StudyClass>();
                // Ta sử dụng thuật toán tô màu sao cho chọn tô màu những môn thuộc cùng trường với nhau trước...
                var locallySortedVertexByOrder = sortedVertexByOrder
                    .FindAll(x => currentStudentYearHash.Contains(course_mainStudentYear[x.Course])).ToList()
                    // Cố add
                    .Concat(sortedVertexByOrder.FindAll(x => currentCourseGroupHash.Contains(x.Course.Name + x.Course.School.ID)))
                    .OrderByDescending(x => currentSchoolHash.Contains(x.Course.School))
                    .ThenByDescending(x => currentCourseGroupHash.Contains(x.Course.Name + x.Course.School.ID))
                    // .ThenByDescending(x => currentStudentYearHash.Contains(course_mainStudentYear[x.Course]))
                    .ThenByDescending(x => nameBothCourseAndSchool_nameCount[x.Course.Name + x.Course.School.ID])
                    .ToList();
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
                        /*Console.WriteLine("RemoveCount"+ sortedVertexByOrder.RemoveAll(x => x.Course == vertex.Course));*/
                        sortedVertexByOrder.Remove(consideringVertex);
                        locallySortedVertexByOrder.Remove(consideringVertex);
                        currentSchoolHash.Add(consideringVertex.Course.School);
                        if(currentCourseGroupHash.Add(consideringVertex.Course.Name + consideringVertex.Course.School.ID))
                        {
                            locallySortedVertexByOrder = locallySortedVertexByOrder
                            .Concat(sortedVertexByOrder.FindAll(x => x.Course.Name == consideringVertex.Course.Name && x.Course.School == consideringVertex.Course.School)).ToList();
                        }
                        locallySortedVertexByOrder = locallySortedVertexByOrder
                            .OrderByDescending(x => currentSchoolHash.Contains(x.Course.School))
                            // .ThenByDescending(x=> currentStudentYearHash.Contains(course_mainStudentYear[x.Course]))
                            .ThenByDescending(x => currentCourseGroupHash.Contains(x.Course.Name+x.Course.School.ID))
                            .ToList();
                    }
                } 
                var dividedCoursesHash = new HashSet<string>();
                foreach (var studyClass in thisColoredClasses)
                {
                    dividedCoursesHash.Add(studyClass.Course.Name + studyClass.Course.School.ID);
                }
                foreach (var strName in dividedCoursesHash)
                {
                    Console.WriteLine($"Detected name in color : {strName}");
                }
                foreach (var studyClass in sortedVertexByOrder.ToList())
                {
                    
                    var nameTobeRemoved = studyClass.Course.Name + studyClass.Course.School.ID;
                    if (dividedCoursesHash.Contains(nameTobeRemoved))       
                    {
                        // var nameTobeRemovedWithStudentYear = nameTobeRemoved + course_mainStudentYear[studyClass.Course].Name;
                        // Console.WriteLine($"Name removed {nameTobeRemovedWithStudentYear}");
                        Console.WriteLine($"Course to be removed {studyClass.Course.ID}, {studyClass.StudentYear.Name}");
                        foreach (var studyClassInColor in thisColoredClasses.ToList())
                        {
                            /*if (studyClassInColor.Course.Name + studyClassInColor.Course.School.ID + course_mainStudentYear[studyClassInColor.Course].Name == nameTobeRemovedWithStudentYear)
                            {
                                thisColoredClasses.Remove(studyClassInColor);
                                sortedVertexByOrder.Add(studyClassInColor);
                            }*/
                            if (studyClassInColor.Course.Name + studyClassInColor.Course.School.ID == nameTobeRemoved)
                            {
                                thisColoredClasses.Remove(studyClassInColor);
                                sortedVertexByOrder.Add(studyClassInColor);
                            }
                        }
                        dividedCoursesHash.Remove(nameTobeRemoved);
                    }
                }
                color++;
                Console.WriteLine($"Color {color}, count class in color {thisColoredClasses.Count}");
                O_color_studyClasses.Add(color, thisColoredClasses);
                O_course_mainStudentYear = course_mainStudentYear;
            }

            // ALL of this are test for verifying correctness of the processes.

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
                        */

            var allStudyClass = O_color_studyClasses.Select(x => x.Value.AsEnumerable()).Aggregate((x, y) => x.Concat(y));
            var allStudentYear = allStudyClass.Select(x => x.StudentYear).ToHashSet() ;
            Dictionary<StudentYear, int> studentYearCountPerShift = new();
            foreach (var studentYear1 in allStudentYear)
            {
                studentYearCountPerShift.Add(studentYear1,0);
            }

            foreach (var (colorInd, studyClasses) in O_color_studyClasses)
            {
                Console.WriteLine($"Color Index {colorInd}");
                var allStudentYearCount = studyClasses.GroupBy(x => course_mainStudentYear[x.Course]).Select(x => (x.Key, x.Count()));
                foreach (var (studentYear, countOfStudentYear) in allStudentYearCount)
                {
                    Console.WriteLine($"Student Year {studentYear.Name}, count : {countOfStudentYear}");
                }
                var studentYearCount = allStudentYearCount.GroupBy(x => x.Key).Select(x => (x.Key, x.Count())).MaxBy(x =>x.Item2);
                var (_studentYear, _countOfStudentYear) = studentYearCount;
                {
                    studentYearCountPerShift[_studentYear] += 1;
                }
            }

            foreach (var (studentYear, countOfStudentYear) in studentYearCountPerShift)
            {
                Console.WriteLine($"Student Year {studentYear.Name}, count : {countOfStudentYear}");
            }

            // var studentYearCount = allStudentYear.GroupBy(x => x.Key).Select(x => (x.Key, x.Count()));
            

        }



        public static List<StudyClass> CreateSortedListHighestOrder(ClassGraph classGraph)
        {
            return classGraph.AdjacencyList.Keys.OrderByDescending(x => classGraph.AdjacencyList[x].Count).ToList();
        }
    }
}
