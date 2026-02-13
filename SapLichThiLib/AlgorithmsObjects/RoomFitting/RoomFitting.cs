using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SapLichThiLib.AlgorithmsObjects.DynamicPooling
{
    internal class RoomFitting
    {
        public Dictionary<int, HashSet<StudyClass>> I_color_studyClasses;
        public Dictionary<StudyClass, List<ExamClass>> I_studyClass_ExamClasses;
        public Dictionary<Course, StudentYear> I_course_mainStudentYear { get; set; }
        public Dictionary<int, List<ExamClass>> O_color_examClasses;
        public Dictionary<int, List<Course>> O_color_course;
        Dictionary<(Room, Room), float> distanceBetweenRooms;
        private int largeAndMediumRoomCount;
        const int MAX_ITER = 100;
        public ClassRoomSea? I_sea;
        ClassRoomPool? currentPool;
        int currentPoolIndex;
        ClassRoomContainer? currentContainer;
        int currentContainerIndex;
        public void Run()
        {
            Logger.logger.LogMessage(
                "===============================================================" +
                "===================== ROOM FITTING.CS =========================" +
                "===============================================================");
            O_color_examClasses = new();
            O_color_course = new();
            foreach (var color_studyClasses_pair in I_color_studyClasses)
            {
                var color = color_studyClasses_pair.Key;
                var allStudyClasses = color_studyClasses_pair.Value;
                var examClasses = new List<ExamClass>();
                var courses = new HashSet<Course>();
                foreach (var studyClass in allStudyClasses)
                {
                    examClasses.AddRange(I_studyClass_ExamClasses[studyClass]);
                    courses.Add(studyClass.Course);
                }
                O_color_examClasses.Add(color, examClasses);
                O_color_course.Add(color, courses.ToList());
            }
            
            largeAndMediumRoomCount = I_sea.Pools.Find(x=>x.coursesInPool.Count == 0).Containers.Sum(x => x.Box.RoomType == RoomType.medium ? 1 : x.Box.RoomType == RoomType.large ? 2 : 0);
            FitRooms();
            Logger.logger.LogMessage(
                "###############################################################" +
                "##################### ROOM FITTING.CS #########################" +
                "###############################################################");
        }


        public void FitRooms()
        {
            if(I_sea == null)
            {
                throw new Exception("No sea exception");
            }
            // sort every pairs by sum of its value
            var sortedColors = 
                O_color_examClasses.OrderByDescending(
                    color_hashset => color_hashset.Value.Sum(
                        examClass => examClass.Count))
                .ToList() ;
            var dividedColors = new List<KeyValuePair<int, List<ExamClass>>>();
            // Choose the largest pool
            int largestPoolCount =
                I_sea.Pools.Max(pool => pool.GetRemainingCapacity());
            // First Filter and division
            // Split color by student year
            foreach (var thisColor in sortedColors)
            {
                SplitColorByStudentYear(thisColor, out var allDivisions);
                foreach (var colorPart in allDivisions)
                {
                    if (colorPart.Count > 0)
                        dividedColors.Add(new KeyValuePair<int, List<ExamClass>>(thisColor.Key, colorPart));
                }
            }
            sortedColors = dividedColors;
            Logger.logger.LogMessage("Divided Color count by Student Year: {0} ; largest pool : {1}", dividedColors.Count, largestPoolCount);
            // Next filter and division 
            // If color size is larger than the largest pool, divide by 2
            dividedColors = new();
            bool flag = true;
            foreach (var thisColor in sortedColors)
            {
                if (flag)
                {
                    // If the color is too large, split color into two.
                    var totalStudentOfColor = thisColor.Value.Sum(x => x.Count);
                    Console.WriteLine($"color {thisColor}, totalStudentOfColor {totalStudentOfColor}");
                    if (totalStudentOfColor >= largestPoolCount * 0.5)
                    {
                        // int n = (int)Math.Ceiling(totalStudentOfColor / (largestPoolCount * 0.5));
                        // TODO : Implement a N divisor color split.
                        SplitColorByCapacity (thisColor, totalStudentOfColor / 2, out var allDivisions);
                        foreach (var colorPart in allDivisions)
                        {
                            // Console.WriteLine($"Exam class in Color part count: {colorPart.Count}");
                            if(colorPart.Count > 0)
                               dividedColors.Add(new KeyValuePair<int, List<ExamClass>>(thisColor.Key, colorPart));
                        }

                        // These function only works for dividing color into 2. Not suitable for dividing into multiple parts.
                        // DEPRECATED
                        // -----------------------------------------------------
                        /*SplitColor(thisColor, totalStudentOfColor / 2, out var firstHalf, out var secondHalf);
                        dividedColors.Add(new KeyValuePair<int, List<ExamClass>>(thisColor.Key, firstHalf));
                        dividedColors.Add(new KeyValuePair<int, List<ExamClass>>(thisColor.Key, secondHalf));*/
                        // -----------------------------------------------------

                    }
                    // because the list is sorted, all element after the first element that is smaller than the largest pool
                    // must be strictly smaller than the largest pool also.
                    // We set flag here as false to convey this meaning
                    else
                    {
                        flag = false;
                        dividedColors.Add(thisColor);
                    }
                }
                // all remanining element should be added here.

                else
                {
                    dividedColors.Add(thisColor);
                }
            }
            Logger.logger.LogMessage($"Divided Color count by Capacity: {dividedColors.Count} ; largest pool : {largestPoolCount}");
            // We sort this list again to prepare for step 2.
            sortedColors = dividedColors.OrderByDescending(
                color_hashset => color_hashset.Value.Sum(
                        examClass => examClass.Count))
                .ToList();
            // Init current conditions 
            currentPool = null; //FindBestPool(); 
            currentContainer = null; //FindBestContainer();
            int indexPool = 0;
            
            int iter = 0;
            int examClassAddedCount = 0;
            HashSet<ClassRoomPool> pools = I_sea.Pools.ToHashSet();
            while (iter < MAX_ITER)
            {
                Logger.logger.LogMessage($"Xếp các lớp tại iteration {iter}");
                List<KeyValuePair<int, List<ExamClass>>> residueForEachColor = new();
                foreach (var (color,examClasses) in sortedColors)
                {
                    Dictionary<Course, int> course_courseCount = new();
                    foreach (var examClass in examClasses)
                    {
                        course_courseCount.TryAdd(examClass.StudyClass.Course,0);
                        course_courseCount[examClass.StudyClass.Course] += 1;
                    }
                    var sortedClasses = examClasses.OrderByDescending(x => course_courseCount[x.StudyClass.Course]).ThenByDescending(examClass => examClass.Count);
                    
                    // HashSet<ExamClass> allAddedExamClasses = new();

                    var totalStudentOfThisColor = examClasses.Sum(x => x.Count);
                    var smallestClassCount = examClasses.Min(x => x.Count);
                    var poolFound = I_sea.FindBestPool(O_color_course[color], I_course_mainStudentYear, examClasses.Count , totalStudentOfThisColor, smallestClassCount, out var bestPool, out var allSuitablePools);
                    if (!poolFound)
                    {
                        Logger.logger.LogMessage($"Không tồn tại Pool hợp lý cho mau {color}");
                        continue;
                    }
                    else
                    {
                        currentPool = bestPool;
                        Logger.logger.LogMessage($"Pool tốt nhất tìm được cho màu {color}: Ngày {bestPool.Date} , Kíp {bestPool.Shift}");
                        // Logger.logger.LogMessage("____________________________________________________");
                    }
                    var atLeastOneClassAdded = false;
                    var poolIndex = 0;

                    var examClassOfOneShift = sortedClasses.ToList();
                    List<ExamClass> residueClasseOfOneShift = new();
                    while (!atLeastOneClassAdded)
                    {
                        residueClasseOfOneShift = new();
                        HashSet<Course> rejectedCourseForThisShift = new();
                        foreach (var examClass in examClassOfOneShift)
                        {
                            if (rejectedCourseForThisShift.Contains(examClass.StudyClass.Course))
                            {
                                residueClasseOfOneShift.Add(examClass);
                                continue;
                            }
                            var containerFound = currentPool.FindBestContainer(examClass, currentContainer, out var bestContainer);
                            if (!containerFound)
                            {
                                Logger.logger.LogMessage($"Không tìm thấy Container cho lớp {examClass}, Kiểm tra môn {examClass.StudyClass.Course.ID} và chuyển sang kíp thi tiếp theo.");
                                bool willThisCourseBeRejected = course_courseCount[examClass.StudyClass.Course] < largeAndMediumRoomCount;
                                if(examClass.StudyClass.Course.ID == "PH1026")
                                    Console.WriteLine();
                                Logger.logger.LogMessage($"Ta kiểm tra số lượng lớp đủ nhỏ để môn này bị bỏ là {(willThisCourseBeRejected ? "Đúng" : "Sai")}");
                                if(willThisCourseBeRejected)
                                    rejectedCourseForThisShift.Add(examClass.StudyClass.Course);
                                foreach (var container in currentPool.Containers)
                                {
                                    if (container.Elements.Any(x => x.StudyClass.Course == examClass.StudyClass.Course) && container.Elements.Any(x => course_courseCount[x.StudyClass.Course] < largeAndMediumRoomCount))
                                    {
                                        foreach (var affectedExamClasss in container.Elements)
                                        {
                                            residueClasseOfOneShift.Add(affectedExamClasss);
                                        }
                                        container.RemoveAllElement();
                                    }
                                }
                                residueClasseOfOneShift.Add(examClass);
                                continue;
                            }
                            else
                            {
                                currentContainer = bestContainer;
                            }
                            if (currentPool.TryAddElementToPool(examClass, currentContainer))
                            {
                                // allAddedExamClasses.Add(examClass);
                                atLeastOneClassAdded = true;
                                Logger.logger.LogMessage($"Tìm thấy Container cho lớp {examClass}, đếm số lượng {examClassAddedCount} ");
                                examClassAddedCount++;
                            }
                        }
                        if(!atLeastOneClassAdded)
                        {
                            if (++poolIndex < allSuitablePools.Count)
                            {
                                currentPool = allSuitablePools[poolIndex];
                                Logger.logger.LogMessage($"Chuyển sang kíp thi tốt thứ {poolIndex + 1}: Ngày {currentPool.Date} , Kíp {currentPool.Shift}");
                                examClassOfOneShift = residueClasseOfOneShift;
                                residueClasseOfOneShift = new();
                                continue;
                            }
                            break;
                        }
                    }
                    var allRemainingClass = residueClasseOfOneShift;
                    if (residueClasseOfOneShift.Count > 0)
                        residueForEachColor.Add(new KeyValuePair<int, List<ExamClass>>(color, allRemainingClass));
                }
                if (residueForEachColor.Count > 0)
                    sortedColors = residueForEachColor;
                else
                {
                    break;
                }
                iter += 1;
            }
            //
        }
         
        private void PutOneColor(HashSet<ExamClass> color, out List<ExamClass> remains)
        {
                throw new NotImplementedException();
            foreach(var examClass in color)
            {
            }
        }
        private void PutOneColor(KeyValuePair<int,List<ExamClass>> color  )
        {
            foreach (var examClass in color.Value)
            {
                //CreateAndExecuteCommand(examClass);
            }
        }

        private void SplitColorByStudentYear(KeyValuePair<int, List<ExamClass>> color, out List<List<ExamClass>> allDivisions)
        {
            var group = color.Value.GroupBy(x => I_course_mainStudentYear[x.StudyClass.Course]);
            allDivisions = group.Select(x => x.ToList()).ToList();
        }

        private void SplitColorByCapacity(KeyValuePair<int, List<ExamClass>> color, int divideThreshold, out List<ExamClass> firstHalf, out List<ExamClass> secondHalf)
        {
            firstHalf = new List<ExamClass>();
            secondHalf = new List<ExamClass>();
            Dictionary<Course,  int> course_totalStudents = new();
            Dictionary<Course, List<ExamClass>> course_examClasses = new();
            foreach (var examClass in color.Value)
            { 
                Course course = examClass.StudyClass.Course;
                if(!course_totalStudents.ContainsKey(course))
                {
                    course_totalStudents.Add(course, 0);
                    course_examClasses.Add(course, new());
                }
                course_totalStudents[course] += examClass.Count;
                course_examClasses[course].Add(examClass);
            }
            int sum = 0;
            bool first = true;
            var sorted = course_totalStudents.OrderByDescending(x => x.Value);
            foreach (var (course, totalStudentOfCourse) in sorted)
            {
                if(first)
                {
                    sum += totalStudentOfCourse;
                    firstHalf.AddRange(course_examClasses[course]);
                    if(sum >= divideThreshold)
                    {
                        first = false;
                    }
                }
                else
                {
                    secondHalf.AddRange(course_examClasses[course]);
                }
            }
        }

        private void SplitColorByCapacity(KeyValuePair<int, List<ExamClass>> color_classes, int divideThreshold, out List<List<ExamClass>> allDivisions)
        {
            var (color, examClasses) = color_classes;
            allDivisions = new();
            Dictionary<Course, int> course_totalStudents = new();
            Dictionary<Course, List<ExamClass>> course_examClasses = new();
            Dictionary<string, HashSet<Course>> courseName_courseList = new();
            foreach (var examClass in examClasses)
            {
                Course course = examClass.StudyClass.Course;
                if (!courseName_courseList.ContainsKey(course.Name))
                {
                    courseName_courseList.Add(course.Name, new());
                }
                courseName_courseList[course.Name].Add(course);
                if (!course_totalStudents.ContainsKey(course))
                {
                    course_totalStudents.Add(course, 0);
                    course_examClasses.Add(course, new());
                }
                course_totalStudents[course] += examClass.Count;
                course_examClasses[course].Add(examClass);
            }
            int sum = 0;
            bool divided = false;

            var sorted_courseList = courseName_courseList.OrderByDescending(x => x.Value.Count).ToList();
            List<ExamClass> divisionPart = new();
            int i = 0;
            while (i < sorted_courseList.Count)
            {
                while (!divided)
                {
                    if (i == sorted_courseList.Count)
                    {
                        divided = true;
                        break;
                    }
                    var (courseName, courseList) = sorted_courseList[i];
                    foreach (var course in courseList)
                    {
                        var totalStudentOfCourse = course_totalStudents[course];
                        sum += totalStudentOfCourse;
                        divisionPart.AddRange(course_examClasses[course]);
                    }

                    if (sum >= divideThreshold)
                    {
                        divided = true;
                    }
                    i++;
                }
                allDivisions.Add(divisionPart);
                divisionPart = new();
                sum = 0;
                divided = false;
            }
        }


        public Command CreateAndExecuteCommand(ClassRoomContainer container, ClassRoomPool pool, ExamClass examClass )
        {
            if (currentContainer == null || currentPool == null)
                throw new Exception("ClassRoomTreeChoices.cs : error creating command, Null values");
            Command command = Command.CreatCommand(currentContainer, currentPool, examClass);
            command.Execute();
            return command;
        }

       
        public ClassRoomContainer GetNextContainer()
        {
            if(currentContainer == null)
            {
                throw new NotImplementedException();
            }
            var currentRoom = currentContainer.Box;
            var sortedContainers = currentPool.Containers.OrderByDescending(x => distanceBetweenRooms[(currentRoom, x.Box)]).ThenByDescending(x => x.GetBoxCapacity());
            return sortedContainers.First();
        }
    }
}
