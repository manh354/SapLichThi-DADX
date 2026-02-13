using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using SapLichThiLib.Extensions;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace SapLichThiLib.AlgorithmsObjects.ExamGroupInserter
{
    public class ExamGroupInserter : IAlgorithmObject
    {
        //Input 
        public List<ExamGroup> I_examGroups { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> I_course_classes { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClass_examClasses { get; set; }
        public int[]? I_biasTable { get; set; }
        public int I_totalLargeRoomCapacity { get; set; }
        // Input and Output
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<(int, int), bool> I_slots_condition { get; set; }

        // Internal working mechanics
        private HRGeneral HRGeneral { get; set; }
        private HRNoOverlapBetweenCourse HRNoOverlapBetweenCourse { get; set; }
        private HRStableClasses HRStableClasses { get; set; }
        private HRNoOverlapClasses HRNoOverlapClasses { get; set; }
        private HRIgnoreFirstSolveLater HRIgnoreFirstSolveLater { get; set; }
        private HRSmallClasses HRSmallClasses { get; set; }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            if (I_biasTable == null || I_course_classes == null || I_examGroups == null || I_studyClass_examClasses == null || I_totalLargeRoomCapacity == 0
                || I_slots_condition == null || I_schedule == null)
            {
                throw new Exception(GetType().ToString() + "Not properly initialized");
            }
        }

        public void InitializeAllOutput()
        {
            return;
        }

        public void InternalInit()
        {
            HRGeneral = new HRGeneral()
            {
                I_allCourse_studyClasses = I_course_classes,
                I_schedule = I_schedule,
                I_studyClass_examClasses = I_studyClass_examClasses,
            };
            HRNoOverlapBetweenCourse = new HRNoOverlapBetweenCourse()
            {
                AllCourse_Class_Dictionary = I_course_classes,
                I_schedule = I_schedule,
                StudyClass_ExamClass_Dictionary = I_studyClass_examClasses
            };
            HRStableClasses = new HRStableClasses()
            {
                AllCourse_Class_Dictionary = I_course_classes,
                Schedule = I_schedule,
                StudyClass_ExamClass_Dictionary = I_studyClass_examClasses
            };
            HRNoOverlapClasses = new HRNoOverlapClasses()
            {
                I_courses_studyClasses = I_course_classes,
                I_schedule = I_schedule,
                I_studyClasses_examClasses = I_studyClass_examClasses
            };
            HRIgnoreFirstSolveLater = new HRIgnoreFirstSolveLater()
            {
                I_allCourses_StudyClass_Dictionary = I_course_classes,
                Schedule = I_schedule,
                StudyClass_ExamClass_Dictionary = I_studyClass_examClasses
            };
            HRSmallClasses = new HRSmallClasses()
            {
                I_courses_studyClasses = I_course_classes,
                Schedule = I_schedule,
                I_studyClasses_examClasses = I_studyClass_examClasses
            };
            FillAllSlots();
        }
        private void FillAllSlots()
        {
            for (int date = 0; date < I_schedule.dates.Length; date++)
            {
                foreach (var shift in I_schedule.shifts)
                {
                    I_slots_condition.Add((date, shift), false);
                }
            }
        }
        public void ProcedureRun()
        {
            InternalInit();
            foreach (var hardRail in I_examGroups)
            {
                IExamGroupSchemeMaker schemeMaker;
                switch (hardRail.Mode)
                {
                    case "triet":
                        schemeMaker = HRGeneral;
                        break;
                    case "vatly":
                        schemeMaker = HRStableClasses;
                        break;
                    case "ppt":
                        schemeMaker = HRNoOverlapClasses;
                        break;
                    case "ta":
                        schemeMaker = HRNoOverlapBetweenCourse;
                        break;
                    default:
                        schemeMaker = HRIgnoreFirstSolveLater;
                        break;
                }
                FitOneHardRailExamClass(hardRail, schemeMaker);
            }
        }

        private void FitOneHardRailExamClass(ExamGroup examGroup, IExamGroupSchemeMaker hardRailSchemeMaker)
        {
            if (!examGroup.Date.HasValue)
            {
                examGroup.Date = RandomExtension.ChooseRandomInt(0, I_schedule.dates.Length + 1);
                Console.WriteLine($"Nhóm môn chung \"{examGroup.Courses.First().Name}\" chưa được cung cấp ngày thi, chọn ngày {examGroup.Date}");
            }
            if (!examGroup.DefaultShift.HasValue)
            {
                examGroup.DefaultShift = RandomExtension.ChooseRandomInt(0, I_schedule.shifts.Length + 1);
                Console.WriteLine($"Nhóm môn chung \"{examGroup.Courses.First().Name}\" chưa được cung cấp kíp thi mặc định, chọn kíp thi {examGroup.DefaultShift}");

            }

            // Main loop
            List<RoomShiftScheme> filledSlots;
            List<int> emptyShifts;
            while (true)
            {
                GetAllEmptyShiftsOfThisDate(examGroup.Date.Value, out emptyShifts);
                // MakeBiasedEmptyShifts(emptyShifts, out List<int> biasedEmptyShifts);
                while (examGroup.NumShift > emptyShifts.Count)
                {
                    GetAllEmptyShiftsOfThisDate(examGroup.Date.Value, out emptyShifts);
                    Console.WriteLine($"Nhóm môn chung \"{examGroup.Courses.First().Name}\" không thể thêm được vào ngày {examGroup.Date} do thiếu kíp. Tăng số ngày lên 1.");
                    examGroup.Date += 1;
                }
                hardRailSchemeMaker.MakeScheme(examGroup, out filledSlots, out bool fit);
                Console.WriteLine($"Nhóm môn chung \"{examGroup.Courses.First().Name}\": {fit}");
                if (fit)
                    break;
                examGroup.NumShift += 1;
                if(examGroup.NumShift >=5)
                {
                    break;
                }

            }
            List<RoomShiftScheme> newSlots;
            int i = 0;
            
            while (TryCombineAllClassesInSlot(filledSlots, out newSlots))
            {
                Console.WriteLine("count {0}", i++);
                filledSlots = newSlots.ToList();
            }
            filledSlots = newSlots.ToList();
            InsertExamClassSchemeIntoScheduleKnowingDate(examGroup.Date.Value, filledSlots, emptyShifts, out var usedSlotsForThisHardRail);
            UpdateAllUsedSlots(usedSlotsForThisHardRail);

        }

        private void MakeBiasedEmptyShifts(List<int> emptyShifts, out List<int> biasedEmptyShifts)
        {
            var BiasList = I_biasTable.ToList();
            Dictionary<int, int> newBiasedForEmptyList = new();
            foreach (var unbiasedShift in emptyShifts)
            {
                int index = BiasList.IndexOf(unbiasedShift);
                newBiasedForEmptyList.Add(unbiasedShift, index);
            }
            var ordered = newBiasedForEmptyList.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            biasedEmptyShifts = ordered;
        }

        private void UpdateAllUsedSlots(List<(int, int)> usedSlot)
        {
            foreach (var item in usedSlot)
            {
                I_slots_condition[item] = true;
            }
        }
        private bool GetAllEmptyShiftsOfThisDate(int date, out List<int> emptyShifts)
        {
            emptyShifts = new();
            foreach (var shift in I_schedule.shifts)
            {
                if (!I_slots_condition[(date, shift)])
                {
                    emptyShifts.Add(shift);
                }
            }
            if (emptyShifts.Count > 0)
                return true;
            return false;
        }

        private void InsertSchemeIntoScheduleKnowingDate(int date, List<(Room, int, StudyClass)> roomShiftStudyClassPositions, List<int> emptyShifts, out List<(int, int)> slotUsed)
        {
            HashSet<(int, int)> slots = new();
            foreach (var r_s_sc in roomShiftStudyClassPositions)
            {
                I_schedule.AddToThisCell(date, emptyShifts[r_s_sc.Item2], r_s_sc.Item1, I_studyClass_examClasses[r_s_sc.Item3]);
                if (!slots.Contains((date, emptyShifts[r_s_sc.Item2])))
                    slots.Add((date, emptyShifts[r_s_sc.Item2]));
            }
            slotUsed = slots.ToList();
        }

        private void InsertExamClassSchemeIntoScheduleKnowingDate(int date, List<RoomShiftScheme> roomShiftStudyClassPositions, List<int> emptyShifts, out List<(int, int)> slotUsed)
        {
            HashSet<(int, int)> slots = new();
            foreach (var r_s_sc in roomShiftStudyClassPositions)
            {
                var shift = emptyShifts[r_s_sc.Shift];
                var room = r_s_sc.Room;
                var examClasses = r_s_sc.ExamClasses;
                I_schedule.AddToThisCell(date, shift, room, examClasses);
                if (!slots.Contains((date, shift)))
                    slots.Add((date, shift));
            }
            slotUsed = slots.ToList();
        }

        private void GetNumberOfShiftUsedForScheme(List<(Room, int, StudyClass)> roomShiftStudyClassPositions, out int numberOfShiftUsed)
        {
            List<int> shiftUsed = new();
            foreach (var item in roomShiftStudyClassPositions)
            {
                if (!shiftUsed.Contains(item.Item2))
                {
                    shiftUsed.Add(item.Item2);
                }
            }
            numberOfShiftUsed = shiftUsed.Count;
        }

        /// <summary>
        /// Ket hop cac lop cu lai voi nhau de tao thanh mot lop moi Co ti le cao hon
        /// </summary>
        /// <param name="slots"></param>
        private bool TryCombineAllClassesInSlot(IEnumerable<RoomShiftScheme> slots, out List<RoomShiftScheme> newSlots)
        {
            // Khoi tao
            newSlots = new();
            Dictionary<Course, List<RoomShiftScheme>> course_schemesList = new();
            // Khoi tao dien gia tri vao dict
            foreach (var slot in slots)
            {
                var course = slot.ExamClasses.First().StudyClass.Course;
                
                if (!course_schemesList.ContainsKey(course))
                {
                    course_schemesList.Add(course, new List<RoomShiftScheme>());
                }
                course_schemesList[course].Add(slot);
            }
            // Sap xep theo ti le phong, lua chon cac phong con co the don lop duoc, tuc la ti le
            // su dung phong khong qua 60%

            foreach (var slot in slots)
            {
                var slotCourse = slot.ExamClasses.First().StudyClass.Course;
                List<RoomShiftScheme> slotsOfCourse = course_schemesList[slotCourse];
                slotsOfCourse = slotsOfCourse.Where(x => x.GetPercentage() <= 1.0)
                    .OrderByDescending(x => x.GetPercentage()).ToList();
                course_schemesList[slotCourse] = slotsOfCourse;
            }

            int count = 0;
            // Thu ket hop cacs ti le phong
            foreach (var course_schemes in course_schemesList)
            {
                var course = course_schemes.Key;
                var schemes = course_schemes.Value;
                // Ma tran ti le khi cong 2 xep lop voi nhau vao lop index 1
                float[,] addedPercentage = new float[schemes.Count, schemes.Count];
                // Tinh toan ma tran ti le cong de tim ra cach sap xep toi uu.
                for (int i = 0; i < schemes.Count; i++)
                {
                    for (int j = 0; j < schemes.Count; j++)
                    {
                        addedPercentage[i, j] = i != j ? 
                            CombineTwoSchemesValue(schemes[i], schemes[j]): float.NegativeInfinity;
                    }
                }
                // addedPercentage.WriteArrayToFile();
                addedPercentage.ChangeArrayWithCondition(0.6f);
                for (int n = 0; n < schemes.Count; n++)
                {
                    // Ta chọn max để tìm các lớp hợp lý nhất theo giải thuật tham lam
                    // Trên ma trận tỉ lệ, các dòng, cột cần được xóa bỏ để nó ko được
                    // chọn nữa.
                    addedPercentage.GetMaxOfArrayWithCondition(
                        0.6f, out int index1, out int index2);
                    if (index1 == -1)
                        break;
                    // Sau khi chọn xong ta thay đổi hàng cột tương ứng với (i,j) và  (j,i)
                    addedPercentage.ChangeArrayAtIndex(index1, index2);
                }
                // addedPercentage.WriteArrayToFile2();
                if (addedPercentage.CheckArrayValid())
                    Console.WriteLine($"{course_schemesList.First().Key.ID} Valid");
                else Console.WriteLine("Not valid");

                // TUYET DOI KHONG DUNG HAM COPY O DAY ///
                List<RoomShiftScheme> newSchemesList = new();
                newSchemesList.AddRange(schemes);
                ///////////////////////////////////////////
                int removeCount = 0;
                int schemeSize = schemes.Count;
                for (int i = 0; i < schemeSize; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        var schemeI = schemes[i];
                        var schemeJ = schemes[j];
                        if (addedPercentage[i, j] < 0)
                            continue;
                        if (!CombineTwoSchemeIntoNewOne(schemeI, schemeJ,
                            0.6f, out var newScheme))
                            Console.WriteLine("ERROR IN ADDING 2 SCHEME TOGETHER");
                        // Đoạn này cần check lỗi rất nhiều,
                        // đảm bảo nó ko bị trùng lớp sau khi xử lý xong.
                        if (newSchemesList.Contains(schemeI) &&
                            newSchemesList.Contains(schemeJ))
                        {
                            Console.WriteLine($"Read at row {i}, col {j}");
                        }
                        else Console.WriteLine("USUFOPSSs");
                        if (!newSchemesList.Remove(schemeI))
                            throw new Exception($"{i},{j} bi loi L");
                        if (!newSchemesList.Remove(schemeJ))
                            throw new Exception($"{i},{j} bi loi R");
                        newSchemesList.Add(newScheme);
                        count++;
                    }
                }
                course_schemesList[course] = new();
                // Sua doi Dict
                course_schemesList[course].AddRange(newSchemesList);
                HashSet<string> ids = new();
                // In ra check loi, khong co gi nhieu
                /*foreach (var item in newSchemesList)
                {
                    string print = string.Empty;
                    foreach (var item2 in item.ExamClasses)
                    {
                        print += "-" + item2.ID;
                    }
                    Console.WriteLine(print);
                }*/
            }
            foreach (var item in course_schemesList)
            {
                newSlots.AddRange(item.Value);
            }
            return count > 0;
        }
        private float CombineTwoSchemesValue(
            RoomShiftScheme mainScheme,
            RoomShiftScheme addedScheme)
        {
            return (float)(mainScheme.GetSum() + addedScheme.GetSum())
                / mainScheme.Room.Capacity;
        }
        private bool CombineTwoSchemeIntoNewOne(RoomShiftScheme mainScheme, RoomShiftScheme addedScheme, float condition, out RoomShiftScheme newScheme)
        {
            var addedList = new List<ExamClass>();
            addedList.AddRange(mainScheme.ExamClasses);
            addedList.AddRange(addedScheme.ExamClasses);
            newScheme = new(mainScheme.Room, mainScheme.Shift, addedList);
            if (newScheme.GetPercentage() > condition)
                return false;
            return true;
        }
    }
}
