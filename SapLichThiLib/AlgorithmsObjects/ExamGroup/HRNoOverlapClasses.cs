using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.ExamGroupInserter
{
    public class HRNoOverlapClasses : IExamGroupSchemeMaker
    {
        // Input
        public Dictionary<Course, HashSet<StudyClass>> I_courses_studyClasses { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClasses_examClasses { get; set; }
        // Input and Output
        public ExamSchedule I_schedule { get; set; }
        public bool TryMatchStudyClassWithRoomSlots(IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            var hashSetOfStudyClass = studyClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int studyClassIndex = 0;
            StudyClass thisStudyClass = studyClasses.ElementAt(studyClassIndex);
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                if (studyClassIndex >= studyClasses.Count())
                    break;
                thisStudyClass = studyClasses.ElementAt(studyClassIndex);
                while(!(roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count))
                {
                    studyClassIndex++;
                    if (studyClassIndex >= studyClasses.Count())
                        break;
                    thisStudyClass = studyClasses.ElementAt(studyClassIndex);
                }
                if (roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count)
                {
                    examClassesForThisSlots.AddRange( I_studyClasses_examClasses[thisStudyClass]);
                    hashSetOfStudyClass.Remove(thisStudyClass);
                    studyClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                }
            }
            if (hashSetOfStudyClass.Count > 0)
                enoughRoomForClasses = false;
            residueClasses = hashSetOfStudyClass.ToList();
            return enoughRoomForClasses;
        }
        private List<StudyClass> MakeDescendingByCapacityStudyClassList(IEnumerable<StudyClass> studyClasses)
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
                foreach (var studyClass in I_courses_studyClasses[course])
                {
                    result = result.Concat(I_studyClasses_examClasses[studyClass]).ToList();
                }
            }
            return result;
        }
        private List<StudyClass> ConcatAllCourseToStudyClass(List<Course> courses)
        {
            List<StudyClass> result = new();
            foreach (var course in courses)
            {
                result = result.Concat(I_courses_studyClasses[course]).ToList();
            }
            return result;
        }

        public void MakeScheme(ExamGroup hardRail, out List<RoomShiftScheme> filledSlots, out bool fit)
        {
            fit = true;
            filledSlots = new List<RoomShiftScheme>();
            var studyClasses = ConcatAllCourseToStudyClass(hardRail.Courses);
            var sortedClasses = MakeDescendingByCapacityStudyClassList(studyClasses);
            var allRooms = I_schedule.rooms;
            var sortedMediumRooms = MakeDescendingByCapacityMediumRoomSlotsList(allRooms, hardRail.NumShift);
            var sortedLargeRooms = MakeDescendingByCapacityLargeRoomSlotsList(allRooms, hardRail.NumShift);
            List<StudyClass> residueClassesForRoomType;
            List<RoomShiftScheme> studyClassPositions = new();
            foreach (var roomType in hardRail.PrioritizedRooms)
            {
                var sortedRoomTypes = MakeDescendingByCapacityRoomTypeSlotsList(allRooms, roomType, hardRail.NumShift);
                if (!TryMatchStudyClassWithRoomSlots(
                    sortedClasses,
                    sortedRoomTypes,
                    out List<RoomShiftScheme> studyClassPositionsForRoomType,
                    out residueClassesForRoomType))
                {
                    Logger.logger.LogMessage($"Thiếu phòng {roomType} cho môn {String.Join('_', hardRail.Courses.Select(x => x.Name))}.");
                }
                if (residueClassesForRoomType.Count() == 0)
                {
                    filledSlots = studyClassPositionsForRoomType;
                    return;
                }
                studyClassPositions.AddRange(studyClassPositionsForRoomType);
                sortedClasses = residueClassesForRoomType;

            }
            
            return;
        }

        private void MakeSchemeLargeRoomPrioritized(ExamGroup hardRail, List<StudyClass> sortedClasses, List<RoomShiftSlot> sortedMediumRooms, List<RoomShiftSlot> sortedLargeRooms, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClassesForMediumRooms)
        {
            if (!TryMatchStudyClassWithRoomSlots(
                sortedClasses,
                sortedLargeRooms,
                out List<RoomShiftScheme> studyClassPositionsForLargeRooms,
                out List<StudyClass> residueClassesForLargeRooms))
            {
                Logger.logger.LogMessage($"HR_NO_OVERLAPSE_CLASSES: Thiếu phòng to cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))} thử sang phòng vừa.");
            }
            if (residueClassesForLargeRooms.Count == 0)
            {
                filledSlots = studyClassPositionsForLargeRooms;
                residueClassesForMediumRooms = new();
                return;
            }
            if (!TryMatchStudyClassWithRoomSlots(
                residueClassesForLargeRooms,
                sortedMediumRooms,
                out List<RoomShiftScheme> studyClassPositionsForMediumRooms,
                out residueClassesForMediumRooms))
            {
                Logger.logger.LogMessage($"HR_NO_OVERLAPSE_CLASSES: Thiếu phòng vừa cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))}.");
            }
            filledSlots = studyClassPositionsForLargeRooms.Concat(studyClassPositionsForMediumRooms).ToList();
        }

        private void MakeSchemeMediumRoomPrioritized(ExamGroup hardRail, List<StudyClass> sortedClasses, List<RoomShiftSlot> sortedMediumRooms, List<RoomShiftSlot> sortedLargeRooms, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClassesForLargeRooms)
        {
            if (!TryMatchStudyClassWithRoomSlots(
                sortedClasses,
                sortedMediumRooms,
                out List<RoomShiftScheme> studyClassPositionsForMediumRooms,
                out List<StudyClass> residueClassesForMediumRooms)) 
            {
                Logger.logger.LogMessage($"HR_NO_OVERLAPSE_CLASSES: Thiếu phòng vừa cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))}, thử sang phòng to.");
            }
            if (residueClassesForMediumRooms.Count == 0)
            {
                filledSlots = studyClassPositionsForMediumRooms;
                residueClassesForLargeRooms = new();
                return;
            }
            if (!TryMatchStudyClassWithRoomSlots(
                residueClassesForMediumRooms,
                sortedLargeRooms,
                out List<RoomShiftScheme> studyClassPositionsForLargeRooms,
                out residueClassesForLargeRooms))
            {
                Logger.logger.LogMessage($"HR_NO_OVERLAPSE_CLASSES: Thiếu phòng to cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.ID))}");
            }
            filledSlots = studyClassPositionsForMediumRooms.Concat(studyClassPositionsForLargeRooms).ToList();
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
    }
}
