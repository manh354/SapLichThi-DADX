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
    internal class KempeChainMove : INeighborhoodMove
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

        private KempeChainDetails? P_kempeChainDetails { get; set; } = null;
        private ClassMoveInfos? P_classMoveInfos { get; set; } = null;


        bool INeighborhoodMove.CreateNeighbor()
        {
            (P_kempeChainDetails, P_classMoveInfos) = CreateNeighbor();
            return P_kempeChainDetails != null;
        }

        (KempeChainDetails?, ClassMoveInfos?) CreateNeighbor()
        {
            if (I_examClasses.Count == 0) return (null, null);

            var startExam = RandomExtension.PickRandomFromList(I_examClasses);
            var period1 = P_exam_slot[startExam].First();

            // Pick a destination period (period2) that conflicts with starting exam or just random?
            // Kempe chain typically swaps color assignment between two colors (periods).
            var period2 = RandomExtension.PickRandomFromList(I_lake.Ponds).Period;
            if (period1 == period2) return (null, null);

            var kempeChain1 = new HashSet<ExamClass>(); // Exams currently in period 1 moving to period 2
            var kempeChain2 = new HashSet<ExamClass>(); // Exams currently in period 2 moving to period 1

            var queue = new Queue<ExamClass>();
            queue.Enqueue(startExam);
            kempeChain1.Add(startExam);

            var pond1 = I_lake.GetPond(period1);
            var pond2 = I_lake.GetPond(period2);

            while (queue.Count > 0)
            {
                var currentExam = queue.Dequeue();
                var currentPeriod = P_exam_slot[currentExam].First();
                var targetPeriod = currentPeriod == period1 ? period2 : period1;

                // Find exams in the TARGET period that conflict with `currentExam`
                // Conflict means sharing students.

                // Optimally: Use precomputed linkages.
                if (I_examClass_linkages.TryGetValue(currentExam, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        var neighborPeriod = P_exam_slot[neighbor].First();

                        // If neighbor is in the target period, it MUST move to avoid conflict
                        if (neighborPeriod == targetPeriod)
                        {
                            bool alreadyInChain = (targetPeriod == period1) ? kempeChain1.Contains(neighbor) : kempeChain2.Contains(neighbor);

                            if (!alreadyInChain)
                            {
                                if (targetPeriod == period1) kempeChain1.Add(neighbor);
                                else kempeChain2.Add(neighbor);

                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            // Now we have two sets of exams to swap.
            // We must verify if they can fit into their new periods (Room constraints).
            // kempeChain1 -> period2
            // kempeChain2 -> period1 

            var newPositions1 = new List<(Period, Room)>();
            var newPositions2 = new List<(Period, Room)>();

            // Check feasiblity for chain1 -> period2
            var tempPond2 = pond2.DeepClone();
            // Remove exams that are leaving (chain2)
            foreach (var exam in kempeChain2) tempPond2.RemoveExamClassFromPond(exam);

            foreach (var exam in kempeChain1)
            {
                if (!FindValidDestinationRoomsInTempPond(exam, tempPond2, out var bestPuddle)) return (null, null);
                tempPond2.TryAddElementToPond(exam, bestPuddle);
                newPositions1.Add((period2, bestPuddle.Room));
            }

            // Check feasibility for chain2 -> period1
            var tempPond1 = pond1.DeepClone();
            // Remove exams that are leaving (chain1)
            foreach (var exam in kempeChain1) tempPond1.RemoveExamClassFromPond(exam);

            foreach (var exam in kempeChain2)
            {
                if (!FindValidDestinationRoomsInTempPond(exam, tempPond1, out var bestPuddle)) return (null, null);
                tempPond1.TryAddElementToPond(exam, bestPuddle);
                newPositions2.Add((period1, bestPuddle.Room));
            }

            // All feasible.
            var details = new KempeChainDetails
            {
                Period1 = period1,
                Period2 = period2,
                Chain1 = kempeChain1.ToList(),
                Chain2 = kempeChain2.ToList(),
                OldPositions1 = kempeChain1.Select(e => P_exam_positions[e].First()).ToList(),
                OldPositions2 = kempeChain2.Select(e => P_exam_positions[e].First()).ToList(),
                NewPositions1 = newPositions1,
                NewPositions2 = newPositions2
            };

            var infos = new ClassMoveInfos();
            for (int k = 0; k < details.Chain1.Count; k++)
                infos.AddStartMoveInfo(details.Chain1[k], new List<(Period period, Room room)>() { details.OldPositions1[k] });
            for (int k = 0; k < details.Chain2.Count; k++)
                infos.AddStartMoveInfo(details.Chain2[k], new List<(Period period, Room room)>() { details.OldPositions2[k] });

            UpdateLake(details, false);
            UpdateMaps(details, false);

            for (int k = 0; k < details.Chain1.Count; k++)
                infos.AddEndMoveInfo(details.Chain1[k], new List<(Period period, Room room)>() { details.NewPositions1[k] });
            for (int k = 0; k < details.Chain2.Count; k++)
                infos.AddEndMoveInfo(details.Chain2[k], new List<(Period period, Room room)>() { details.NewPositions2[k] });

            return (details, infos);
        }

        private bool FindValidDestinationRoomsInTempPond(ExamClass examClass, Pond pond, out Puddle bestPuddle)
        {
            RuleBookExamClass ruleBookExamClass = new(
                       largestRoomFirst: true,
                       primaryRoomFirst: true,
                       examClass_ValidRoomsPenalties: I_examClass_validRoomsPenalties,
                       examClass_ValidSlotsPenalties: I_examClass_validSlotsPenalties,
                       room_ValidSlotsPenalties: I_room_validSlotsPenalties,
                       examClassLinkage: I_examClass_linkages,
                       slot_Penalties: I_slot_penalties,
                       hardConstraint_LimitedCapacity: true,
                       hardConstraint_DifferentRoomForCourses: true,
                       hardConstraint_OnlyOneExamClassPerRoom: true,
                       hardConstraint_NoStudentConflict: true,
                       relaxedCoef: 1f,
                       examClass: examClass
                   );

            var ruleSetPuddle = ruleBookExamClass.BuildDefaultPuddleExamRuleSet();

            ruleSetPuddle.AddNewContition((puddle, rb) =>
            {
                return puddle.GetRemainingCapacity() >= Puddle.GetElementSize(rb.ExamClass);
            });
            ruleSetPuddle.AddNewContition((puddle, rb) =>
            {
                if (rb.ExamClass_ValidRoomsPenalties.TryGetValue(rb.ExamClass, out var validRooms))
                {
                    return validRooms.ContainsKey(puddle.Room);
                }
                return true;
            });

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
            if (P_kempeChainDetails != null)
                RevertNeighbor(P_kempeChainDetails);
        }

        void RevertNeighbor(KempeChainDetails details)
        {
            UpdateLake(details, true);
            UpdateMaps(details, true);
        }

        void INeighborhoodMove.UpdateOldResource()
        {
            if (P_kempeChainDetails != null)
            {
                foreach (var e in P_kempeChainDetails.Chain1) P_old_exam_slot[e] = P_exam_slot[e];
                foreach (var e in P_kempeChainDetails.Chain2) P_old_exam_slot[e] = P_exam_slot[e];
            }
        }

        void UpdateLake(KempeChainDetails details, bool revert)
        {
            var chain1 = details.Chain1;
            var chain2 = details.Chain2;

            if (!revert)
            {
                // Move chain1 -> period 2
                for (int i = 0; i < chain1.Count; i++)
                {
                    var exam = chain1[i];
                    var oldPos = details.OldPositions1[i];
                    var newPos = details.NewPositions1[i];
                    RemoveAndAddExamClass(oldPos, newPos, exam);
                }
                // Move chain2 -> period 1
                for (int i = 0; i < chain2.Count; i++)
                {
                    var exam = chain2[i];
                    var oldPos = details.OldPositions2[i];
                    var newPos = details.NewPositions2[i];
                    RemoveAndAddExamClass(oldPos, newPos, exam);
                }
            }
            else
            {
                // Revert chain1 <- period 2
                for (int i = 0; i < chain1.Count; i++)
                {
                    var exam = chain1[i];
                    var currentPos = details.NewPositions1[i];
                    var originalPos = details.OldPositions1[i];
                    RemoveAndAddExamClass(currentPos, originalPos, exam);
                }
                // Revert chain2 <- period 1
                for (int i = 0; i < chain2.Count; i++)
                {
                    var exam = chain2[i];
                    var currentPos = details.NewPositions2[i];
                    var originalPos = details.OldPositions2[i];
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

        void UpdateMaps(KempeChainDetails details, bool revert)
        {
            var chain1 = details.Chain1;
            var chain2 = details.Chain2;

            if (!revert)
            {
                for (int i = 0; i < chain1.Count; i++)
                {
                    UpdateSingleExamMap(chain1[i], details.OldPositions1[i], details.NewPositions1[i], removeOld: true, addNew: true);
                    P_exam_slot[chain1[i]] = [details.NewPositions1[i].period];
                    P_exam_positions[chain1[i]] = [details.NewPositions1[i]];
                }
                for (int i = 0; i < chain2.Count; i++)
                {
                    UpdateSingleExamMap(chain2[i], details.OldPositions2[i], details.NewPositions2[i], removeOld: true, addNew: true);
                    P_exam_slot[chain2[i]] = [details.NewPositions2[i].period];
                    P_exam_positions[chain2[i]] = [details.NewPositions2[i]];
                }
            }
            else
            {
                for (int i = 0; i < chain1.Count; i++)
                {
                    UpdateSingleExamMap(chain1[i], details.NewPositions1[i], details.OldPositions1[i], removeOld: true, addNew: true);
                    P_exam_slot[chain1[i]] = [details.OldPositions1[i].period];
                    P_exam_positions[chain1[i]] = [details.OldPositions1[i]];
                }
                for (int i = 0; i < chain2.Count; i++)
                {
                    UpdateSingleExamMap(chain2[i], details.NewPositions2[i], details.OldPositions2[i], removeOld: true, addNew: true);
                    P_exam_slot[chain2[i]] = [details.OldPositions2[i].period];
                    P_exam_positions[chain2[i]] = [details.OldPositions2[i]];
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

        private class KempeChainDetails
        {
            public Period Period1 { get; set; }
            public Period Period2 { get; set; }
            public List<ExamClass> Chain1 { get; set; } // From Period 1 to Period 2
            public List<ExamClass> Chain2 { get; set; } // From Period 2 to Period 1
            public List<(Period period, Room room)> OldPositions1 { get; set; }
            public List<(Period period, Room room)> OldPositions2 { get; set; }
            public List<(Period period, Room room)> NewPositions1 { get; set; }
            public List<(Period period, Room room)> NewPositions2 { get; set; }
        }
    }
}
