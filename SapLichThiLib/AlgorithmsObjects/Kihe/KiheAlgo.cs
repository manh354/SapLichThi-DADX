using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.Kihe
{
    internal class KiheAlgo : IAlgorithmObject
    {
        public List<Room> I_rooms { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_class_Dict { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClass_examClass_Dict { get; set; }
        public void Run()
        {
            throw new NotImplementedException();
        }

        public void CheckAllInput()
        {
            throw new NotImplementedException();
        }

        public void InitializeAllOutput()
        {
            throw new NotImplementedException();
        }

        public void ProcedureRun()
        {
            throw new NotImplementedException();
        }

        private bool TryMatchTwoExamClassesWithRoomSlots(IEnumerable<ExamClass> examClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            var hashSetOfExamClass = examClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int totalSlots = roomSlots.Count();
            var lastClass = examClasses.Last();
            int examClassIndex = 0;
            ExamClass thisExamClass = examClasses.ElementAt(examClassIndex);
            bool reachedEnd = false;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                for (int i = 0; i < 2; i++)
                {
                    int sum = 0;
                    examClassesForThisSlots.ForEach(x => sum = sum + x.Count);
                    if (roomSlot.room.Capacity * 0.6f >= sum + thisExamClass.Count)
                    {
                        examClassesForThisSlots.Add(thisExamClass);
                        hashSetOfExamClass.Remove(thisExamClass);
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

        private bool TryMatchExamClassesWithMediumRoomSlots(IEnumerable<ExamClass> examClasses, IEnumerable<RoomShiftSlot> mediumRoomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueExamClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            HashSet<ExamClass> hashSetOfExamClasses = examClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int totalSlots = mediumRoomSlots.Count();
            var lastClass = examClasses.Last();
            int examClassIndex = 0;
            ExamClass thisExamClass = examClasses.ElementAt(examClassIndex);
            bool reachedEnd = false;
            foreach (var roomSlot in mediumRoomSlots)
            {
                List<ExamClass> examClassesForThisSlot = new();
                int sum = 0;
                examClassesForThisSlot.ForEach(x => sum = sum + x.Count);
                if (roomSlot.room.Capacity * 0.6f >= sum + thisExamClass.Count)
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



        private List<StudyClass> MakeDescendingByCapacityStudyClassList(IEnumerable<StudyClass> studyClasses)
        {
            return studyClasses.OrderByDescending(x => x.Count).ToList();
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

        private List<RoomShiftSlot> MakeDescendingRoomsSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShift = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShift = rooms.Select(s => new RoomShiftSlot(s, currentShift)).ToList();
            }
            roomShift = roomShift.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShift;
        }


        private List<ExamClass> ConcatAllCourses(List<Course> courses)
        {
            List<ExamClass> result = new();
            foreach (Course course in courses)
            {
                foreach (var studyClass in I_course_class_Dict[course])
                {
                    result = result.Concat(I_studyClass_examClass_Dict[studyClass]).ToList();
                }
            }
            return result;
        }

        public void MakeScheme(ExamGroup hardRail, out List<RoomShiftScheme> filledSlots)
        {
            filledSlots = new List<RoomShiftScheme>();
            var examClass = ConcatAllCourses(hardRail.Courses);
            var sortedClasses = MakeDescendingByCapacityExamClassList(examClass);
            var allRooms = I_rooms;
            var sortedLargeRooms = MakeDescendingByCapacityLargeRoomSlotsList(allRooms, hardRail.NumShift);
            var sortedMediumRooms = MakeDescendingByCapacityMediumRoomSlotsList(allRooms, hardRail.NumShift);
            if (!TryMatchTwoExamClassesWithRoomSlots(
                sortedClasses,
                sortedLargeRooms,
                out List<RoomShiftScheme> studyClassPositionsForLargeRooms,
                out List<ExamClass> residueClassesForLargeRooms))
            {
                Logger.logger.LogMessage($"Thiếu phòng to cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.Name))}, thử sang phòng vừa.");
            }
            if (residueClassesForLargeRooms.Count() == 0)
            {
                filledSlots = studyClassPositionsForLargeRooms;
                return;
            }
            if (!TryMatchTwoExamClassesWithRoomSlots(
                residueClassesForLargeRooms,
                sortedMediumRooms,
                out List<RoomShiftScheme> studyClassPositionsForMediumRooms,
                out List<ExamClass> residueClassesForMediumRooms))
            {
                Logger.logger.LogMessage($"Thiếu phòng vừa cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.Name))}");
            }
            filledSlots = studyClassPositionsForLargeRooms.Concat(studyClassPositionsForMediumRooms).ToList();
            return;
        }

        /*public void MakeScheme(HardRail hardRail, out List<RoomShiftScheme> filledSlots)
        {
            filledSlots = new List<RoomShiftScheme>();
            var allRooms = I_rooms;
            var sortedLargeRooms = MakeDescendingByCapacityLargeRoomSlotsList(allRooms, hardRail.NumShift);
            var sortedMediumRooms = MakeDescendingByCapacityMediumRoomSlotsList(allRooms, hardRail.NumShift);
            var concatRooms = sortedLargeRooms.Concat(sortedMediumRooms).ToList();
            var filledSlot = new List<RoomShiftScheme>();
            foreach (var course in hardRail.Courses)
            {
                var roomList = FilledRoom(filledSlot, MakeDescendingRoomsSlotsList(concatRooms,hardRail.NumShift));
                List<ExamClass> examClasses = ConcatExamClassesInCourse(course);
                examClasses = MakeDescendingByCapacityExamClassList(examClasses);
                bool step1 = TryMatchTwoExamClassesWithRoomSlots(examClasses, sortedLargeRooms, out var filledSlot1, out var residueClasses1);
                filledSlot = filledSlot.Concat(filledSlot1).ToList();
                if (step1) break;
                bool step2 = TryMatchTwoExamClassesWithRoomSlots(examClasses, sortedMediumRooms, out var filledSlot2, out var residueClasses2);
                filledSlot = filledSlot.Concat(filledSlot2).ToList();
                if (step2) break;
            }
            return;
        }*/

        public List<ExamClass> ConcatExamClassesInCourse(Course course)
        {
            var result = new List<ExamClass>();
            foreach (var studyClass in I_course_class_Dict[course])
            {
                result = result.Concat(I_studyClass_examClass_Dict[studyClass]).ToList();
            }
            return result;
        }
        public List<Room> FilledRoom(List<RoomShiftScheme> filledSlot, List<Room> concatRooms)
        {
            HashSet<Room> allRooms = concatRooms.ToHashSet();
            foreach (var slot in filledSlot)
            {
                if (allRooms.Contains(slot.Room))
                {
                    allRooms.Remove(slot.Room);
                }
            }
            var roomList = allRooms.ToList();
            return roomList;
        }
        
    }
}
