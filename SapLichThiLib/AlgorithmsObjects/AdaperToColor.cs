using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class AdaperToColor : IAlgorithmObject
    {
        // inputs
        public Dictionary<string, HashSet<Course>> I_stringDateShiftRoom_course { get; set; }

        // outputs
        public Dictionary<int, HashSet<Course>> O_color_courses { get; set; }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }

        public void ProcedureRun()
        {
            int color_index = 0;
            O_color_courses = new();
            foreach (var item in I_stringDateShiftRoom_course)
            {
                O_color_courses.Add(color_index, new());
                foreach (var item2 in item.Value)
                {
                    O_color_courses[color_index].Add(item2);
                }
                color_index += 1;
            }
        }

        public void CheckAllInput()
        {
            if( I_stringDateShiftRoom_course == null)
            {
                throw new Exception(GetType().ToString() + "Not properly initialized");
            }
        }

        public void InitializeAllOutput()
        {
            O_color_courses = new();
        }
    }

    public class SearchCommonCourseInColor : IAlgorithmObject
    {
        public HashSet<Course> I_commonCourses { get; set; }
        public Dictionary<int, HashSet<Course>> I_color_courses { get; set; }
        public Dictionary<int, Course> O_color_commonCourse { get; set;}
        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }
        public void CheckAllInput()
        {
            if (I_commonCourses == null || I_color_courses == null)
                throw new Exception(GetType().ToString() + "Not properly initialized");
        }

        public void InitializeAllOutput()
        {
            O_color_commonCourse = new();
        }
        public void ProcedureRun() 
        {
            foreach (var color_courses_pair in I_color_courses)
            {
                int count = 0;
                Course commonCourseFound = null;
                foreach (var thisCourse in color_courses_pair.Value)
                {
                    if (I_commonCourses.Contains(thisCourse))
                    {
                        commonCourseFound = thisCourse;
                        count++;
                    }
                }
                if(count == 0)
                {
                    Console.WriteLine("Màu không có môn chung, ta sẽ xếp tự do.");
                    O_color_commonCourse.Add(color_courses_pair.Key, null);
                    continue;
                }
                if(count == 1)
                {
                    Console.WriteLine("Màu có duy nhất một môn chung, hợp lý để xếp vào cùng môn chung trong lịch thi.");
                    O_color_commonCourse.Add(color_courses_pair.Key, commonCourseFound);
                    continue;
                }    
                if(count > 1)
                {
                    Console.WriteLine("Có nhiều hơn 2 môn chung cùng được xếp vào màu này. Ta thấy điều này là bất hợp lý. Phải xem xét lại.");
                    O_color_commonCourse.Add(color_courses_pair.Key, commonCourseFound);
                }
            }
        }
    }

    public class GroupStudyClassInColor : IAlgorithmObject
    {
        public HashSet<Course> I_commonCourses { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_studyClasses { get; set; }
        public Dictionary<int, HashSet<Course>> I_color_courses { get;  set; }
        public Dictionary<int, HashSet<StudyClass>> O_color_studyClasses { get; set; }
        
        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }
        
        public void CheckAllInput()
        {
            if (I_commonCourses == null || I_course_studyClasses == null || I_color_courses == null)
                throw new Exception(GetType().ToString() + " Not properly implemented");
        }

        public void InitializeAllOutput()
        {
            O_color_studyClasses = new();
        }

        public void ProcedureRun()
        {
            foreach (var color_courses_pair in I_color_courses)
            {
                O_color_studyClasses.Add(color_courses_pair.Key, new());
                foreach (var course in color_courses_pair.Value)
                {
                    if (I_commonCourses.Contains(course))
                        continue;
                    O_color_studyClasses[color_courses_pair.Key] = O_color_studyClasses[color_courses_pair.Key].Concat(I_course_studyClasses[course]).ToHashSet();
                }
            }
        }
    }

    public class SchemeCreater : IAlgorithmObject
    {
        public Dictionary<int, HashSet<StudyClass>> I_color_studyClasses { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_studyClasses { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClasses_examClasses { get; set; }
        public Dictionary<Course, List<PartialEmptySlot>> I_commonCourse_partialEmptySlot { get; set; }
        public Dictionary<int, Course> I_color_commonCourse { get; set; }
        public List<EmptySlot> I_emptySlots { get; set; }
        public List<Room> I_rooms { get; set; }
        public List<ClassPosition> O_classPositions { get; set; }
        public List<KeyValuePair<int,  HashSet<StudyClass>>> O_remaining_classes_for_each_color { get; set; }
        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput() ;
            ProcedureRun();
        }
        public void CheckAllInput()
        {
            if (I_color_studyClasses == null || I_commonCourse_partialEmptySlot == null|| I_color_commonCourse == null || I_rooms == null
                || I_studyClasses_examClasses == null || I_emptySlots == null || I_course_studyClasses == null)
                throw new Exception(GetType().ToString() + " Not properly initialized.");
        }

        public void InitializeAllOutput()
        {
            O_classPositions = new();
            O_remaining_classes_for_each_color = new();
        }
        public void ProcedureRun()
        {
            List<KeyValuePair<int, HashSet<StudyClass>>> dontHaveCommon = new();
            List<KeyValuePair<int, HashSet<StudyClass>>> haveCommon = new();
            foreach (var color_classes_pair in I_color_studyClasses)
            {
                // Neu dicitonary nay tra ve khac null chung to mau ko chua mon chung
                if (I_color_commonCourse[color_classes_pair.Key] == null)
                {
                    dontHaveCommon.Add(color_classes_pair);
                }
                else
                {
                    haveCommon.Add(color_classes_pair);
                }
            }
            MakeClassPositions(dontHaveCommon, out var o_classPositions, out var o_remaining_classes_for_each_color);
            MakeClassPositionsPartialSlot(haveCommon, out var o_classPositions1, out var o_remaining_classes_for_each_color1);
            O_classPositions = o_classPositions.Concat(o_classPositions1).ToList();
            O_remaining_classes_for_each_color = o_remaining_classes_for_each_color.Concat(o_remaining_classes_for_each_color1).ToList();
        }

        // Code doan nay su dung lai tu doan truoc
        public void MakeClassPositions(IEnumerable<KeyValuePair<int, HashSet<StudyClass>>> sorted_Color_StudyClasses, out List<ClassPosition> o_classPositions, out List<KeyValuePair<int, HashSet<StudyClass>>> o_remaining_classes_for_each_color)
        {
            var emptySlotIndex = 0;
            o_classPositions = new();
            EmptySlot emptySlot;
            o_remaining_classes_for_each_color = new();
            var color_studyClasses_HashSet = I_color_studyClasses.ToHashSet();

            List<RoomShiftSlot> largeRoomSlot = MakeDescendingByCapacityLargeRoomSlotsList(I_rooms, 1);
            List<RoomShiftSlot> mediumRoomSlot = MakeDescendingByCapacityMediumRoomSlotsList(I_rooms, 1);
            List<RoomShiftSlot> smallRoomSlot = MakeDescendingByCapacitySmallRoomSlotsList(I_rooms, 1);
            List<RoomShiftSlot> allRoomSlot = largeRoomSlot.Concat(mediumRoomSlot).Concat(smallRoomSlot).ToList();

            foreach (var color_studyClasses_pair in sorted_Color_StudyClasses)
            {
                if (emptySlotIndex >= I_emptySlots.Count)
                {
                    Console.WriteLine("Thieu emptyslot de them cac mau sac vao.");
                    break;
                }
                emptySlot = I_emptySlots.ElementAt(emptySlotIndex);

                var classesOfThisColor = color_studyClasses_pair.Value;

                Console.WriteLine($"Color {color_studyClasses_pair.Key} : Count = {color_studyClasses_pair.Value.Count}");
                MakeDescendingByStudentCountStudyClasses(classesOfThisColor, out var sortedClassesOfThisColor);
                if (!TryMatchStudyClassesWithRoomSlots(sortedClassesOfThisColor, allRoomSlot, out var filledSlots, out var residueClasses, out var remainingEmptySlots))
                {
                    Console.WriteLine($"Thieu phong cho mau {color_studyClasses_pair.Key}, so lop thua  = {residueClasses.Count} STUDY");
                    o_remaining_classes_for_each_color.Add(new KeyValuePair<int, HashSet<StudyClass>>(color_studyClasses_pair.Key, residueClasses.ToHashSet()));
                    if (!TryMatchExamClassesWithRoomSlots(sortedClassesOfThisColor, remainingEmptySlots, out var filledSlots2, out var residueExamClasses))
                    {
                        Console.WriteLine($"Thieu phong cho mau {color_studyClasses_pair.Key}, so lop thua  = {residueClasses.Count} EXAM");
                        o_remaining_classes_for_each_color.Add(new KeyValuePair<int, HashSet<StudyClass>>(color_studyClasses_pair.Key, residueClasses.ToHashSet()));
                    }
                };
                
                color_studyClasses_HashSet.Remove(color_studyClasses_pair);
                emptySlotIndex++;
                foreach (var filledSlot in filledSlots)
                {
                    o_classPositions.Add(new ClassPosition() { Date = emptySlot.Date, Shift = emptySlot.Shift, Room = filledSlot.Room, ExamClasses = filledSlot.ExamClasses });
                }
            }

        }

        public bool TryMatchStudyClassesWithRoomSlots(IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<StudyClass> residueClasses, out List<RoomShiftSlot> emptySlots)
        {
            filledSlots = new List<RoomShiftScheme>();
            var hashSetOfStudyClass = studyClasses.ToHashSet();
            var hashSetOfRoomSlot = roomSlots.ToHashSet();  
            bool enoughRoomForClasses = true;
            int studyClassIndex = 0;
            StudyClass thisStudyClass;
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
                    examClassesForThisSlots.AddRange(I_studyClasses_examClasses[thisStudyClass]);
                    hashSetOfStudyClass.Remove(thisStudyClass);
                    studyClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                }
                hashSetOfRoomSlot.Remove(roomSlot);
            }
            
            if (hashSetOfStudyClass.Count > 0)
                enoughRoomForClasses = false;
            residueClasses = hashSetOfStudyClass.ToList();
            emptySlots = hashSetOfRoomSlot.ToList();
            return enoughRoomForClasses;
        }

        public bool TryMatchExamClassesWithRoomSlots(IEnumerable<StudyClass> studyClasses, IEnumerable<RoomShiftSlot> roomSlots, out List<RoomShiftScheme> filledSlots, out List<ExamClass> residueClasses)
        {
            List<ExamClass> examClasses = new();
            foreach (var item in studyClasses)
            {
                examClasses = examClasses.Concat(I_studyClasses_examClasses[item]).ToList();
            }
            examClasses = examClasses.OrderByDescending(x => x.Count).ToList();
            filledSlots = new List<RoomShiftScheme>();
            var hashSetOfExamClass = examClasses.ToHashSet();
            bool enoughRoomForClasses = true;
            int examClassIndex = 0;
            ExamClass thisExamClass;
            foreach (var roomSlot in roomSlots)
            {
                List<ExamClass> examClassesForThisSlots = new();
                if (examClassIndex >= studyClasses.Count())
                    break;
                thisExamClass = examClasses.ElementAt(examClassIndex);
                while (!(roomSlot.room.Capacity * 1f >= thisExamClass.Count))
                {
                    examClassIndex++;
                    if (examClassIndex >= studyClasses.Count())
                        break;
                    thisExamClass = examClasses.ElementAt(examClassIndex);
                    Console.WriteLine(roomSlot.room.Capacity * 1f  +" "+ thisExamClass.Count);   
                }
                if (roomSlot.room.Capacity * 1f >= thisExamClass.Count)
                {
                    examClassesForThisSlots.Add(thisExamClass);
                    hashSetOfExamClass.Remove(thisExamClass);
                    examClassIndex++;
                    filledSlots.Add(new RoomShiftScheme(roomSlot.room, roomSlot.shift, examClassesForThisSlots));
                }
            }

            if (hashSetOfExamClass.Count > 0)
                enoughRoomForClasses = false;
            residueClasses = hashSetOfExamClass.ToList();
            return enoughRoomForClasses;
        }

        public void Redivision(IEnumerable<ExamClass> examClasses)
        {

        }

        public void MakeClassPositionsPartialSlot(IEnumerable<KeyValuePair<int, HashSet<StudyClass>>> sorted_Color_StudyClasses, out List<ClassPosition> o_classPositions, out List<KeyValuePair<int, HashSet<StudyClass>>> o_remaining_classes_for_each_color)
        {
            o_remaining_classes_for_each_color = new();
            o_classPositions = new();
            foreach (var color_studyClasses_pair in sorted_Color_StudyClasses)
            {
                Console.WriteLine($"Color {color_studyClasses_pair.Key} : Count = {color_studyClasses_pair.Value.Count}");

                Course commonCourseForColor = I_color_commonCourse[color_studyClasses_pair.Key];
                PartialEmptySlot slot;
                try
                {
                    slot = I_commonCourse_partialEmptySlot[commonCourseForColor].ElementAt(0);
                }
                catch (Exception ex) { continue; }

                
                List<Room> reamainingRoomForSlot = slot.Rooms;
                // Chay thuat toan

                // Tao danh sach phong
                List<RoomShiftSlot> largeRoomSlot = MakeDescendingByCapacityLargeRoomSlotsList(reamainingRoomForSlot, 1);
                List<RoomShiftSlot> mediumRoomSlot = MakeDescendingByCapacityMediumRoomSlotsList(reamainingRoomForSlot, 1);
                List<RoomShiftSlot> smallRoomSlot = MakeDescendingByCapacitySmallRoomSlotsList(reamainingRoomForSlot, 1);
                List<RoomShiftSlot> allRoomSlot = largeRoomSlot.Concat(mediumRoomSlot).Concat(smallRoomSlot).ToList();

                var classesOfThisColor = color_studyClasses_pair.Value;
                MakeDescendingByStudentCountStudyClasses(classesOfThisColor, out var sortedClassesOfThisColor);
                if(!TryMatchStudyClassesWithRoomSlots(sortedClassesOfThisColor, allRoomSlot, out var filledSlots, out var residueClasses, out var remainingEmptySlots))
                {
                    Console.WriteLine($"Thieu phong cho mau {color_studyClasses_pair.Key}, xep vao mon {commonCourseForColor.ID}, so lop thua  = {residueClasses.Count} STUDY");
                    o_remaining_classes_for_each_color.Add(new KeyValuePair<int, HashSet<StudyClass>>(color_studyClasses_pair.Key, residueClasses.ToHashSet()));

                    if (!TryMatchExamClassesWithRoomSlots(sortedClassesOfThisColor, remainingEmptySlots, out var filledSlots2, out var residueExamClasses))
                    {
                        Console.WriteLine($"Thieu phong cho mau {color_studyClasses_pair.Key}, xep vao mon {commonCourseForColor.ID}, so lop thua  = {residueExamClasses.Count} EXAM");
                        o_remaining_classes_for_each_color.Add(new KeyValuePair<int, HashSet<StudyClass>>(color_studyClasses_pair.Key, residueClasses.ToHashSet()));
                    }
                }
                
                

                foreach (var item in filledSlots)
                {
                    o_classPositions.Add(new ClassPosition() { Date = slot.Date, Shift = slot.Shift, Room = item.Room, ExamClasses = item.ExamClasses });
                }

                // Xoa slot vua dung
                I_commonCourse_partialEmptySlot[commonCourseForColor].RemoveAt(0);
                
            }
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
        private void MakeDescendingByStudentCountStudyClasses(IEnumerable<StudyClass> studyClasses, out List<StudyClass> sortedStudyClasses)
        {
            sortedStudyClasses = studyClasses.OrderByDescending(x => x.Count).ToList();
        }

    }

    public class ScheduleInsert : IAlgorithmObject
    {
        public ExamSchedule I_schedule { get; set; }
        public List<ClassPosition> I_classPositions { get; set; }
        
        public void CheckAllInput()
        {
            if(I_schedule == null || I_classPositions == null)
                throw new Exception(GetType().ToString() + " Not properly initialized.");
        }

        public void InitializeAllOutput()
        {
            
        }

        public void ProcedureRun()
        {
            foreach (var pos in I_classPositions)
            {
                I_schedule.AddToThisCell(pos.Date, pos.Shift, pos.Room, pos.ExamClasses);
            }
        }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }
    }
}
