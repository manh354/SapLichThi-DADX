using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.ErrorAndLog;

namespace SapLichThiLib.AlgorithmsObjects.ExamGroupInserter
{
    public class HRNoOverlapBetweenCourse : IExamGroupSchemeMaker
    {
        public Dictionary<Course, HashSet<StudyClass>> AllCourse_Class_Dictionary { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> StudyClass_ExamClass_Dictionary { get; set; }
        private double P_basePercentage { get; set; } = 0.6;
        public int TotalLargeRoomCapacity { get; set; }
        // Input and Output
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<(int, int), bool> UsedSlots_Dictionary { get; set; }

        private bool TryMatchTwoExamClassesWithRoomSlots(IEnumerable<ExamClass> examClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            var hashSetOfExamClass = examClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int totalSlots = roomSlots.Count();
            var lastClass = examClasses.Last();
            int examClassCount = 0;
            ExamClass thisExamClass = examClasses.ElementAt(0);
            bool reachedEnd = false;
            foreach (var roomSlot in roomSlots)
            {
                thisExamClass = hashSetOfExamClass.First();
                List<ExamClass> examClassesForThisSlots = new();
                for (int i = 0; i < 2; i++)
                {
                    int sum = 0;
                    examClassesForThisSlots.ForEach(x => sum = sum + x.Count);
                    if (roomSlot.room.Capacity * P_basePercentage >= sum + thisExamClass.Count)
                    {
                        examClassesForThisSlots.Add(thisExamClass);
                        hashSetOfExamClass.Remove(thisExamClass);
                        examClassCount++;
                    }
                    else
                    {
                        break;
                    }
                    if (examClassCount >= examClasses.Count())
                    {
                        reachedEnd = true;
                        break;
                    }
                    var allExamClassesOfTheSameCourse = hashSetOfExamClass.Where(x => x.StudyClass.Course == thisExamClass.StudyClass.Course);
                    if (allExamClassesOfTheSameCourse.Count() == 0)
                        break;
                    thisExamClass = allExamClassesOfTheSameCourse.First();
                }
                filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                if (reachedEnd)
                {
                    break;
                }
            }
            if (hashSetOfExamClass.Count > 0)
                enoughRoomForClasses = false;
            residueClasses = hashSetOfExamClass.ToList();
            return enoughRoomForClasses;
        }

        private bool TryMatchExamClassesWithRoomSlots (IEnumerable<ExamClass> examClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueExamClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            HashSet<ExamClass> hashSetOfExamClasses = examClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int totalSlots = roomSlots.Count();
            var lastClass = examClasses.Last();
            int examClassIndex = 0;
            ExamClass thisExamClass = examClasses.ElementAt(examClassIndex);
            bool reachedEnd = false;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlot = new();
                int sum = 0;
                examClassesForThisSlot.ForEach(x => sum = sum + x.Count);
                if (roomSlot.room.Capacity * 0.55f >= sum + thisExamClass.Count)
                {
                    examClassesForThisSlot.Add(thisExamClass);
                    hashSetOfExamClasses.Remove(thisExamClass);
                    examClassIndex++;
                }
                else
                {
                    break;
                }
                if (examClassIndex >= examClasses.Count())
                {
                    reachedEnd = true;
                    break;
                }
                thisExamClass = examClasses.ElementAt(examClassIndex);
                filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlot));
            }
            if (hashSetOfExamClasses.Count > 0)
                enoughRoomForClasses = false;
            residueExamClasses = hashSetOfExamClasses.ToList();
            return enoughRoomForClasses;
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
        private List<RoomShiftSlot> MakeDescendingByCapacityRoomTypeSlotsList(IEnumerable<Room> rooms, RoomType roomType, int totalShift)
        {
            List<RoomShiftSlot> roomShift = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShift = roomShift.Concat(rooms.Where(x => x.RoomType == roomType).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShift = roomShift.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShift;
        }
        private List<ExamClass> ConcatAllCourses (List<Course> courses)
        {
            List<ExamClass> result = new();
            foreach (Course course in courses)
            {
                foreach (var studyClass in AllCourse_Class_Dictionary[course])
                {
                    result = result.Concat(StudyClass_ExamClass_Dictionary[studyClass]).ToList() ;
                }
            }
            return result;
        }

        public void MakeScheme(ExamGroup hardRail, out List<RoomShiftScheme> filledSlots,out bool fit)
        {
            fit = true;
            filledSlots = new List<RoomShiftScheme>();
            var examClasses = ConcatAllCourses(hardRail.Courses);
            var sortedClasses = MakeDescendingByCapacityExamClassList(examClasses);
            var allRooms = I_schedule.rooms;
            foreach (var roomType in hardRail.PrioritizedRooms)
            {
                var sortedRoomTypes = MakeDescendingByCapacityRoomTypeSlotsList(allRooms, roomType, hardRail.NumShift);
                if (!TryMatchTwoExamClassesWithRoomSlots(
                    sortedClasses,
                    sortedRoomTypes,
                    out List<RoomShiftScheme> studyClassPositionsForRoomType,
                    out List<ExamClass> residueClassesForRoomType))
                {
                    Logger.logger.LogMessage($"Thiếu phòng {roomType} cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.Name))}, thử sang phòng vừa.");
                }
                if (residueClassesForRoomType.Count() == 0)
                {
                    filledSlots.AddRange(studyClassPositionsForRoomType);
                    return;
                }
                filledSlots.AddRange(studyClassPositionsForRoomType);
                sortedClasses = residueClassesForRoomType;
            }
            fit = false;
            return;
        }
    }
}
