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
    public class RoomPeriodSwapMove : INeighborhoodMove
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

        private RoomPeriodSwapDetails? P_moveDetails { get; set; } = null;
        private ClassMoveInfos? P_classMoveInfos { get; set; } = null;

        bool INeighborhoodMove.CreateNeighbor()
        {
            (P_moveDetails, P_classMoveInfos) = CreateNeighbor();
            return P_moveDetails != null;
        }

        (RoomPeriodSwapDetails?, ClassMoveInfos?) CreateNeighbor()
        {
            if (I_lake.Ponds.Count < 2) return (null, null);

            for (int i = 0; i < 20; i++) // Try 20 times
            {
                var pond1 = RandomExtension.PickRandomFromList(I_lake.Ponds);
                var puddle1 = RandomExtension.PickRandomFromList(pond1.Puddles);

                var pond2 = RandomExtension.PickRandomFromList(I_lake.Ponds);
                if (pond1 == pond2) continue; // Must be different periods

                var puddle2 = RandomExtension.PickRandomFromList(pond2.Puddles);
                
                // At least one puddle must have exams
                if (puddle1.Elements.Count == 0 && puddle2.Elements.Count == 0) continue;

                if (IsValidSwap(puddle1, puddle2))
                {
                    var details = new RoomPeriodSwapDetails
                    {
                        Period1 = pond1.Period,
                        Room1 = puddle1.Room,
                        Period2 = pond2.Period,
                        Room2 = puddle2.Room,
                        ExamClasses1 = puddle1.Elements.ToList(),
                        ExamClasses2 = puddle2.Elements.ToList()
                    };

                    var infos = new ClassMoveInfos();
                    
                    // Add Start Move Infos
                    foreach(var exam in details.ExamClasses1)
                        infos.AddStartMoveInfo(exam, new List<(Period, Room)> { (details.Period1, details.Room1) });
                    foreach (var exam in details.ExamClasses2)
                        infos.AddStartMoveInfo(exam, new List<(Period, Room)> { (details.Period2, details.Room2) });

                    UpdateLake(details, false);
                    UpdateMaps(details, false);

                    // Add End Move Infos
                    foreach (var exam in details.ExamClasses1)
                        infos.AddEndMoveInfo(exam, new List<(Period, Room)> { (details.Period2, details.Room2) });
                    foreach (var exam in details.ExamClasses2)
                        infos.AddEndMoveInfo(exam, new List<(Period, Room)> { (details.Period1, details.Room1) });

                    return (details, infos);
                }
            }
            return (null, null);
        }

        private bool IsValidSwap(Puddle puddle1, Puddle puddle2)
        {
             // Check Capacity: Exams from puddle1 must fit in puddle2's room
             int size1 = puddle1.GetUsedCapacity();
             if (size1 > puddle2.Room.Capacity) return false;

             // Check Capacity: Exams from puddle2 must fit in puddle1's room
             int size2 = puddle2.GetUsedCapacity();
             if (size2 > puddle1.Room.Capacity) return false;

             // Check Room Constraints
             foreach(var exam in puddle1.Elements)
             {
                 if (I_examClass_validRoomsPenalties.TryGetValue(exam, out var dict) && !dict.ContainsKey(puddle2.Room))
                    return false;
             }
             foreach (var exam in puddle2.Elements)
             {
                 if (I_examClass_validRoomsPenalties.TryGetValue(exam, out var dict) && !dict.ContainsKey(puddle1.Room))
                     return false;
             }
             
             return true;
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
            if (P_moveDetails != null)
                RevertNeighbor(P_moveDetails);
        }

        void RevertNeighbor(RoomPeriodSwapDetails details)
        {
            UpdateLake(details, true);
            UpdateMaps(details, true);
        }

        void INeighborhoodMove.UpdateOldResource()
        {
            if (P_moveDetails != null)
            {
                foreach(var e in P_moveDetails.ExamClasses1) P_old_exam_slot[e] = P_exam_slot[e];
                foreach(var e in P_moveDetails.ExamClasses2) P_old_exam_slot[e] = P_exam_slot[e];
            }
        }

        void UpdateLake(RoomPeriodSwapDetails details, bool revert)
        {
             var e1 = details.ExamClasses1;
             var e2 = details.ExamClasses2;

             var pond1 = I_lake.GetPond(details.Period1);
             var pond2 = I_lake.GetPond(details.Period2);
             var puddle1 = pond1.GetPuddle(details.Room1);
             var puddle2 = pond2.GetPuddle(details.Room2);

             if (!revert)
             {
                 // Move Exams1: 1 -> 2
                 foreach(var e in e1) 
                 {
                      pond1.TryRemoveElementFromPond(e, puddle1);
                      pond2.TryAddElementToPond(e, puddle2);
                 }
                 // Move Exams2: 2 -> 1
                 foreach(var e in e2) 
                 {
                      pond2.TryRemoveElementFromPond(e, puddle2);
                      pond1.TryAddElementToPond(e, puddle1);
                 }
             }
             else
             {
                 // Revert Exams1: 2 -> 1
                 foreach(var e in e1) 
                 {
                      pond2.TryRemoveElementFromPond(e, puddle2);
                      pond1.TryAddElementToPond(e, puddle1);
                 }
                 // Revert Exams2: 1 -> 2
                 foreach(var e in e2) 
                 {
                      pond1.TryRemoveElementFromPond(e, puddle1);
                      pond2.TryAddElementToPond(e, puddle2);
                 }
             }
        }
        
        void UpdateMaps(RoomPeriodSwapDetails details, bool revert)
        {
             var p1 = details.Period1; 
             var r1 = details.Room1;
             var p2 = details.Period2;
             var r2 = details.Room2;
             var e1 = details.ExamClasses1;
             var e2 = details.ExamClasses2;
             
             if (!revert)
             {
                 foreach(var e in e1) UpdateSingle(e, p1, r1, p2, r2);
                 foreach(var e in e2) UpdateSingle(e, p2, r2, p1, r1);
             }
             else
             {
                 foreach(var e in e1) UpdateSingle(e, p2, r2, p1, r1);
                 foreach(var e in e2) UpdateSingle(e, p1, r1, p2, r2);
             }
        }

        private void UpdateSingle(ExamClass e, Period oldP, Room oldR, Period newP, Room newR)
        {
             // Remove old
             foreach(var s in e.Students)
             {
                 P_slot_students[oldP].SubtractOrDelete(s, 1);
                 P_student_slots[s].SubtractOrDelete(oldP, 1);
             }
             // Add new
             foreach(var s in e.Students)
             {
                 P_slot_students[newP].AddOrCreate(s, 1);
                 P_student_slots[s].AddOrCreate(newP, 1);
             }
             P_exam_slot[e] = [newP];
             P_exam_positions[e] = [(newP, newR)];
        }

        public class RoomPeriodSwapDetails
        {
             public Period Period1 { get; set; }
             public Room Room1 { get; set; }
             public Period Period2 { get; set; }
             public Room Room2 { get; set; }
             public List<ExamClass> ExamClasses1 { get; set; }
             public List<ExamClass> ExamClasses2 { get; set; }
        }
    }
}
