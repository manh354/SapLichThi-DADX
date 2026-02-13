using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.ExamGroupInserter
{
    public class HRStableClasses : IExamGroupSchemeMaker
    {
        public List<ExamGroup> I_HardRails { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> AllCourse_Class_Dictionary { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> StudyClass_ExamClass_Dictionary { get; set; }
        public int[] BiasTable { get; set; }
        public int TotalLargeRoomCapacity { get; set; }
        // Input and Output
        public ExamSchedule Schedule { get; set; }
        public Dictionary<(int, int), bool> UsedSlots_Dictionary { get; set; }
        public RoomType PrioritizedRoomType { get; set; }
        private bool TryMatchMultipleClassForEachRoomSlotCocktailShaker(IEnumerable<Course> courses, IEnumerable<ExamClass> examClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueClasses, out Dictionary<Course, List<ExamClass>> residueClassesForEachCourse)
        {
            filledSlots = new List<RoomShiftScheme>();
            residueClassesForEachCourse = new Dictionary<Course, List<ExamClass>>();
            var listOfAllExamClass = new List<ExamClass>();
            var listOfExamClassesForEachCourse = new Dictionary<Course, List<ExamClass>>();
            foreach (var course in courses)
            {
                residueClassesForEachCourse.Add(course, new List<ExamClass>());
                listOfExamClassesForEachCourse.Add(course, new());
            }
            // Thêm các dữ liệu cần thiết vào các cấu trúc dữ liệu đã khởi tạo ở trên
            foreach (var examClass in examClasses)
            {
                listOfAllExamClass.Add(examClass);
                listOfExamClassesForEachCourse[examClass.StudyClass.Course].Add(examClass);
            }
            
            Console.WriteLine($"Tat ca cac lop exam bat dau : {listOfAllExamClass.Count}");
            // Sap xep du lieu giam dan (knapsack)
            foreach (var course in courses)
            {
                listOfAllExamClass = MakeDescendingByCapacityExamClassList(listOfAllExamClass);
                listOfExamClassesForEachCourse[course] = MakeDescendingByCapacityExamClassList(listOfExamClassesForEachCourse[course]);
            }
            

            //  Khoi tao du lieu cho vong lap dau tien, tao ra HashSet de kiem tra su ton tai cua cac lop.
            var examClassIndexFromTop = 0;
            var examClassIndexFromLast = listOfAllExamClass.Count() -1;
            var examClassIndexForEachCourseFromTop = new Dictionary<Course, int>();
            var examClassIndexForEachCourseFromLast = new Dictionary<Course, int>();
            var hashSetOfAllExamClasses = listOfAllExamClass.ToHashSet();
            var hashSetOfExamClassesForEachCourse = new Dictionary<Course, HashSet<ExamClass>>();
            foreach (var course in courses)
            {
                hashSetOfExamClassesForEachCourse.Add(course, listOfExamClassesForEachCourse[course].ToHashSet());
                examClassIndexForEachCourseFromTop.Add(course, 0);
                examClassIndexForEachCourseFromLast.Add(course, listOfExamClassesForEachCourse[course].Count -1);
            }
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlot = new List<ExamClass>();
                foreach (var examClass in listOfAllExamClass)
                {
                    if (!hashSetOfAllExamClasses.Contains(examClass))
                        continue;
                    var thisCourse = examClass.StudyClass.Course;
                    for (; ; )
                    { 
                        var thisExamClass = listOfExamClassesForEachCourse[thisCourse].ElementAtOrDefault(examClassIndexForEachCourseFromTop[thisCourse]);
                        if (thisExamClass == null)
                        {
                            break;
                        }
                        if (!hashSetOfExamClassesForEachCourse[thisCourse].Contains(thisExamClass))
                        {
                            break;
                        }
                        var sum = examClassesForThisSlot.Sum(x => x.Count);
                        if (roomSlot.room.Capacity * 0.55f >= sum + thisExamClass.Count)
                        {

                            examClassesForThisSlot.Add(thisExamClass);
                            hashSetOfAllExamClasses.Remove(thisExamClass);
                            hashSetOfExamClassesForEachCourse[thisCourse].Remove(thisExamClass);
                            examClassIndexForEachCourseFromTop[thisCourse] += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    /*for (; ; )
                    {
                        var thisExamClass = listOfExamClassesForEachCourse[thisCourse].ElementAtOrDefault(examClassIndexForEachCourseFromLast[thisCourse]);
                        if(thisExamClass == null)
                        {

                            break;
                        }
                        if (!hashSetOfExamClassesForEachCourse[thisCourse].Contains(thisExamClass))
                        {

                            break;
                        }
                        var sum = examClassesForThisSlot.Sum(x => x.Count);
                        if(roomSlot.room.Capacity * 0.55f >= sum + thisExamClass.Count)
                        {

                            examClassesForThisSlot.Add(thisExamClass);
                            hashSetOfAllExamClasses.Remove(thisExamClass);
                            hashSetOfExamClassesForEachCourse[thisCourse].Remove(thisExamClass);
                            examClassIndexForEachCourseFromLast[thisCourse] -= 1;
                        }
                        else
                        {
                            break;
                        }
                    }*/
                }
                filledSlots.Add(new(roomSlot.room, roomSlot.shift, examClassesForThisSlot));
            }
            var total = filledSlots.Select(s => s.ExamClasses.Count).Sum();
            Console.WriteLine($"Tat ca cac lop exam ket thuc: {total}");

            residueClasses = hashSetOfAllExamClasses.ToList();
            if (residueClasses.Count == 0)
                return true;
            foreach (var course in courses)
            {
                residueClassesForEachCourse[course] = hashSetOfExamClassesForEachCourse[course].ToList();
            }
            return false;
        }

        

        private List<ExamClass> MakeDescendingByCapacityExamClassList(IEnumerable<ExamClass> studyClasses)
        {
            return studyClasses.OrderByDescending(x => x.Count).ToList();
        }
        private List<RoomShiftSlot> MakeDescendingByCapacityLargeRoomSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShift = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShift = roomShift.Concat(rooms.Where(x => x.RoomType == RoomType.large).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShift = roomShift.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShift;
        }

        private List<RoomShiftSlot> MakeDescendingByCapacityMediumRoomSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShift = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShift = roomShift.Concat(rooms.Where(x => x.RoomType == RoomType.medium).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShift = roomShift.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShift;
        }

        private List<ExamClass> ConcatAllCourses(List<Course> courses)
        {
            List<ExamClass> result = new();
            foreach (Course course in courses)
            {
                foreach (var studyClass in AllCourse_Class_Dictionary[course])
                {
                    result = result.Concat(StudyClass_ExamClass_Dictionary[studyClass]).ToList();
                }
            }
            return result;
        }

        public void MakeScheme(ExamGroup hardRail, out List<RoomShiftScheme> filledSlots ,out bool fit)
        {
            fit = true;
            filledSlots = new List<RoomShiftScheme>();
            var examClasses = ConcatAllCourses(hardRail.Courses);
            var sortedClasses = MakeDescendingByCapacityExamClassList(examClasses);
            var allRooms = Schedule.rooms;
            var sortedLargeRooms = MakeDescendingByCapacityLargeRoomSlotsList(allRooms, hardRail.NumShift);
            var sortedMediumRooms = MakeDescendingByCapacityMediumRoomSlotsList(allRooms, hardRail.NumShift);
            if (!TryMatchMultipleClassForEachRoomSlotCocktailShaker(
                hardRail.Courses,
                examClasses,
                sortedLargeRooms,
                out List<RoomShiftScheme> studyClassPositionsForLargeRooms,
                out List<ExamClass> residueClassesForLargeRooms,
                out Dictionary<Course, List<ExamClass>> residueClassesForEachCourse))
            {
                Logger.logger.LogMessage($"HR_STABLE_CLASS: Thiếu phòng to cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))}, thử sang phòng vừa.");
            }
            //Console.WriteLine(studyClassPositionsForLargeRooms.Count);
            if (residueClassesForLargeRooms.Count() == 0)
            {
                Logger.logger.LogMessage($"HR_STABLE_CLASS: count :{residueClassesForLargeRooms.Count}");
                filledSlots = studyClassPositionsForLargeRooms;
                return;
            }
            if (!TryMatchMultipleClassForEachRoomSlotCocktailShaker(
                hardRail.Courses,
                residueClassesForLargeRooms,
                sortedMediumRooms,
                out List<RoomShiftScheme> studyClassPositionsForMediumRooms,
                out List<ExamClass> residueClassesForMeidumRooms,
                out Dictionary<Course, List<ExamClass>> residueClassesForEachCourseMedium))
            {
                Logger.logger.LogMessage($"HR_STABLE_CLASS: Thiếu phòng vừa cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))}");
                fit = false;
            }
            filledSlots = studyClassPositionsForLargeRooms.Concat(studyClassPositionsForMediumRooms).ToList();
            Logger.logger.LogMessage($"HR_STABLE_CLASS: count :{residueClassesForLargeRooms.Count}");
            //Console.WriteLine(studyClassPositionsForMediumRooms.Count);
            return;
        }
    }
}
