using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataType;
using SapLichThiStream.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    /// <summary>
    /// Evaluator for ITC 2007 Exam Timetabling format.
    /// Implements the ITC 2007 evaluation criteria as specified in the competition.
    /// </summary>
    public class ITC2007Evaluator : IEvaluator
    {
        // Static registry to store ITC2007ExamReader instances by input data type
        private static Dictionary<InputDataType, ITC2007ExamReader> _readerRegistry = new();

        public static void RegisterReader(InputDataType dataType, ITC2007ExamReader reader)
        {
            _readerRegistry[dataType] = reader;
        }

        public static ITC2007ExamReader GetReader(InputDataType dataType)
        {
            return _readerRegistry.GetValueOrDefault(dataType);
        }
        private Dictionary<Period, Dictionary<Student, int>> P_slot_students { get; set; } = new();
        private Dictionary<Student, Dictionary<Period, int>> P_student_slots { get; set; } = new();
        private Dictionary<ExamClass, List<Period>> P_exam_slot { get; set; } = new();
        private Dictionary<ExamClass, List<Period>> P_old_exam_slot { get; set; } = new();  
        private Dictionary<ExamClass, List<(Period period, Room room)>> P_exam_positions { get; set; } = new();

        private Dictionary<ExamClass, Dictionary<Period, int>> I_examClass_validSlotsPenalties { get; set; } = new();
        private Dictionary<ExamClass, Dictionary<Room, int>> I_examClass_validRoomsPenalties { get; set; } = new();
        private Dictionary<Room, Dictionary<Period, int>> I_room_validSlotsPenalties { get; set; } = new();
        private Dictionary<Period, int> I_slot_penalties { get; set; } = new();
        private List<BinaryConstraint> I_binaryConstraints { get; set; } = new();
        private List<UnaryConstraint> I_unaryConstraints { get; set; } = new();
        private Lake P_lake { get; set; }
        private ITC2007ExamReader P_reader { get; set; }

        // ITC 2007 weights
        private int W_TwoInARow = 0;
        private int W_TwoInADay = 0;
        private int W_PeriodSpread = 1;
        private int W_NonMixedDurations = 0;
        private int W_FrontLoadLargestExams = 5; //Number of largest exams to consider for front load penalty (First Param)
        private int W_FrontLoadThreshold = 5; //Index of the period where front load penalty starts (Second Param)
        private int W_FrontLoad = 0; // Weight for front load penalty (Third Param)
        private int W_BinaryViolation = 5000;
        private int W_DirectConflict = 1000;

        // ITC 2007 penalty counters
        private int V_TwoInARow = 0;
        private int V_TwoInADay = 0;
        private int V_WiderSpread = 0;
        private int V_MixedDurations = 0;
        private int V_FrontLoad = 0;
        private int V_RoomPenalty = 0;
        private int V_PeriodPenalty = 0;
        private int V_BinaryViolations = 0;
        private int V_DirectConflicts = 0;

        private EvalDouble V_currentVal;

        public AlgorithmContext Context { get; set; }
        public List<Student> I_students { get; set; }

        public ITC2007Evaluator(Lake lake)
        {
            P_lake = lake;
        }

        public IEvaluator SetContext(AlgorithmContext context)
        {
            Context = context;
            I_students = context.I_students;
            I_examClass_validRoomsPenalties = context.I_examClass_validRoomsPenalties;
            I_examClass_validSlotsPenalties = context.I_examClass_validSlotsPenalties;
            I_room_validSlotsPenalties = context.I_room_validSlotsPenalties;
            I_slot_penalties = context.I_slot_penalties;
            I_binaryConstraints = context.I_binaryConstraints;
            I_unaryConstraints = context.I_unaryConstraints;
            // Try to get ITC2007ExamReader from static registry
            if (P_reader == null)
            {
                P_reader = GetReader(context.I_inputDataType);
            }

            if (P_reader != null)
            {
                W_TwoInARow = P_reader.TwoInARow;
                W_TwoInADay = P_reader.TwoInADay;
                W_PeriodSpread = P_reader.PeriodSpread;
                W_NonMixedDurations = P_reader.NonMixedDurations;
                W_FrontLoad = P_reader.FrontLoad[2];
            }

            // Build exam-to-slot and exam-to-room mappings from lake
            BuildExamMappings();

            return this;
        }

        public IEvaluator SetContext(AlgorithmContext context, NeighborhoodContext neighborhoodContext)
        {
            SetContext(context);

            P_slot_students = neighborhoodContext.P_slot_students;
            P_student_slots = neighborhoodContext.P_student_slots;
            P_exam_slot = neighborhoodContext.P_exam_slot;
            P_old_exam_slot = neighborhoodContext.P_old_exam_slot;
            P_exam_positions = neighborhoodContext.P_exam_positions;
            // Rebuild exam-to-room mapping
            BuildExamMappings();

            return this;
        }

        private void BuildExamMappings()
        {

        }

        public EvalDouble CalculateCost(bool print = false)
        {
            // Reset counters
            V_TwoInARow = 0;
            V_TwoInADay = 0;
            V_WiderSpread = 0;
            V_MixedDurations = 0;
            V_FrontLoad = 0;
            V_RoomPenalty = 0;
            V_PeriodPenalty = 0;
            V_BinaryViolations = 0;
            V_DirectConflicts = 0;

            double hardCost = 0;
            double softCost = 0;

            // Including TwoInADay, TwoInARow, DirectConflicts, WiderSpread
            CalculatePeriodConstraints();
            CalculateMixedDurations();
            CalculateFrontLoad();
            CalculateRoomPenalty();
            CalculatePeriodPenalty();
            CalculateBinaryViolations();

            // Apply weights
            softCost += W_TwoInARow * V_TwoInARow;
            softCost += W_TwoInADay * V_TwoInADay;
            softCost += V_WiderSpread; // Already weighted in calculation
            softCost += W_NonMixedDurations * V_MixedDurations;
            softCost += W_FrontLoad * V_FrontLoad;
            softCost += V_RoomPenalty;
            softCost += V_PeriodPenalty;
            hardCost += W_BinaryViolation * V_BinaryViolations;
            hardCost += W_DirectConflict * V_DirectConflicts;

            V_currentVal = new EvalDouble(hardCost, softCost);
            return V_currentVal;
        }


        /// <summary>
        /// Two exams in a day: Count occurrences where students have two exams in a day that are not adjacent.
        /// </summary>
        private void CalculatePeriodConstraints()
        {
            foreach (var (student, slots) in P_student_slots)
            {
                var sortedSlots = slots.Keys.OrderBy(s => s.Date).ThenBy(s => s.Shift).ToList();
                for (int i = 0; i < sortedSlots.Count; i++)
                {
                    for (int j = i + 1; j < sortedSlots.Count; j++)
                    {
                        var sloti = sortedSlots[i];
                        var slotj = sortedSlots[j];
                        var adjacent = sloti.Date == slotj.Date && sloti.Shift + 1 == slotj.Shift;
                        var conflict = sloti.Date == slotj.Date && sloti.Shift == slotj.Shift;
                        var inPeriodSpread = (sloti.Index - slotj.Index) < W_PeriodSpread;
                        var inADay = sloti.Date == slotj.Date;
                        if (adjacent)
                        {
                            V_TwoInARow++;
                        }
                        if (conflict)
                        {
                            V_DirectConflicts++;
                        }
                        if (!adjacent && !conflict && inADay)
                        {
                            V_TwoInADay++;
                        }
                        if (!adjacent && !conflict && inPeriodSpread)
                        {
                            V_WiderSpread++;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Mixed durations: Penalty for rooms with mixed exam durations in the same period.
        /// </summary>
        private void CalculateMixedDurations()
        {
            if (W_NonMixedDurations == 0)
                return;

            foreach (var pond in P_lake.Ponds)
            {
                foreach (var puddle in pond.Puddles)
                {
                    var durations = new HashSet<int>();
                    foreach (var examClass in puddle.Elements)
                    {
                        if (durations.Contains(examClass.Duration)) continue;
                            durations.Add(examClass.Duration);
                    }
                    V_MixedDurations += Math.Max(0,durations.Count - 1);
                }
            }
        }

        private HashSet<ExamClass>? FrontLoadLargestExamClasses = null;
        /// <summary>
        /// Front load: Penalty for large exams scheduled in later periods.
        /// </summary>
        private void CalculateFrontLoad()
        {
            if (W_FrontLoad == 0 || P_reader == null)
                return;

            int threshold = P_reader.FrontLoadThreshold;
            var frontLoadParams = P_reader.FrontLoad;
            int numLargest = frontLoadParams[0];

            if (FrontLoadLargestExamClasses == null)
            {
                // Cache largest exam classes
                FrontLoadLargestExamClasses = P_exam_positions.Keys
                    .OrderByDescending(ec => ec.Count)
                    .Take(numLargest)
                    .ToHashSet();
            }

            // Find largest exams by student count
            var largestExams = FrontLoadLargestExamClasses;

            foreach (var examClass in largestExams)
            {
                if (P_exam_slot.TryGetValue(examClass, out var slot))
                {
                    P_exam_slot.TryGetValue(examClass, out var slotList);
                    var period = slotList?.FirstOrDefault();
                    if (period != null && period.Index >= threshold)
                    {
                        V_FrontLoad++;
                    }
                }
            }
        }

        /// <summary>
        /// Room penalty: Penalty for using rooms with associated penalties.
        /// </summary>
        private void CalculateRoomPenalty()
        {
            foreach (var (examClass, position) in P_exam_positions)
            {
                var room = position.First().room;
                var period = position.First().period;
                if (I_room_validSlotsPenalties.Count > 0)
                {
                    I_room_validSlotsPenalties.TryGetValue(room, out var periodPenalty);
                    if (periodPenalty != null && periodPenalty.TryGetValue(period, out int penalty))
                        V_RoomPenalty += penalty;
                }
            }
        }

        /// <summary>
        /// Period penalty: Penalty for using periods with associated penalties.
        /// </summary>
        private void CalculatePeriodPenalty()
        {
            if (I_slot_penalties.Count == 0)
                return;

            foreach (var (examClass, position) in P_exam_slot)
            {
                var period = position.FirstOrDefault();
                if (period != null && I_slot_penalties.TryGetValue(period, out var penalty))
                {
                    V_PeriodPenalty += penalty;
                }
            }
        }

        /// <summary>
        /// Binary violations: Count violations of binary constraints (AFTER, EXAM_COINCIDENCE, EXCLUSION).
        /// </summary>
        private void CalculateBinaryViolations()
        {
            if (P_reader == null)
                return;

            foreach (var constraint in P_reader.GetBinaryConstraints())
            {
                if (!constraint.IsHard)
                    continue;

                var exam1Class = constraint.ExamClass1;
                var exam2Class = constraint.ExamClass2;
                if (!P_exam_slot.TryGetValue(exam1Class, out var slots1) ||
                            !P_exam_slot.TryGetValue(exam2Class, out var slots2))
                    continue;

                var slot1 = slots1.First();
                var slot2 = slots2.First();

                bool violated = false;
                switch (constraint.ConstraintType)
                {
                    case BinaryConstraintType.AFTER:
                        // Exam1 must be after Exam2
                        if (slot1.Date < slot2.Date || slot1.Date == slot2.Date && slot1.Shift <= slot2.Shift)
                            violated = true;
                        break;
                    case BinaryConstraintType.SAME_SLOT:
                        // Must be in same period
                        if (slot1.Date != slot2.Date || slot1.Shift != slot2.Shift)
                            violated = true;
                        break;
                    case BinaryConstraintType.DIFFERENT_SLOT:
                        // Must be in different periods
                        if (slot1.Date == slot2.Date && slot1.Shift == slot2.Shift)
                            violated = true;
                        break;
                }

                if (violated)
                    V_BinaryViolations++;
            }


        }

        public double[] GetEval()
        {
            return new double[]
            {
                V_TwoInARow,
                V_TwoInADay,
                V_WiderSpread,
                V_MixedDurations,
                V_FrontLoad,
                V_RoomPenalty,
                V_PeriodPenalty,
                V_BinaryViolations,
                V_DirectConflicts,
                V_currentVal.hardCost,
                V_currentVal.softCost
            };
        }

        public string[] GetColumn()
        {
            return new string[]
            {
                "TwoInARow",
                "TwoInADay",
                "WiderSpread",
                "MixedDurations",
                "FrontLoad",
                "RoomPenalty",
                "PeriodPenalty",
                "BinaryViolations",
                "DirectConflicts",
                "HardCost",
                "SoftCost"
            };
        }

        public void Check()
        {
            // Validation check - can be implemented if needed
        }

        public EvalDouble CalculateDiffCost(EvalDouble currentCost, ClassMoveInfos classMoveInfos)
        {
            return new EvalDouble(0, 0); // not implemented
        }
    }
}
