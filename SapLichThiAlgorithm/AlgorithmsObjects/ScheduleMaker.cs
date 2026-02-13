/*using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    // THực hiện nhiệm vụ cho tất cả các lớp vào trong một lịch.
    public class ScheduleMaker
    {
        // Inputs
        public Dictionary<int, HashSet<StudyClass>> Color_SetOfCommonClasses_Dictionary { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> CommonCourse_Class_Dictionary { get; set; }
        public Dictionary<int, HashSet<StudyClass>> Color_SetOfNonCommonClasses_Dictionary { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> StudyClass_ExamClass_Dictionary { get; set; }
        public Dictionary<School, HashSet<StudyClass>> School_StudyClass_Ditcionary { get; set; }
        public Dictionary<(int, int), Course> DateShift_CourseReserved_C_Dictionary { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> Course_Group_Dictionary { get; set; }

        public CommonClassDivider ClassDivider { get; set; }
        public List<Room> RoomList { get; set; }
        public int TotalLargeRoomCapacity { get; set; }
        public List<DateOnly> DateList { get; set; }
        public List<int> ShiftList { get; set; }
        public ExamScheduleInitializeInfos ExamScheduleIAdditionalInfos { get; set; }
        public float IncreaseThreshold { get; set; }
        // Output
        public ExamSchedule O_schedule { get; set; }


        private Room largestRoom;
        private Room FindLargestRoom()
        {
            return O_schedule.Rooms.MaxBy(x => x.Capacity);
        }


        public void MakeSchedual()
        {
            O_schedule = new ExamSchedule(DateList.ToArray(), ShiftList.ToArray(), RoomList.ToArray(), ExamScheduleIAdditionalInfos);
            FindAllLargeUnusedRoomInThisSlot(0, 0, out List<Room> rooms);
            TotalLargeRoomCapacity = rooms.Sum(x => x.Capacity);
            largestRoom = FindLargestRoom();
            DateShift_CourseReserved_C_Dictionary = new();
            FitCommonClassesToSchedule(CommonCourse_Class_Dictionary);
            //FitColorSchemeToSchedual(Color_SetOfNonCommonClasses_Dictionary, 0, Shift.shift1);
        }

        *//*private void FitColorSchemeToSchedual(Dictionary<int, HashSet<StudyClass>> thisDict, int startDate, int startShift)
        {
            if (Schedule == null)
                throw new Exception("Schedual is Null");
            Room? room;
            int date = startDate;
            int shift = startShift;
            Dictionary<int, HashSet<StudyClass>> color_residueClasses = new Dictionary<int, HashSet<StudyClass>>();
            int countResidueClasses = 0;
            int DEBUG_ALL_CLASSES = 0;
            foreach (var item in thisDict.Values)
            {
                DEBUG_ALL_CLASSES += item.Count;
            }
            int DEBUG_ALL_LOOP_CLASSES = 0;
            Console.WriteLine($"Đếm tất cả các lớp cần xếp = {DEBUG_ALL_CLASSES}");
            // Phase 1: tất cả những lớp xếp được lịch thành công và phù hợp với sức chứa
            foreach (var colorAndClasses in thisDict)
            {
                foreach (var studyClass in colorAndClasses.Value.OrderBy(x => x.Count))
                {
                    if (TraverseSchedualToFindRoom(studyClass, ref date, ref shift, out room, 1.0f))
                    {
                        DEBUG_ALL_LOOP_CLASSES++;
                        Schedule[shift, date, room] = new ArrayCell(studyClass, StudyClass_ExamClass_Dictionary[studyClass], shift, date, room);
                        continue;
                    }
                    //Console.WriteLine("FALSE");
                    List<(Shift, int, Room)> tempExamClassesPosition = new List<(Shift, int, Room)>();
                    foreach (var examClass in StudyClass_ExamClass_Dictionary[studyClass])
                    {
                        if (TraverseSchedualToFindRoom(examClass, ref date, ref shift, out room, 1.0f))
                        {
                            DEBUG_ALL_LOOP_CLASSES++;
                            Schedule[shift, date, room] = new ArrayCell(studyClass, new List<ExamClass> { examClass }, shift, date, room);
                            tempExamClassesPosition.Add((shift, date, room));
                            continue;
                        }
                        countResidueClasses++;
                        // Xoá các lớp examclass để chia lại từ đầu
                        foreach (var (_shift, _date, _room) in tempExamClassesPosition)
                        {
                            if (!Schedule.MakeCellNull(_shift, _date, _room))
                            {
                                throw new Exception("Cell is already null");
                            }
                        }
                        if (color_residueClasses.ContainsKey(colorAndClasses.Key))
                        {
                            color_residueClasses[colorAndClasses.Key].Add(studyClass);
                            break;
                        }
                        color_residueClasses.Add(colorAndClasses.Key, new HashSet<StudyClass>());
                        color_residueClasses[colorAndClasses.Key].Add(studyClass);
                    }
                }
            }

            Console.WriteLine($"all residue classes = {countResidueClasses}");
            Console.WriteLine($"Tất cả các lớp lý thuyết được loop qua : = {DEBUG_ALL_LOOP_CLASSES}");
            // Phase 2: tất cả những lớp không xếp được lịch thành công
            foreach (var color_residueClass in color_residueClasses)
            {
                foreach (var studyClass in color_residueClass.Value)
                {
                    if (TraverseSchedualToFindRoom(studyClass, ref date, ref shift, out room, IncreaseThreshold, maxShift: 8))
                    {
                        Schedule[shift, date, room] = new ArrayCell(studyClass, StudyClass_ExamClass_Dictionary[studyClass], shift, date, room);
                        continue;
                    }
                    foreach (var examClass in StudyClass_ExamClass_Dictionary[studyClass])
                    {
                        if (TraverseSchedualToFindRoom(examClass, ref date, ref shift, out room, IncreaseThreshold, ShiftList.Last()))
                        {
                            Schedule[shift, date, room] = new ArrayCell(studyClass, new List<ExamClass> { examClass }, shift, date, room);
                            continue;
                        }
                        Console.WriteLine("Lớp ko tìm được phòng!");
                    }
                }
            }
            DEBUG_ALL_CLASSES = Schedule.CountAllClasses();
            Console.WriteLine($"Đếm tất cả các lớp được xếp = {DEBUG_ALL_CLASSES}");
        }*//*

        // Hàm mới - Newfunction
        private void FitCommonClassesToSchedule(Dictionary<Course, HashSet<StudyClass>> allClassesAndCourse)
        {
            foreach (var classesOfOneCourse in allClassesAndCourse)
                FitOneCommonCourseIntoSchedule(classesOfOneCourse);

        }

        void FitOneCommonCourseIntoSchedule(KeyValuePair<Course, HashSet<StudyClass>> classesOfOneCourse)
        {
            float capacityMul;
            while (classesOfOneCourse.Value.Count > 0)
            {
                fitMode mode = ChooseBestModeToFitThisCommonCourse(classesOfOneCourse.Value);
                if (mode == fitMode.threeQuater)
                    capacityMul = 0.75f;
                else
                    capacityMul = 0.5f;
                Logger.LogMessage($"Phòng nhân {capacityMul}");

                if (!FindSlotForCommonCourses(out int foundDate, out int foundShift))
                {
                    Logger.LogMessage($"Không tìm thấy kíp trống cho môn {classesOfOneCourse.Key.Name}");
                    return;
                }
                Logger.LogMessage($"Tìm thấy ngày {foundDate} - kíp {foundShift} cho môn {classesOfOneCourse.Key.Name}");
                DateShift_CourseReserved_C_Dictionary.Add((foundDate, foundShift), classesOfOneCourse.Key);

                foreach (var studyClass in classesOfOneCourse.Value)
                {
                    Logger.LogMessage($"Lớp {studyClass.Id} có {studyClass.Count} sinh viên");

                    if (FindBestRoomToFitCommonStudyClass(foundDate, foundShift, studyClass, capacityMul, out var bestRoom))
                    {
                        O_schedule.AddToThisCell(foundDate, foundShift, bestRoom, StudyClass_ExamClass_Dictionary[studyClass]);
                        Logger.LogMessage($"Tìm thấy phòng {bestRoom.Id} sức chứa {bestRoom.Capacity} cho lớp {studyClass.Id} SLDK {studyClass.Count} môn {studyClass.Course.Name}");
                        classesOfOneCourse.Value.Remove(studyClass);
                        continue;
                    }
                    Logger.LogMessage($"Không tìm thấy phòng cho lớp {studyClass.Id} môn {studyClass.Course.Name}");
                    break;
                }
                foreach (var studyClass in classesOfOneCourse.Value)
                {
                    foreach (var examClass in StudyClass_ExamClass_Dictionary[studyClass])
                    {

                        Logger.LogMessage($"Lớp exam của {studyClass.Id} có {examClass.Count} sinh viên");
                        if (FindBestRoomToFitCommonExamClass(foundDate, foundShift, examClass, capacityMul, out var bestRoom))
                        {
                            O_schedule.AddToThisCell(foundDate, foundShift, bestRoom, examClass);
                            Logger.LogMessage($"Tìm thấy phòng {bestRoom.Id} sức chứa {bestRoom.Capacity} cho lớp {studyClass.Id} SLDK {examClass.Count} môn {studyClass.Course.Name}");
                            classesOfOneCourse.Value.Remove(studyClass);
                            continue;
                        }
                        Logger.LogMessage($"Không tìm thấy phòng cho lớp exam {studyClass.Id}");
                        break;
                    }
                }
            }
        }

        enum fitMode
        {
            half, threeQuater
        }



        private fitMode ChooseBestModeToFitThisCommonCourse(HashSet<StudyClass> classesOfOneCourse)
        {
            var sum = classesOfOneCourse.Sum(x => x.Count);
            if (classesOfOneCourse.MaxBy(x => x.Count).Count > largestRoom.Capacity * 0.5f)
                return fitMode.threeQuater;
            if (sum > TotalLargeRoomCapacity)
                return fitMode.threeQuater;
            return fitMode.half;
        }

        private bool FindSlotForCommonCourses(out int foundDate, out int foundShift)
        {
            for (int thisDate = 0; thisDate < DateList.Count; thisDate++)
            {
                for (int thisShift = 0; thisShift < ShiftList.Count; thisShift++)
                {
                    if (DateShift_CourseReserved_C_Dictionary.ContainsKey((thisDate, thisShift)))
                        continue;
                    foundDate = thisDate;
                    foundShift = thisShift;
                    return true;
                }
            }
            foundDate = -1;
            foundShift = -1;
            return false;
        }

        private bool FindBestRoomToFitCommonStudyClass(int date, int shift, StudyClass studyClass, float capacityMul, out Room? bestRoom)
        {
            if (!FindAllLargeUnusedRoomInThisSlot(date, shift, out List<Room> foundLargeRooms))
            {
                bestRoom = null;
                return false;
            }
            var acceptableRooms = foundLargeRooms.Where(x => x.Capacity * capacityMul >= studyClass.Count);
            if (acceptableRooms.Count() == 0)
            {
                bestRoom = null;
                return false;
            }
            var sortedRooms = acceptableRooms.OrderBy(x => x.Capacity / studyClass.Count);
            bestRoom = sortedRooms.Last();
            return true;
        }

        private bool FindAllLargeUnusedRoomInThisSlot(int date, int shift, out List<Room> foundRooms)
        {
            foundRooms = new List<Room>();
            var allRooms = O_schedule.Rooms;
            foreach (var room in allRooms)
            {
                if (room.RoomType == RoomType.small)
                    continue;
                if (!O_schedule.IsCellEmpty(date, shift, room))
                    continue;
                foundRooms.Add(room);
            }
            if (foundRooms.Count == 0)
                return false;
            return true;
        }

        private bool FindBestRoomToFitCommonExamClass(int date, int shift, ExamClass examClass, float capacityMul, out Room? bestRoom)
        {
            if (!FindAllRoomThatCanFitCommonExamClass(date, shift, examClass, capacityMul, out Dictionary<Room, float> foundRooms_Capacitites))
            {
                bestRoom = null;
                return false;
            }
            bestRoom = foundRooms_Capacitites.MinBy(r_c => r_c.Value).Key;
            return true;
        }

        private bool FindAllRoomThatCanFitCommonExamClass(int date, int shift, ExamClass examClass, float capacityMul, out Dictionary<Room, float> foundRooms_Capacitites)
        {
            foundRooms_Capacitites = new();
            var allRooms = O_schedule.Rooms;
            foreach (var room in allRooms)
            {
                if (room.RoomType == RoomType.small)
                    continue;
                var cell = O_schedule.GetCell(date, shift, room);
                float residueCapacity = cell.Capacity * capacityMul - cell.TotalStudent;
                if (residueCapacity > examClass.Count)
                {
                    foundRooms_Capacitites.Add(room, residueCapacity);
                }
            }
            if (foundRooms_Capacitites.Count == 0)
                return false;
            return true;
        }

        *//*private bool TryFitAllClassesIntoThisSlot();*/

        /*private bool TraverseSchedualToFindRoom(StudyClass studyClass,ref int startDate,ref int startShift, out Room? foundRoom, float increaseThreshold, int dateSeparation = 1, int maxShift = 5)
        {
            var date = startDate;
            var shift = startShift;
            Room? room;
            for (room = FindBestFitRoomForThisClassForShiftAndDate(studyClass, date, shift, increaseThreshold); room == null; room = FindBestFitRoomForThisClassForShiftAndDate(studyClass, date, shift, increaseThreshold))
            {
                (shift, date) = AdvanceShiftAndDate(shift, date, DateList.Count(),dateSeparation ,maxShift);
                if (shift == startShift && date == startDate)
                {
                    foundRoom = null;
                    return false;
                }
            }
            startDate = date;
            startShift = shift;
            foundRoom = room;
            return true;
        }
        private bool TraverseSchedualToFindRoom(ExamClass examClass,ref int startDate,ref int startShift , out Room? foundRoom, float increaseThreshold, int maxShift)
        {
            var date = startDate;
            var shift = startShift;
            Room? room;
            for(room = FindBestFitRoomForThisClassForShiftAndDate(examClass,date, shift , increaseThreshold); room == null;room= FindBestFitRoomForThisClassForShiftAndDate(examClass,date,shift, increaseThreshold))
            {
                (shift, date) = AdvanceShiftAndDate(shift, date, DateList.Count(), maxShift: maxShift);
                if ( date <= startDate && shift <= startShift)
                {
                    foundRoom = null;
                    return false;
                }
            }
            startDate = date;
            startShift = shift;
            foundRoom = room;
            return true;
        }*/
        /*private Room? FindBestFitRoomForThisClassForShiftAndDate(StudyClass studyClass, int date, int shift, float increaseThreshold)
        {
            if (Schedule == null)
                throw new Exception("Schedual is Null");
            var allRooms = Schedule.rooms;
            Room? bestRoom = null;
            for (int i = 0; i < allRooms.Length; i++)
            {
                var isThisCellOccupied = Schedule.IsThisCellOccupied(shift, date, allRooms[i]);
                if (isThisCellOccupied) continue;
                if (allRooms[i].Capacity * increaseThreshold >= studyClass.Count)
                {
                    if (bestRoom == null)
                    {
                        bestRoom = allRooms[i];
                        continue;
                    }
                    if (bestRoom.Capacity > allRooms[i].Capacity)
                    {
                        bestRoom = allRooms[i];
                        continue;
                    }
                }
            }
            return bestRoom;
        }
        private Room? FindBestFitRoomForThisClassForShiftAndDate(ExamClass examClass, int date, int shift, float increaseThreshold)
        {
            if (Schedule == null)
                throw new Exception("Schedual is Null");
            var allRooms = Schedule.rooms;
            Room? bestRoom = null;
            for (int i = 0; i < allRooms.Length; i++)
            {
                var isThisCellOccupied = Schedule.IsThisCellOccupied(shift, date, allRooms[i]);
                if (isThisCellOccupied) continue;
                if (allRooms[i].Capacity * increaseThreshold >= examClass.Count)
                {
                    if(bestRoom == null)
                    {
                        bestRoom = allRooms[i];
                        continue;
                    }    
                    if(bestRoom.Capacity > allRooms[i].Capacity)
                    {
                        bestRoom = allRooms[i];
                        continue;
                    }
                }    
            }
            return bestRoom;
        }
        private (int, int) AdvanceShiftAndDate(int thisDate, int thisShift, int dateLength,int dateSeparation,int maxShift)
        {
            thisShift++;
            if(thisShift > maxShift)
            {
                thisShift = ShiftList[0];
                thisDate += dateSeparation;
                if (thisDate >= dateLength)
                    thisDate %= dateLength;
            }
            return (thisShift, thisDate);
        }*//*
    }
}
*/