using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.DataStructureExtension;
using SapLichThiAlgorithm.Extensions;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove.TorontoNeighborhoodMove
{
    internal class ShiftSwapMove : INeighborhoodMove
    {
        private float I_optimalRoomCoef { get; set; } = 0.8f;
        private Lake I_lake { get; set; } = default!;
        private AlgorithmContext Context { get; set; } = default!;
        public List<Student> I_students { get; set; } = new();

        public List<ExamClass> I_examClasses { get; set; } = new();

        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClass_linkages { get; set; } = new();
        public Dictionary<ExamClass, Dictionary<Period, int>> I_examClass_validSlotsPenalties { get; set; } = new();
        public Dictionary<ExamClass, Dictionary<Room, int>> I_examClass_validRoomsPenalties { get; set; } = new();
        public Dictionary<Room, Dictionary<Period, int>> I_room_validSlotsPenalties { get; set; } = new();
        public Dictionary<Period, int> I_slot_penalties { get; set; } = new();

        private Dictionary<Period, Dictionary<Student, int>> P_slot_students = new();
        private Dictionary<Student, Dictionary<Period, int>> P_student_slots = new();
        private Dictionary<ExamClass, List<Period>> P_exam_slot = new();
        private Dictionary<ExamClass, List<Period>> P_old_exam_slot = new();
        private Dictionary<ExamClass, List<(Period period, Room room)>> P_exam_positions = new();

        private ShiftSwapDetails? P_shiftSwapDetails { get; set; } = null;
        private ClassMoveInfos? P_classMoveInfos { get; set; } = null;


        bool INeighborhoodMove.CreateNeighbor()
        {
            (P_shiftSwapDetails, P_classMoveInfos) = CreateNeighbor();
            return P_shiftSwapDetails != null;
        }

        (ShiftSwapDetails?, ClassMoveInfos?) CreateNeighbor()
        {
            // Pick random period 1
            if (I_lake.Ponds.Count < 2) return (null, null);

            var pond1 = RandomExtension.PickRandomFromList(I_lake.Ponds);
            var period1 = pond1.Period;

            // Try 10 times to find a suitable swap
            for (int i = 0; i < 10; i++)
            {
                // We pick another period to swap with
                var pond2 = RandomExtension.PickRandomFromList(I_lake.Ponds);
                if (pond1 == pond2) continue; // Same period, no swap

                var period2 = pond2.Period;

                if (IsValidPeriodSwap(pond1, pond2, out var newPositions1, out var newPositions2))
                {
                    // Found a valid swap
                    var details = new ShiftSwapDetails
                    {
                        Period1 = period1,
                        Period2 = period2,
                        ExamClasses1 = pond1.ExamClassesInPond.ToList(),
                        ExamClasses2 = pond2.ExamClassesInPond.ToList(),
                        OldPositions1 = pond1.ExamClassesInPond.Select(ec => P_exam_positions[ec].First()).ToList(),
                        OldPositions2 = pond2.ExamClassesInPond.Select(ec => P_exam_positions[ec].First()).ToList(),
                        NewPositions1 = newPositions1,
                        NewPositions2 = newPositions2
                    };

                    var infos = new ClassMoveInfos();
                    for (int k = 0; k < details.ExamClasses1.Count; k++)
                        infos.AddStartMoveInfo(details.ExamClasses1[k], new List<(Period period, Room room)>() { details.OldPositions1[k] });
                    for (int k = 0; k < details.ExamClasses2.Count; k++)
                        infos.AddStartMoveInfo(details.ExamClasses2[k], new List<(Period period, Room room)>() { details.OldPositions2[k] });

                    UpdateLake(details, false);
                    UpdateMaps(details, false);

                    for (int k = 0; k < details.ExamClasses1.Count; k++)
                        infos.AddEndMoveInfo(details.ExamClasses1[k], new List<(Period period, Room room)>() { details.NewPositions1[k] });
                    for (int k = 0; k < details.ExamClasses2.Count; k++)
                        infos.AddEndMoveInfo(details.ExamClasses2[k], new List<(Period period, Room room)>() { details.NewPositions2[k] });

                    return (details, infos);
                }
            }

            return (null, null);
        }

        private bool IsValidPeriodSwap(Pond pond1, Pond pond2, out List<(Period period, Room room)> newPositions1, out List<(Period period, Room room)> newPositions2)
        {
            newPositions1 = new List<(Period, Room)>();
            newPositions2 = new List<(Period, Room)>();


            var tempPond1 = pond1.DeepClone();
            tempPond1.RemoveEverythingFromPond(); // effectively period 1 empty

            var tempPond2 = pond2.DeepClone();
            tempPond2.RemoveEverythingFromPond(); // effectively period 2 empty

            // Try to place exams from pond1 into tempPond2
            foreach (var exam1 in pond1.ExamClassesInPond)
            {
                if (!FindValidDestinationRoomsInTempPond(exam1, tempPond2, out var bestPuddle)) return false;
                tempPond2.TryAddElementToPond(exam1, bestPuddle);
                newPositions1.Add((tempPond2.Period, bestPuddle.Room));
            }

            // Try to place exams from pond2 into tempPond1
            foreach (var exam2 in pond2.ExamClassesInPond)
            {
                if (!FindValidDestinationRoomsInTempPond(exam2, tempPond1, out var bestPuddle)) return false;
                tempPond1.TryAddElementToPond(exam2, bestPuddle);
                newPositions2.Add((tempPond1.Period, bestPuddle.Room));
            }

            return true;
        }


        private bool FindValidDestinationRoomsInTempPond(ExamClass examClass, Pond pond, out Puddle bestPuddle)
        {
            // Similar logic but using the temp pond which fills up as we go
            RuleBookExamClass ruleBookExamClass = new(
                       largestRoomFirst: true,
                       primaryRoomFirst: true,
                       examClass_ValidRoomsPenalties: I_examClass_validRoomsPenalties,
                       examClass_ValidSlotsPenalties: I_examClass_validSlotsPenalties,
                       room_ValidSlotsPenalties: I_room_validSlotsPenalties,
                       examClassLinkage: I_examClass_linkages,
                       slot_Penalties: I_slot_penalties,
                       hardConstraint_LimitedCapacity: true,
                       hardConstraint_DifferentRoomForCourses: true, // This constraint might be tricky if it checks global state vs temp state. 
                                                                     // It checks `Available` which checks `UsedCapacity`.
                       hardConstraint_OnlyOneExamClassPerRoom: true,
                       hardConstraint_NoStudentConflict: true, // This is a SLOT-level constraint. If we swap periods, student conflicts in that slot might occur with OTHER slots?
                                                               // No. Swapping two whole periods.
                                                               // Conflict WITHIN the period (Concurrent exams).
                                                               // If exam A and B are in period 1 (so no conflict), moving both to period 2 (where they still coexist) -> still no conflict.
                                                               // Conflict WITH OTHER PERIODS (e.g., student has exam in period 3).
                                                               // If we move Exam A to Period 2, we need to check if student has exam in Period 2... 
                                                               // BUT we are emptying Period 2. So no conflict with "previous occupants" of Period 2.
                                                               // We only care about Period 2 vs Period X (non-swapped).
                                                               // `hardConstraint_NoStudentConflict` usually checks if any student taking this exam is BUSY at this slot.
                                                               // P_slot_students[period2] CURRENTLY contains the students of the exams we are removing.
                                                               // So checking against `Context` or `I_lake` directly will yield false positives (conflicts with exams we are about to move out).

                       // We need to ignore conflicts with exams currently in period2 (which are moving to period1).

                       relaxedCoef: 1f,
                       examClass: examClass
                   );

            var ruleSetPuddle = ruleBookExamClass
                 .BuildDefaultPuddleExamRuleSet();

            pond.FindBestPuddle(ruleBookExamClass, ruleSetPuddle, null, out var allPossiblePuddles, out bestPuddle);
            return allPossiblePuddles.Count > 0;
        }

        ClassMoveInfos? INeighborhoodMove.GetMoveResults()
        {
            return P_classMoveInfos;
        }

        INeighborhoodMove INeighborhoodMove.IncludeContext(AlgorithmContext algorithmContext, NeighborhoodContext neighborhoodContext)
        {
            Context = algorithmContext;
            I_optimalRoomCoef = algorithmContext.I_optimalRoomCoef;
            I_lake = algorithmContext.I_lake;
            I_students = algorithmContext.I_students;
            I_examClass_linkages = algorithmContext.I_examClass_linkages;
            I_examClass_validSlotsPenalties = algorithmContext.I_examClass_validSlotsPenalties;
            I_examClass_validRoomsPenalties = algorithmContext.I_examClass_validRoomsPenalties;
            I_room_validSlotsPenalties = algorithmContext.I_room_validSlotsPenalties;
            I_slot_penalties = algorithmContext.I_slot_penalties;

            P_slot_students = neighborhoodContext.P_slot_students;
            P_student_slots = neighborhoodContext.P_student_slots;
            P_exam_slot = neighborhoodContext.P_exam_slot;
            P_old_exam_slot = neighborhoodContext.P_old_exam_slot;
            P_exam_positions = neighborhoodContext.P_exam_positions;

            I_examClasses = algorithmContext.I_allExamClasses;
            return this;
        }

        INeighborhoodMove INeighborhoodMove.IncludeIndependentLake(Lake lake)
        {
            I_lake = lake;
            return this;
        }

        void INeighborhoodMove.RevertNeighbor()
        {
            if (P_shiftSwapDetails != null)
                RevertNeighbor(P_shiftSwapDetails);
        }

        void RevertNeighbor(ShiftSwapDetails details)
        {
            UpdateLake(details, true);
            UpdateMaps(details, true);
        }

        void INeighborhoodMove.UpdateOldResource()
        {
            if (P_shiftSwapDetails != null)
            {
                foreach (var e in P_shiftSwapDetails.ExamClasses1)
                    P_old_exam_slot[e] = P_exam_slot[e];
                foreach (var e in P_shiftSwapDetails.ExamClasses2)
                    P_old_exam_slot[e] = P_exam_slot[e];
            }
        }

        void UpdateLake(ShiftSwapDetails details, bool revert)
        {
            // We can use the precalculated positions to just execute the moves
            // Instead of swapping lists one by one which might be slow, we iterate.

            var exams1 = details.ExamClasses1;
            var exams2 = details.ExamClasses2;

            // Move 1 -> 2
            if (!revert)
            {
                // Move group 1 to positions 2
                for (int i = 0; i < exams1.Count; i++)
                {
                    var exam = exams1[i];
                    var oldPos = details.OldPositions1[i];
                    var newPos = details.NewPositions1[i]; // in period 2

                    RemoveAndAddExamClass(oldPos, newPos, exam);
                }

                // Move group 2 to positions 1
                for (int i = 0; i < exams2.Count; i++)
                {
                    var exam = exams2[i];
                    var oldPos = details.OldPositions2[i];
                    var newPos = details.NewPositions2[i]; // in period 1

                    RemoveAndAddExamClass(oldPos, newPos, exam);
                }
            }
            else
            {
                // REVERT: Move group 1 BACK to positions 1
                for (int i = 0; i < exams1.Count; i++)
                {
                    var exam = exams1[i];
                    var currentPos = details.NewPositions1[i]; // currently in period 2
                    var originalPos = details.OldPositions1[i]; // back to period 1

                    RemoveAndAddExamClass(currentPos, originalPos, exam);
                }

                // REVERT: Move group 2 BACK to positions 2
                for (int i = 0; i < exams2.Count; i++)
                {
                    var exam = exams2[i];
                    var currentPos = details.NewPositions2[i]; // currently in period 1
                    var originalPos = details.OldPositions2[i]; // back to period 2

                    RemoveAndAddExamClass(currentPos, originalPos, exam);
                }
            }
        }

        private void RemoveAndAddExamClass((Period period, Room room) startPosition, (Period period, Room room) endPosition, ExamClass examClass)
        {
            var startPond = I_lake.GetPond(startPosition.period);
            var startPuddle = startPond.GetPuddle(startPosition.room);
            var endPond = I_lake.GetPond(endPosition.period);
            var endPuddle = endPond.GetPuddle(endPosition.room);

            startPond.TryRemoveElementFromPond(examClass, startPuddle);
            endPond.TryAddElementToPond(examClass, endPuddle);
        }


        void UpdateMaps(ShiftSwapDetails details, bool revert)
        {
            var exams1 = details.ExamClasses1;
            var exams2 = details.ExamClasses2;

            if (!revert)
            {
                // Update 1 -> 2
                for (int i = 0; i < exams1.Count; i++)
                {
                    UpdateSingleExamMap(exams1[i], details.OldPositions1[i], details.NewPositions1[i], removeOld: true, addNew: true);
                    P_exam_slot[exams1[i]] = [details.NewPositions1[i].period];
                    P_exam_positions[exams1[i]] = [details.NewPositions1[i]];
                }
                // Update 2 -> 1
                for (int i = 0; i < exams2.Count; i++)
                {
                    UpdateSingleExamMap(exams2[i], details.OldPositions2[i], details.NewPositions2[i], removeOld: true, addNew: true);
                    P_exam_slot[exams2[i]] = [details.NewPositions2[i].period];
                    P_exam_positions[exams2[i]] = [details.NewPositions2[i]];
                }
            }
            else
            {
                // Revert 1 <- 2
                for (int i = 0; i < exams1.Count; i++)
                {
                    UpdateSingleExamMap(exams1[i], details.NewPositions1[i], details.OldPositions1[i], removeOld: true, addNew: true);
                    P_exam_slot[exams1[i]] = [details.OldPositions1[i].period];
                    P_exam_positions[exams1[i]] = [details.OldPositions1[i]];
                }
                // Revert 2 <- 1
                for (int i = 0; i < exams2.Count; i++)
                {
                    UpdateSingleExamMap(exams2[i], details.NewPositions2[i], details.OldPositions2[i], removeOld: true, addNew: true);
                    P_exam_slot[exams2[i]] = [details.OldPositions2[i].period];
                    P_exam_positions[exams2[i]] = [details.OldPositions2[i]];
                }
            }
        }


        private void UpdateSingleExamMap(ExamClass exam, (Period period, Room room) start, (Period period, Room room) end, bool removeOld = false, bool addNew = false)
        {
            if (removeOld)
            {
                foreach (var student in exam.Students)
                {
                    P_slot_students[start.period].SubtractOrDelete(student, 1);
                    P_student_slots[student].SubtractOrDelete(start.period, 1);
                }
            }
            if (addNew)
            {
                foreach (var student in exam.Students)
                {
                    P_slot_students[end.period].AddOrCreate(student, 1);
                    P_student_slots[student].AddOrCreate(end.period, 1);
                }
            }
        }

        private class ShiftSwapDetails
        {
            public Period Period1 { get; set; }
            public Period Period2 { get; set; }
            public List<ExamClass> ExamClasses1 { get; set; }
            public List<ExamClass> ExamClasses2 { get; set; }
            public List<(Period period, Room room)> OldPositions1 { get; set; }
            public List<(Period period, Room room)> OldPositions2 { get; set; }
            public List<(Period period, Room room)> NewPositions1 { get; set; }
            public List<(Period period, Room room)> NewPositions2 { get; set; }
        }
    }
}
