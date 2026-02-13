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
    public class HRIgnoreFirstSolveLater : IExamGroupSchemeMaker
    {
        public Dictionary<Course, HashSet<StudyClass>> I_allCourses_StudyClass_Dictionary { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> StudyClass_ExamClass_Dictionary { get; set; }
        // Input and Output
        public ExamSchedule Schedule { get; set; }
        bool TryMatchStudyClassWithRoomSlots(IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueStudyClasses, out List<ExamClass> residueOddExamClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
            residueOddExamClasses = new();
            var hashSetOfStudyClass = studyClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int studyClassIndex = 0;
            StudyClass thisStudyClass ;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                if (studyClassIndex >= studyClasses.Count())
                    break;
                thisStudyClass = studyClasses.ElementAt(studyClassIndex);
                if (roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count)
                {
                    examClassesForThisSlots.AddRange(StudyClass_ExamClass_Dictionary[thisStudyClass]);
                    hashSetOfStudyClass.Remove(thisStudyClass);
                    studyClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                    continue;
                }

                var allExamClassesOfThisStudyClasses = StudyClass_ExamClass_Dictionary[thisStudyClass];
                var acceptedExamClassesToThisSlot = new List<ExamClass>();
                int sum = 0;
                foreach (var examClass in allExamClassesOfThisStudyClasses)
                {
                    sum += examClass.Count;
                    if (roomSlot.room.Capacity * 0.6f >= sum)
                    {
                        acceptedExamClassesToThisSlot.Add(examClass);
                    }
                    else
                    {
                        residueOddExamClasses.Add(examClass);
                    }
                }
                if(acceptedExamClassesToThisSlot.Count > 0)
                {
                    examClassesForThisSlots.AddRange(acceptedExamClassesToThisSlot);
                    hashSetOfStudyClass.Remove(thisStudyClass);
                    studyClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                    continue;
                }
            }
            if (hashSetOfStudyClass.Count > 0 || residueOddExamClasses.Count > 0)
                enoughRoomForClasses = false;
            residueStudyClasses = hashSetOfStudyClass.ToList();
            return enoughRoomForClasses;
        }
        bool TryMatchRemainingStudyClasses(IEnumerable<StudyClass> remainingStudyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<RoomShiftSlot> remainingRoomSlots, out List<StudyClass> residueStudyClasses)
        {
            filledSlots = new();
            var hashSetOfStudyClass = remainingStudyClasses.ToHashSet();
            var hashSetOfRoomSlots = roomSlots.ToHashSet();
            int studyClassIndex = 0;
            StudyClass thisStudyClass ;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                BEGINNING:
                if (studyClassIndex >= remainingStudyClasses.Count())
                    break;
                thisStudyClass = remainingStudyClasses.ElementAt(studyClassIndex);
                if (roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count)
                {
                    examClassesForThisSlots.AddRange(StudyClass_ExamClass_Dictionary[thisStudyClass]);
                    hashSetOfStudyClass.Remove(thisStudyClass);
                    studyClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                    hashSetOfRoomSlots.Remove(roomSlot);
                    continue;
                }
                studyClassIndex++;
                goto BEGINNING;
            }
            remainingRoomSlots = hashSetOfRoomSlots.ToList();
            if(hashSetOfStudyClass.Count > 0)
            {
                residueStudyClasses = hashSetOfStudyClass.ToList();
                return false;
            }
            residueStudyClasses = new();
            return true;

        }

        bool TryMatchRemainingOddExamClasses(IEnumerable<ExamClass> remainingOddExamClasses, IEnumerable <RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueExamClasses)
        {
            filledSlots = new();
            residueExamClasses = new();
            var hashSetOfExamClass = remainingOddExamClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int examClassIndex = 0;
            ExamClass thisExamClass;
            if (remainingOddExamClasses.Count() > roomSlots.Count())
                enoughRoomForClasses = false;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                if (examClassIndex >= remainingOddExamClasses.Count())
                    break;
                thisExamClass = remainingOddExamClasses.ElementAt(examClassIndex);
                if (roomSlot.room.Capacity * 0.6f >= thisExamClass.Count)
                {
                    examClassesForThisSlots.Add(thisExamClass);
                    hashSetOfExamClass.Remove(thisExamClass);
                    examClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                    continue;
                }
                residueExamClasses.Add(thisExamClass);
            }
            if(residueExamClasses.Count > 0)
            {
                enoughRoomForClasses = false;
            }
            return enoughRoomForClasses;
        }
        void ProcessDescription(IEnumerable<StudyClass> studyClasses, out Dictionary<StudyClass, string> processedString, out Dictionary<string, HashSet<StudyClass>> allSimilarDescriptionClass)
        {
            processedString = new();
            allSimilarDescriptionClass = new();
            foreach (var studyClass in studyClasses)
            {
                string desc = studyClass.Description;
                desc = desc.Trim().ToLowerInvariant();
                StringBuilder newStr = new StringBuilder();
                foreach (char c in desc)
                {
                    if(char.IsLetter(c))
                    {
                        newStr.Append(c);
                    }
                    else
                    {
                        if(char.IsDigit(c))
                            break;
                    }
                }
                processedString.Add(studyClass,newStr.ToString());
            }
        }

        private void ConcatAllCourses(IEnumerable<Course> courses, out List<ExamClass> examClasses, out List<StudyClass> studyClasses)
        {
            examClasses = new();
            studyClasses = new();
            foreach (Course course in courses)
            {
                studyClasses = studyClasses.Concat(I_allCourses_StudyClass_Dictionary[course]).ToList();
                foreach (var studyClass in I_allCourses_StudyClass_Dictionary[course])
                {
                    examClasses = examClasses.Concat(StudyClass_ExamClass_Dictionary[studyClass]).ToList();
                }
            }
        }

        private List<StudyClass> MakeDescendingByCapacityStudyClassList(IEnumerable<StudyClass> studyClasses)
        {
            return studyClasses.OrderByDescending(x => x.Count).ToList();
        }

        private List<RoomShiftSlot> MakeDescendingByCapacityLargeRoomSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShifts = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShifts = roomShifts.Concat(rooms.Where(x => x.RoomType == RoomType.large).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShifts = roomShifts.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShifts;
        }

        private List<RoomShiftSlot> MakeDescendingByCapacityMediumRoomSlotsList(IEnumerable<Room> rooms, int totalShift)
        {
            List<RoomShiftSlot> roomShifts = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShifts = roomShifts.Concat(rooms.Where(x => x.RoomType == RoomType.medium).Select(s => new RoomShiftSlot(s, currentShift))).ToList();
            }
            roomShifts = roomShifts.OrderByDescending(x => x.room.Capacity).ToList();
            return roomShifts;
        }
        private List<List<RoomShiftSlot>> MakeAllRoomSlotGroupingByShift(IEnumerable<Room> rooms, int totalShift)
        {
            List<List<RoomShiftSlot>> roomSlotsGroupedByShift = new List<List<RoomShiftSlot>>();
            List<RoomShiftSlot> roomShifts = new List<RoomShiftSlot>();
            for (int currentShift = 0; currentShift < totalShift; currentShift++)
            {
                roomShifts = rooms.Where(x => x.RoomType == RoomType.large || x.RoomType == RoomType.medium).Select(s => new RoomShiftSlot(s, currentShift)).ToList();
                roomSlotsGroupedByShift.Add(roomShifts);
            }
            return roomSlotsGroupedByShift;
        }

        private void MakeDescendingByCapacityAllTypeSlotsList(IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftSlot> sortedLargeRoomSlots, out List<RoomShiftSlot> sortedMediumRoomSlots)
        {
            sortedLargeRoomSlots = new();
            sortedMediumRoomSlots = new();
            foreach (var roomShiftSlot in roomSlots)
            {
                if (roomShiftSlot.room.RoomType == RoomType.large)
                {
                    sortedLargeRoomSlots.Add(roomShiftSlot);
                    continue;
                }
                if(roomShiftSlot.room.RoomType == RoomType.medium)
                {
                    sortedMediumRoomSlots.Add(roomShiftSlot);
                    continue;
                }
            }
            sortedLargeRoomSlots = sortedLargeRoomSlots.OrderByDescending(cls => cls.room.Capacity).ToList();
            sortedMediumRoomSlots = sortedMediumRoomSlots.OrderByDescending(cls => cls.room.Capacity).ToList();
        }

        private List<List<StudyClass>> MakeListOfEqualSizeListOfStudyClasses(IEnumerable<StudyClass> studyClasses, int numShift)
        {
            List<List<StudyClass>> result = new();
            for (int i = 0; i < numShift; i++)
            {
                result.Add(new());
            }
            int index = 0;
            foreach (var item in studyClasses)
            {
                result[index % numShift].Add(item);
                index++;
            }
            return result;
        }

        public void MakeScheme(ExamGroup hardRail, out List<RoomShiftScheme> filledSlots, out bool fit)
        {
            fit = true;
            filledSlots = new List<RoomShiftScheme>();
            ConcatAllCourses(hardRail.Courses, out var examClasses, out var studyClasses);
            var roomSlotsGroupedByShift = MakeAllRoomSlotGroupingByShift(Schedule.rooms, hardRail.NumShift);
            var sortedExamClasses = MakeDescendingByCapacityStudyClassList(studyClasses);
            var sortedSubListsExamClasses = MakeListOfEqualSizeListOfStudyClasses(sortedExamClasses, hardRail.NumShift);
            Console.WriteLine(studyClasses.Count);
            int index = 0;
            foreach (var slot in roomSlotsGroupedByShift)
            {
                MakeDescendingByCapacityAllTypeSlotsList(slot, out var sortedLargeRoomSlots, out var sortedMediumRoomSlots);
                var sortedExamClassForThisSlot = sortedSubListsExamClasses[index];
                if (!TryMatchStudyClassWithRoomSlots(
                    sortedExamClassForThisSlot,
                    sortedLargeRoomSlots,
                    out var filledSlotsForLargeRooms,
                    out var residueStudyClasses,
                    out var residueOddExamClasses))
                {
                    Logger.logger.LogMessage($"HR_IGNORE_FIRST_SOLVE_LATER: Không đủ phòng lớn cho {String.Join('_', hardRail.Courses.Select(x => x.ID))} , dư {residueStudyClasses.Count} StudyClass , {residueOddExamClasses.Count} ExamClass, Thử sang phòng vừa.");
                }
                else
                {
                    filledSlots = filledSlotsForLargeRooms;
                    break;
                }
                if (!TryMatchRemainingStudyClasses(
                    residueStudyClasses,
                    sortedMediumRoomSlots,
                    out var filledSlotsForStudyClassesForMediumRooms,
                    out var remainingRoomSlots,
                    out var residueStudyClassesForMediumSlots))
                {
                    Logger.logger.LogMessage($"HR_IGNORE_FIRST_SOLVE_LATER: Không đủ phòng vừa cho {String.Join('_', hardRail.Courses.Select(x => x.ID))} studyClass thừa {residueStudyClassesForMediumSlots.Count}. Chuyển sang examClass để xếp.");
                }
                if (residueStudyClassesForMediumSlots.Count > 0)
                {
                    residueStudyClassesForMediumSlots.ForEach(x => residueOddExamClasses.AddRange(StudyClass_ExamClass_Dictionary[x]));
                }
                if (!TryMatchRemainingOddExamClasses(
                    residueOddExamClasses,
                    remainingRoomSlots,
                    out var filledSlotsForExamClassesForMediumRooms,
                    out var residueExamClasses))
                {
                    Logger.logger.LogMessage($"HR_IGNORE_FIRST_SOLVE_LATER: Không đủ phòng vừa cho các lớp examClass thừa.");
                    fit = false;
                }

                filledSlots = filledSlots.Concat(filledSlotsForLargeRooms).Concat(filledSlotsForExamClassesForMediumRooms).Concat(filledSlotsForStudyClassesForMediumRooms).ToList();
                index++;
            }
            
        }
        

    }
}
