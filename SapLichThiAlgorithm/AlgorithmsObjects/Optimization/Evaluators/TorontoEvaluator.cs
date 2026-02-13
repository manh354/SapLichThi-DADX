using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    public class TorontoEvaluator : IEvaluator
    {
        private Dictionary<Period, Dictionary<Student, int>> P_slot_students { get; set; } = new();
        private Dictionary<Student, Dictionary<Period, int>> P_student_slots { get; set; } = new();
        private Dictionary<ExamClass, List<Period>> P_exam_slot { get; set; } = new();
        private Dictionary<ExamClass, List<Period>> P_old_exam_slot { get; set; } = new();

        public AlgorithmContext Context { get; set; }
        public List<Student> I_students { get; set; }
        public Dictionary<ExamClass, Dictionary<Period, int>> I_examClass_validSlotsPenalties { get; set; }
        public Dictionary<ExamClass, Dictionary<Room, int>> I_examClass_validRoomsPenalties { get; set; }
        public Dictionary<Room, Dictionary<Period, int>> I_room_validSlotsPenalties { get; set; }
        public Dictionary<Period, int> I_Slot_Penalty { get; set; }

        private Lake P_lake { get; set; }

        private int W0 = 16;
        private int W1 = 8;
        private int W2 = 4;
        private int W3 = 2;
        private int W4 = 1;

        EvalDouble V_currentVal;

        public TorontoEvaluator(Lake lake)
        {
            P_lake = lake;
        }

        public EvalDouble CalculateCost(bool print = false)
        {

            EvalDouble cost = new EvalDouble(0, 0);

            // For students that have 2 or more distinct slots, compute separation penalty for every pair of slots
            foreach (var kvp in P_student_slots)
            {
                var slots = new List<Period>();
                foreach (var item in kvp.Value)
                {
                    for (int k = 0; k < item.Value; k++)
                        slots.Add(item.Key);
                }
                slots = slots.OrderBy(sl => sl.Index).ToList();
                if (slots.Count < 2)
                    continue;

                // consider every pair (i,j) with i<j
                for (int i = 0; i < slots.Count; i++)
                {
                    for (int j = i + 1; j < slots.Count; j++)
                    {
                        int between = slots[j].Index - slots[i].Index - 1; // number of slots between the two exams
                        if (between < 0)
                        {
                            cost.hardCost += 1;
                            continue;
                        }

                        switch (between)
                        {
                            case 0:
                                cost.softCost += W0;
                                break;
                            case 1:
                                cost.softCost += W1;
                                break;
                            case 2:
                                cost.softCost += W2;
                                break;
                            case 3:
                                cost.softCost += W3;
                                break;
                            case 4:
                                cost.softCost += W4;
                                break;
                            default:
                                // no penalty for separations greater than 4
                                break;
                        }
                    }
                }
            }
            // Check();
            V_currentVal = cost;
            return cost;
        }


        public IEvaluator SetContext(AlgorithmContext context)
        {
            Context = context;
            I_students = context.I_students;
            I_examClass_validSlotsPenalties = context.I_examClass_validSlotsPenalties;

            I_examClass_validSlotsPenalties = context.I_examClass_validSlotsPenalties;
            I_room_validSlotsPenalties = context.I_room_validSlotsPenalties;
            I_examClass_validRoomsPenalties = context.I_examClass_validRoomsPenalties;

            /*I_STUDENT_CONFLICT_PENALTY = context.I_STUDENT_CONFLICT_PENALTY;
            I_COURSE_SEPARATION_PENALTY = context.I_COURSE_SEPARATION_PENALTY;
            I_NON_CONSECUTIVE_SHIFT_PENALTY = context.I_NON_CONSECUTIVE_SHIFT_PENALTY;
            I_SAME_DAY_EXAM_PENALTY = context.I_SAME_DAY_EXAM_PENALTY;
            I_CONSECUTIVE_DAY_EXAM_PENALTY = context.I_CONSECUTIVE_DAY_EXAM_PENALTY;
            I_STUDENT_YEAR_PREFERENCE_PENALTY = context.I_STUDENT_YEAR_PREFERENCE_PENALTY;*/

            return this;
        }
        public IEvaluator SetContext(AlgorithmContext context, NeighborhoodContext neighborhoodContext)
        {
            SetContext(context);

            P_slot_students = neighborhoodContext.P_slot_students;
            P_student_slots = neighborhoodContext.P_student_slots;
            P_exam_slot = neighborhoodContext.P_exam_slot;
            P_old_exam_slot = neighborhoodContext.P_old_exam_slot;
            return this;
        }

        public double[] GetEval()
        {
            return new double[] { V_currentVal.hardCost, V_currentVal.softCost };
        }

        public string[] GetColumn()
        {
            return new string[] { "HC", "SC" };
        }

        public void Check()
        {
            var check_student_slots = new Dictionary<Student, Dictionary<Period, int>>();

            foreach (var pond in P_lake.Ponds)
            {
                var period = pond.Period;
                foreach (var examClass in pond.ExamClassesInPond)
                {
                    foreach (var student in examClass.Students)
                    {
                        if (!check_student_slots.ContainsKey(student))
                            check_student_slots[student] = new Dictionary<Period, int>();

                        if (!check_student_slots[student].ContainsKey(period))
                            check_student_slots[student][period] = 0;

                        check_student_slots[student][period]++;
                    }
                }
            }

            // Verify P_student_slots against check_student_slots
            foreach (var (student, slots) in P_student_slots)
            {
                // Filter out empty slots for comparison if necessary, or just compare exact content
                if (slots.Count == 0)
                {
                    if (check_student_slots.ContainsKey(student) && check_student_slots[student].Count > 0)
                        throw new Exception($"Check failed: Student {student.ID} has 0 slots in P_student_slots but {check_student_slots[student].Count} in Lake.");
                    continue;
                }

                if (!check_student_slots.TryGetValue(student, out var checkSlots))
                {
                    throw new Exception($"Check failed: Student {student.ID} has {slots.Count} slots in P_student_slots but 0 in Lake.");
                }

                foreach (var (period, count) in slots)
                {
                    if (!checkSlots.TryGetValue(period, out int checkCount))
                        throw new Exception($"Check failed: Student {student.ID} has period {period.Index} in P_student_slots but not in Lake.");

                    if (count != checkCount)
                        throw new Exception($"Check failed: Student {student.ID} period {period.Index} count mismatch. P_student_slots: {count}, Lake: {checkCount}.");
                }

                foreach (var (period, count) in checkSlots)
                {
                    if (!slots.ContainsKey(period))
                        throw new Exception($"Check failed: Student {student.ID} has period {period.Index} in Lake but not in P_student_slots.");
                }
            }

            // Reverse check for keys missing in P_student_slots
            foreach (var student in check_student_slots.Keys)
            {
                if (!P_student_slots.ContainsKey(student))
                    throw new Exception($"Check failed: Student {student.ID} exists in Lake but missing in P_student_slots.");
            }
        }

        public EvalDouble CalculateDiffCost(EvalDouble currentCost, ClassMoveInfos classMoveInfos)
        {
            EvalDouble diff = new EvalDouble(0, 0);
            HashSet<Student> affectedStudents = new HashSet<Student>();

            // Identify all affected students
            foreach (var (examClass, move) in classMoveInfos)
            {
                foreach (var student in examClass.Students)
                {
                    affectedStudents.Add(student);
                }
            }

            foreach (var student in affectedStudents)
            {
                // Calculate New Cost (current state in P_student_slots)
                if (!P_student_slots.TryGetValue(student, out var newSlots))
                {
                    // Should not happen if data is consistent, but for safety
                    continue; 
                }
                
                EvalDouble costNew = CalculateCostForStudent(newSlots);

                // Reconstruct Old Cost
                // We need a temporary dictionary representing the old state for this student
                var oldSlots = new Dictionary<Period, int>(newSlots);
                
                foreach (var (examClass, move) in classMoveInfos)
                {
                    if (!examClass.Students.Contains(student)) continue;

                    var startPeriods = move.StartPositions.Select(p => p.period).ToList();
                    var endPeriods = move.EndPositions.Select(p => p.period).ToList();

                    // Revert changes: 
                    // The move was Start -> End. 
                    // Current state is End. We want Start.
                    // So we remove End and add Start.

                    foreach (var endPeriod in endPeriods)
                    {
                        if (oldSlots.ContainsKey(endPeriod))
                        {
                            oldSlots[endPeriod]--;
                            if (oldSlots[endPeriod] <= 0) oldSlots.Remove(endPeriod);
                        }
                    }

                    foreach (var startPeriod in startPeriods)
                    {
                        if (!oldSlots.ContainsKey(startPeriod)) oldSlots[startPeriod] = 0;
                        oldSlots[startPeriod]++;
                    }
                }

                EvalDouble costOld = CalculateCostForStudent(oldSlots);

                diff += (costNew - costOld);
            }

            return currentCost + diff;
        }

        private EvalDouble CalculateCostForStudent(Dictionary<Period, int> studentSlots)
        {
            EvalDouble cost = new EvalDouble(0, 0);
            var slots = new List<int>();
            
            foreach (var kvp in studentSlots)
            {
                for (int k = 0; k < kvp.Value; k++)
                    slots.Add(kvp.Key.Index);
            }
            
            if (slots.Count < 2) return cost;
            
            slots.Sort();

            for (int i = 0; i < slots.Count; i++)
            {
                for (int j = i + 1; j < slots.Count; j++)
                {
                    int between = slots[j] - slots[i] - 1;
                    if (between < 0)
                    {
                        cost.hardCost += 1;
                        continue;
                    }

                    switch (between)
                    {
                        case 0: cost.softCost += W0; break;
                        case 1: cost.softCost += W1; break;
                        case 2: cost.softCost += W2; break;
                        case 3: cost.softCost += W3; break;
                        case 4: cost.softCost += W4; break;
                    }
                }
            }
            return cost;
        }
    }
}
