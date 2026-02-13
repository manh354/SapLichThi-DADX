using AlgorithmExtensions;
using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.GeneralScheduling
{
    public class GeneralScheduler : BaseAlgorithmObject
    {
        public Dictionary<int, HashSet<ExamClass>> I_color_examClasses { get; set; } = new();
        public Dictionary<ExamClass, Dictionary<Period, int>> I_examClass_validSlotsPenalties { get; set; } = new();
        public Dictionary<ExamClass, Dictionary<Room, int>> I_examClass_validRoomsPenalties { get; set; } = new();
        public Dictionary<Room, Dictionary<Period, int>> I_room_validSlotsPenalties { get; set; } = new();
        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClass_Linkages { get; set; } = new();
        public Dictionary<Period, int> I_slot_penalties { get; set; } = new();
        public List<Room> I_rooms { get; set; }
        public Lake? I_lake { get; set; }
        public int MAX_ITER { get; set; }
        protected override void InitializeAllOutput()
        {
            // No output is needed
        }

        protected override void ProcedureRun()
        {
            Logger.LogMessage(
                "===============================================================" +
                "===================== ROOM FITTING.CS =========================" +
                "===============================================================");

            FitRooms();
            Logger.LogMessage(
                "###############################################################" +
                "##################### ROOM FITTING.CS #########################" +
                "###############################################################");
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_color_examClasses = context.I_color_examClasses;
            I_lake = context.I_lake;
            I_rooms = context.I_rooms;
            MAX_ITER = context.MAX_ITER;
            I_examClass_Linkages = context.I_examClass_linkages;

            I_examClass_validRoomsPenalties = context.I_examClass_validRoomsPenalties;
            I_examClass_validSlotsPenalties = context.I_examClass_validSlotsPenalties;
            I_room_validSlotsPenalties = context.I_room_validSlotsPenalties;
            I_slot_penalties = context.I_slot_penalties;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            // No output is needed, because the algorithm is working 
            // directly in the ClassRoomSea data structure.
        }

        public void FitRooms()
        {
            if (I_lake == null)
            {
                throw new Exception("No sea exception");
            }
            // sort every pairs by sum of its value
            var sortedColors =
                I_color_examClasses.OrderByDescending(cl => cl.Value.Max(x=>x.Count)).ToList();
            // Choose the largest pond
            int largestPondCount =
                I_lake.Ponds.Max(pond => pond.GetRemainingCapacity());


            bool flag = true;
            

            // Xếp các lớp theo màu trước
            sortedColors = FitRoomsByColorAndCourse(sortedColors);
            // Nếu còn màu chưa được xếp sau quá trình xếp lịch thi, ta phân rã màu đã có, và xếp theo môn học.
            // If there are colors that have not been scheduled yet, we break down the color and schedule by course.
            
            // Nếu còn màu chưa được xếp, ta nới lỏng các điều kiện phòng và xếp theo môn học.
            // If there are colors that have not been scheduled yet, we relax the room conditions.
            if (sortedColors.Count > 0)
            {
                Dictionary<int, HashSet<ExamClass>> residueForEachColor = new();
                foreach (var (color, examClasses) in sortedColors)
                {
                    Logger.LogMessage($"Vẫn còn dư màu {color} và các môn {string.Join(", ", examClasses.Select(x => x.Id))}", LogType.Warning);
                    Logger.LogMessage($"Chương trình sẽ tăng tỉ lệ phòng và xếp theo môn.", LogType.Info);
                    if (!FitRoomsByCourseRelaxed(color, examClasses, out var residueExamClasses))
                    {
                        residueForEachColor.Add(color, residueExamClasses);
                    }
                }
                sortedColors = residueForEachColor.ToList();
            }
            // Mếu còn màu chưa được xếp, ta tiếp tục bẻ các tổ hợp lớp thi trong môn học và xếp theo lớp học
            // If there are colors that have not been scheduled yet, we break down the course group and schedule by
            // study class.
            if (sortedColors.Count > 0)
            {
                Dictionary<int, HashSet<ExamClass>> residueForEachColor = new();
                foreach (var (color, examClasses) in sortedColors)
                {
                    Logger.LogMessage($"Vẫn còn dư màu {color} và các môn {string.Join(", ", examClasses.Select(x=>x.Id))}", LogType.Warning);
                    Logger.LogMessage($"Chương trình sẽ cho phép xếp trùng lặp sinh viên và tối ưu hóa sau.", LogType.Warning);
                    if (!FitRoomsWithConflicts(color, examClasses, out var residueExamClass))
                    {
                        residueForEachColor.Add(color, residueExamClass);
                    }
                }
                sortedColors = residueForEachColor.ToList();
            }
            // Nếu còn màu chưa được xếp, ta tiếp tục bẻ các tổ hợp lớp thi và xếp từng lớp thi
            // If there are colors that have not been scheduled yet, we break down the studyClass group and schedule by
            // individual exam class.
            if (sortedColors.Count > 0)
            {
                foreach (var (color, examClasses) in sortedColors)
                {
                    Logger.LogMessage($"Vẫn còn dư màu {color} và các môn {string.Join(", ", examClasses.Select(x => x.Id))}", LogType.Warning);
                    Logger.LogMessage($"Chương trình chưa có solver để giải bài toán này.", LogType.Error);
                }
            }
        }

        private List<KeyValuePair<int, HashSet<ExamClass>>> FitRoomsByColorAndCourse(List<KeyValuePair<int, HashSet<ExamClass>>> sortedColors)
        {
            int iter = 0;
            HashSet<Pond> ponds = I_lake!.Ponds.ToHashSet();


            while (++iter < MAX_ITER)
            {
                Logger.LogMessage($"Xếp các lớp tại iteration {iter}");
                List<KeyValuePair<int, HashSet<ExamClass>>> residueForEachColor = new();

                bool atLeastOneColorAdded = false;

                foreach (var (color, examClasses) in sortedColors)
                {

                    if (examClasses.Count == 0)
                        continue;
                    var sortedClasses = examClasses
                        // .ThenByDescending(x=>tinchiGroupName.Contains(x.GroupName))
                        .OrderByDescending(examClass => examClass.Count);

                    var totalStudentOfThisColor = examClasses.Sum(x => x.Count);
                    var largestExamClass = examClasses.Max(x => x.Count);

                    RuleBookExamClass ruleBookExamClass = new(
                        largestRoomFirst: true,
                        primaryRoomFirst: true,
                        examClass_ValidRoomsPenalties: I_examClass_validRoomsPenalties,
                        examClass_ValidSlotsPenalties: I_examClass_validSlotsPenalties,
                        room_ValidSlotsPenalties: I_room_validSlotsPenalties,
                        examClassLinkage: I_examClass_Linkages,
                        slot_Penalties: I_slot_penalties,
                        hardConstraint_LimitedCapacity: true,
                        hardConstraint_DifferentRoomForCourses: true,
                        hardConstraint_OnlyOneExamClassPerRoom: true,
                        hardConstraint_NoStudentConflict: true,
                        relaxedCoef: 1f,
                        examClass: null! // This will be set later
                    );
                    var ruleSetPuddleExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPuddleExamRuleSet();
                    var ruleSetPondExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPondCourseRuleSet();
                    bool pondFoundForAllPonds = true;
                    List<Pond> allPonds = I_lake.Ponds;

                    // Đoạn code thực hiện tìm kiếm kíp thi hợp lý (đi tìm Pond)
                    // The block of code that finds the suitable shifts (Pond)

                    Dictionary<Pond, int> pondPoint = allPonds.ToDictionary(p => p, p => 0);

                    foreach (var examClass in examClasses)
                    {
                        ruleBookExamClass.ExamClass = examClass;
                        pondFoundForAllPonds &= I_lake.FindBestPond(ruleBookExamClass, ruleSetPondExamClass, out var returnPonds, out var bestPond);

                        int index = returnPonds.Count;
                        foreach (var pond in returnPonds)
                        {
                            pondPoint[pond] += index;
                            index--;
                        }

                        allPonds = allPonds.FindAll(returnPonds.Contains);
                        if (!pondFoundForAllPonds)
                            break;
                    }

                    allPonds = allPonds.OrderByDescending(p => pondPoint[p]).ToList();

                    if (!pondFoundForAllPonds)
                    {
                        Logger.LogMessage($"Không tồn tại Pond hợp lý cho màu {color}");
                        residueForEachColor.Add(new KeyValuePair<int, HashSet<ExamClass>>(color, examClasses));
                        continue;
                    }

                    // Đoạn code thực hiện xếp lớp thi vào trong lịch
                    // The block of code that schedules the exam classes into the schedule
                    bool atLeastOneCourseAdded = false;
                    HashSet<ExamClass> residueExamClasses = new();
                    foreach (var examClass in examClasses)
                    {
                        bool examClassAdded = false;

                        foreach (var pond in allPonds)
                        {
                            if (ScheduleGroupOfExamClasses(examClass, pond, ruleBookExamClass, ruleSetPuddleExamClass, true, true))
                            {
                                atLeastOneCourseAdded = true;
                                examClassAdded = true;
                                break;
                            }
                        }
                        if (!examClassAdded)
                            residueExamClasses.Add(examClass);
                    }

                    atLeastOneColorAdded |= atLeastOneCourseAdded;
                    if (atLeastOneCourseAdded)
                    {
                        Logger.LogMessage($"Đã xếp ít nhất 1 môn trong màu số: {color}");
                    }
                    else
                    {
                        Logger.LogMessage($"Chưa xếp được môn nào trong màu số: {color}", LogType.Warning);
                    }

                    HashSet<ExamClass> allRemainingClass = examClasses.Where(x => residueExamClasses.Contains(x))
                        .ToHashSet();

                    if (allRemainingClass.Count > 0)
                        residueForEachColor.Add(new KeyValuePair<int, HashSet<ExamClass>>(color, allRemainingClass));
                }
                sortedColors = residueForEachColor;
                if (residueForEachColor.Count > 0 && atLeastOneColorAdded)
                    continue;
                else if (residueForEachColor.Count == 0)
                    break;
            }

            return sortedColors;
        }



        private bool FitRoomsByCourseRelaxed(int color, HashSet<ExamClass> examClassesOfColor, out HashSet<ExamClass> remainingClasses)
        {

            RuleBookExamClass ruleBookExamClass = new(
                        largestRoomFirst: true,
                        primaryRoomFirst: true,
                        examClass_ValidRoomsPenalties: I_examClass_validRoomsPenalties,
                        examClass_ValidSlotsPenalties: I_examClass_validSlotsPenalties,
                        room_ValidSlotsPenalties: I_room_validSlotsPenalties,
                        examClassLinkage: I_examClass_Linkages,
                        slot_Penalties: I_slot_penalties,
                        hardConstraint_LimitedCapacity: true,
                        hardConstraint_DifferentRoomForCourses: true,
                        hardConstraint_NoStudentConflict: true,
                        hardConstraint_OnlyOneExamClassPerRoom: true,
                        relaxedCoef: 1.0f,
                        examClass: null! // This will be set later
                    );
            var ruleSetPondExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPondCourseRuleSet();
            var ruleSetPuddleExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPuddleExamRuleSet();
            while (ruleBookExamClass.RelaxedCoef < 10.0f / CompositeSettings.MaximumPercentage)
            {
                var residueClassForColor = new HashSet<ExamClass>();
                ruleBookExamClass.RelaxedCoef += 0.1f * CompositeSettings.MaximumPercentage;
                Logger.LogMessage($"Thử nghiệm Relaxed Coef: {ruleBookExamClass.RelaxedCoef}", LogType.Info);
                var sortedExamClassesOfColor = examClassesOfColor.OrderBy(x => x.Count).ToList();

                foreach (var examClass in sortedExamClassesOfColor)
                {
                    ruleBookExamClass.ExamClass = examClass;

                    // Khối code chính để tìm các kíp thi hợp lý (đi tìm Pond)
                    var pondFound = I_lake.FindBestPond(ruleBookExamClass, ruleSetPondExamClass, out var allSuitablePonds, out var bestPond);
                    if (!pondFound)
                    {
                        Logger.LogMessage($"Không tìm thấy Pond hợp lý cho lớp {examClass.Id}");
                        residueClassForColor.Add(examClass);
                        continue;
                    }

                    // Khối code chính để xếp các lớp thi vào lịch theo phòng thi (đi tìm Puddle)
                    bool examClassAdded = false;
                    foreach (var pond in allSuitablePonds)
                    {
                        var scheduleSuccess = ScheduleGroupOfExamClasses(examClass, pond,ruleBookExamClass, ruleSetPuddleExamClass, true,true);
                        if (scheduleSuccess)
                        {
                            examClassAdded = true;
                            break;
                        }
                    }

                    // Khối code kiểm tra nếu không tìm thấy Puddle hợp lý cho môn học thì log lại
                    if (!examClassAdded)
                    {
                        Logger.LogMessage($"Không tìm thấy Puddle hợp lý cho lớp thi {examClass.Id}", LogType.Warning);
                        residueClassForColor.Add(examClass);
                        continue;
                    }
                }
                if (residueClassForColor.Count > 0)
                {
                    examClassesOfColor = residueClassForColor;
                    continue;
                }
                break;
            }
            if (examClassesOfColor.Count > 0)
            {
                remainingClasses = examClassesOfColor;
                return false;
            }
            remainingClasses = null!;
            return true;
        }

        private bool FitRoomsWithConflicts(int color, HashSet<ExamClass> examClassesOfColor, out HashSet<ExamClass> remainingClasses)
        {
            Context.HardConstraint_NoStudentConflict = false;
            
            RuleBookExamClass ruleBookExamClass = new(
                        largestRoomFirst: true,
                        primaryRoomFirst: true,
                        examClass_ValidRoomsPenalties: I_examClass_validRoomsPenalties,
                        examClass_ValidSlotsPenalties: I_examClass_validSlotsPenalties,
                        room_ValidSlotsPenalties: I_room_validSlotsPenalties,
                        examClassLinkage: I_examClass_Linkages,
                        slot_Penalties: I_slot_penalties,
                        hardConstraint_LimitedCapacity: true,
                        hardConstraint_DifferentRoomForCourses: true,
                        hardConstraint_NoStudentConflict: true,
                        hardConstraint_OnlyOneExamClassPerRoom: true,
                        relaxedCoef: 1.0f,
                        examClass: null! // This will be set later
                    );

            var ruleSetPuddleExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPuddleExamRuleSet();
            var ruleSetPondExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPondCourseRuleSet();
            var residueClassForColor = new HashSet<ExamClass>();

            foreach (var examClass in examClassesOfColor)
            {
                ruleBookExamClass.ExamClass = examClass;

                // Khối code chính để tìm các kíp thi hợp lý (đi tìm Pond)
                var pondFound = I_lake.FindBestPond(ruleBookExamClass, ruleSetPondExamClass, out var allSuitablePonds, out var bestPond);
                if (!pondFound)
                {
                    Logger.LogMessage($"Không tìm thấy Pond hợp lý cho môn {examClass.Id}");
                    residueClassForColor.Add(examClass);
                    continue;
                }

                // Khối code chính để xếp các lớp thi vào lịch theo phòng thi (đi tìm Puddle)
                bool examClassAdded = false;
                foreach (var pond in allSuitablePonds)
                {
                    var scheduleSuccess = ScheduleGroupOfExamClasses(examClass, pond, ruleBookExamClass, ruleSetPuddleExamClass, true, true);
                    if (scheduleSuccess)
                    {
                        examClassAdded = true;
                        break;
                    }
                }

                // Khối code kiểm tra nếu không tìm thấy Puddle hợp lý cho môn học thì log lại
                if (!examClassAdded)
                {
                    Logger.LogMessage($"Không tìm thấy Puddle hợp lý cho môn {examClass.Id}", LogType.Warning);
                    residueClassForColor.Add(examClass);
                    continue;
                }

            }
            if (residueClassForColor.Count > 0)
            {
                remainingClasses = residueClassForColor.ToHashSet();
                return false;
            }
            remainingClasses = null!;
            return true;

        }



        private bool ScheduleGroupOfExamClasses(
            ExamClass examClass,
            Pond chosenPond,
            RuleBookExamClass ruleBookExamClass,
            PuddleRuleSet<RuleBookExamClass> ruleSetExamClass,
            bool selectPrimaryPuddleFirst,
            bool selectLargePuddleFirst
            )
        {
            var dclonePond = chosenPond.DeepClone();

            ruleBookExamClass.ExamClass = examClass;
            var pond = dclonePond;
            if (!pond.FindBestPuddle(
                t: ruleBookExamClass,
                puddleRuleSet: ruleSetExamClass,
                currentPuddle: null,
                suitablePuddles: out var suitablePuddles,
                bestPuddle: out Puddle? bestPuddle
                ))
            {
                return false;
            }

            dclonePond.TryAddElementToPond(examClass, bestPuddle!);

            chosenPond.CopyFrom(dclonePond);
            return true;
        }

    }
}

