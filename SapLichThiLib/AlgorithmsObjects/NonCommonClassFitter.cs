using SapLichThiLib.AlgorithmsObjects.ExamGroupInserter;
using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public struct ClassPosition
    {
        public int Date { get; set; }
        public int Shift { get; set; }
        public Room Room { get; set; }
        public List<ExamClass> ExamClasses { get; set; }
    }
    public class NonCommonClassFitter
    {
        public List<PartialEmptySlot> PartialEmptySlots { get; set; }
        public List<EmptySlot> EmptySlots { get; set; }
        public Dictionary<int, HashSet<StudyClass>> Color_StudyClasses_Pairs { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> StudyClass_ExamClass_Dictionary { get; set; }
        public ExamSchedule Schedule { get; set; }
        public List<ClassPosition> ClassPositions { get; set; }
        // Output
        public List<KeyValuePair<int,HashSet<StudyClass>>> O_RemainingClasses { get; set; }
        
        public void InsertIntoSchedule()
        {
            Console.WriteLine($"SO luong mau to do thi = {Color_StudyClasses_Pairs.Count}");
            SortClasses(Color_StudyClasses_Pairs, out var sorted_Color_StudyClasses);
            
            // Pass 1 : All Empty Full slot is used to fill in all the classes.
            MakeClassPositions(sorted_Color_StudyClasses,out var classPositions, out var remaining_classes_for_each_color);
            ClassPositions = classPositions;
            InsertClassPositionsIntoSchedule();
            // Pass 2 : All Empty Partial slot is used to fill all remaining classes.
            O_RemainingClasses = remaining_classes_for_each_color;
        }

        public void MakeClassPositions(IEnumerable<KeyValuePair<int,HashSet<StudyClass>>> sorted_Color_StudyClasses,out List<ClassPosition> o_classPositions, out List<KeyValuePair<int, HashSet<StudyClass>>> o_remaining_classes_for_each_color)
        {
            var emptySlotIndex = 0;
            var emptySlot = EmptySlots.ElementAt(emptySlotIndex);
            o_classPositions = new();
            o_remaining_classes_for_each_color = new();
            var color_studyClasses_HashSet = Color_StudyClasses_Pairs.ToHashSet();

            var largeRoomSlot = MakeDescendingByCapacityLargeRoomSlotsList(Schedule.rooms, 1);
            var mediumRoomSlot = MakeDescendingByCapacityMediumRoomSlotsList(Schedule.rooms, 1);
            var smallRoomSlot = MakeDescendingByCapacitySmallRoomSlotsList(Schedule.rooms, 1);
            var large_and_mediumRooms = largeRoomSlot.Concat(mediumRoomSlot).Concat(smallRoomSlot).ToList();
            
            foreach (var color_studyClasses_pair in sorted_Color_StudyClasses)
            {
                if (emptySlotIndex >= EmptySlots.Count)
                {
                    Console.WriteLine("Thieu emptyslot de them cac mau sac vao.");
                    break;
                }
                emptySlot = EmptySlots.ElementAt(emptySlotIndex);

                var classesOfThisColor = color_studyClasses_pair.Value;

                Console.WriteLine($"Color {color_studyClasses_pair.Key} : Count = {color_studyClasses_pair.Value.Count}");
                MakeDescendingByStudentCountStudyClasses(classesOfThisColor, out var sortedClassesOfThisColor);
                if (!TryMatchStudyClassesWithRoomSlots(sortedClassesOfThisColor, large_and_mediumRooms, out var filledSlots, out var residueClasses))
                {
                    Console.WriteLine($"Thieu phong cho mau {color_studyClasses_pair.Key}, so lop thua  = {residueClasses.Count}");
                    o_remaining_classes_for_each_color.Add(new KeyValuePair<int, HashSet<StudyClass>>(color_studyClasses_pair.Key, residueClasses.ToHashSet()));
                };
                color_studyClasses_HashSet.Remove(color_studyClasses_pair);
                emptySlotIndex++;
                foreach (var item in filledSlots)
                {
                    o_classPositions.Add(new ClassPosition() { Date = emptySlot.Date, Shift = emptySlot.Shift, Room = item.Room, ExamClasses = item.ExamClasses });
                }
            }

        }

        public void MakeClassPosition(IEnumerable<StudyClass> remainingClasses, out List<ClassPosition> o_classPositions)
        {
            throw new NotImplementedException();
        }
        public void InsertClassPositionsIntoSchedule()
        {
            foreach (var item in ClassPositions)
            {
                Schedule.AddToThisCell(item.Date, item.Shift, item.Room, item.ExamClasses);
            }
        }
        public bool TryMatchStudyClassesWithRoomSlots(IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClasses)
        {
            filledSlots = new List<RoomShiftScheme>();
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
                while (!(roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count))
                {
                    studyClassIndex++;
                    if (studyClassIndex >= studyClasses.Count())
                        break;
                    thisStudyClass = studyClasses.ElementAt(studyClassIndex);
                }
                if (roomSlot.room.Capacity * 0.6f >= thisStudyClass.Count)
                {
                    examClassesForThisSlots.AddRange(StudyClass_ExamClass_Dictionary[thisStudyClass]);
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

        public void SortClasses(IEnumerable<KeyValuePair<int, HashSet<StudyClass>>> Color_StudyClasses_Pairs, out List<KeyValuePair<int, HashSet<StudyClass>>> Sorted_Color_StudyClasses)
        {
            Sorted_Color_StudyClasses = Color_StudyClasses_Pairs.
                OrderByDescending(x => x.Value.MaxBy(y => y.Count).Count).
                OrderByDescending(x => x.Value.Count).ToList();
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
        private void MakeDescendingByStudentCountStudyClasses (IEnumerable<StudyClass> studyClasses, out List<StudyClass> sortedStudyClasses)
        {
            sortedStudyClasses = studyClasses.OrderByDescending(x => x.Count).ToList();
        }
    }
}
