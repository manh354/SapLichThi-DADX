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
    public class HRSmallClasses : IExamGroupSchemeMaker
    {
        public Dictionary<Course, HashSet<StudyClass>> I_courses_studyClasses { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClasses_examClasses { get; set; }
        // Input and Output
        public ExamSchedule Schedule { get; set; }
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
                if (roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count)
                {
                    examClassesForThisSlots.AddRange(I_studyClasses_examClasses[thisStudyClass]);
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


        private List<RoomShiftSlot> MakeDescendingByCapacitySmallRoomSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShift = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShift = roomShift.Concat(rooms.Where(x => x.RoomType == RoomType.small).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShift = roomShift.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShift;
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
            var allRooms = Schedule.rooms;
            var sortedSmallRooms = MakeDescendingByCapacitySmallRoomSlotsList(allRooms, hardRail.NumShift);
            Console.WriteLine(hardRail.PrioritizedRooms[0]);
            if (hardRail.PrioritizedRooms[0] == RoomType.small)
            {
                MakeSchemeSmallRoomPrioritized(hardRail, sortedClasses, sortedSmallRooms, out filledSlots, out var residueClassesForMediumRooms);
                if(residueClassesForMediumRooms.Count > 0) 
                { 
                    fit = false; 
                }
            }
            return;
        }
        public void MakeSchemeSmallRoomPrioritized(ExamGroup hardRail, IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> sortedSmallRooms, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClassesForMediumRooms)
        {
            filledSlots = new();
            if (!TryMatchStudyClassWithRoomSlots(
                studyClasses,
                sortedSmallRooms,
                out List<RoomShiftScheme> studyClassPositionsForMediumRooms,
                out residueClassesForMediumRooms))
            {
                Logger.logger.LogMessage($"HR_NO_OVERLAPSE_CLASSES: Thiếu phòng nhỏ cho hệ lớp {String.Join('_', hardRail.Courses.Select(x => x.Name))}");
            }
            filledSlots = studyClassPositionsForMediumRooms;
            return;
        }
    }
}
