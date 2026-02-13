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
    public class RoomMove : INeighborhoodMove
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

        private int I_max_mutation { get; set; } = 1;

        private RoomMoveDetails? P_roomMoveDetails { get; set; } = null;
        private ClassMoveInfos? P_classMoveInfos { get; set; } = null;

        bool INeighborhoodMove.CreateNeighbor()
        {
            (P_roomMoveDetails, P_classMoveInfos) = CreateNeighbor();
            return P_roomMoveDetails != null;
        }

        (RoomMoveDetails?, ClassMoveInfos?) CreateNeighbor()
        {
            // Logic to find a single exam class
            var chosenExamClass = RandomExtension.PickRandomFromList(I_examClasses);
            var currentSlot = P_exam_slot[chosenExamClass].First();
            var currentPond = I_lake.GetPond(currentSlot);

            var availableRooms = FindValidDestinationRooms(chosenExamClass, currentPond, out var allPossiblePuddles);
            if (!availableRooms)
                return (null, null);
            var newPuddle = RandomExtension.PickRandomFromList(allPossiblePuddles);

            var oldPosition = P_exam_positions[chosenExamClass].First();
            var newPosition = (currentPond.Period, newPuddle.Room);
            var roomMoveDetails = new RoomMoveDetails()
            {
                ExamClass = chosenExamClass,
                OldPosition = oldPosition,
                NewPosition = newPosition
            };
            var classMoveInfos = new ClassMoveInfos();
            ReadPondInfoStartPosition(classMoveInfos, chosenExamClass, currentPond, newPuddle);
            UpdateLake(roomMoveDetails, false);
            UpdateMaps(roomMoveDetails, false);
            ReadPondInfoEndPosition(classMoveInfos, chosenExamClass, currentPond, newPuddle);

            return (roomMoveDetails, classMoveInfos);
        }
        private void ReadPondInfoStartPosition(ClassMoveInfos infos, ExamClass examClass, Pond pond, Puddle puddle)
        {
            infos.AddStartMoveInfo(examClass, new List<(Period period, Room room)>() { (pond.Period, puddle.Room) });
        }

        private void ReadPondInfoEndPosition(ClassMoveInfos infos, ExamClass examClass, Pond pond, Puddle puddle)
        {
            infos.AddEndMoveInfo(examClass, new List<(Period period, Room room)>() { (pond.Period, puddle.Room) });
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
            this.I_lake = lake;
            return this;
        }

        void INeighborhoodMove.RevertNeighbor()
        {
            RevertNeighbor(P_roomMoveDetails, P_classMoveInfos);
        }

        void RevertNeighbor(RoomMoveDetails singleCourseMoveInfo, ClassMoveInfos classMoveInfos)
        {
            UpdateLake(singleCourseMoveInfo, true);
            UpdateMaps(singleCourseMoveInfo, true);
        }

        void INeighborhoodMove.UpdateOldResource()
        {
            P_old_exam_slot[P_roomMoveDetails.ExamClass] = P_exam_slot[P_roomMoveDetails.ExamClass];
        }

        void UpdateLake(RoomMoveDetails singleCourseMoveInfo, bool revert)
        {
            var startPosition = revert ? singleCourseMoveInfo.NewPosition : singleCourseMoveInfo.OldPosition;
            var endPosition = revert ? singleCourseMoveInfo.OldPosition : singleCourseMoveInfo.NewPosition;
            var examClass = singleCourseMoveInfo.ExamClass;

            RemoveAndAddExamClass(startPosition, endPosition, examClass);
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


        void UpdateMaps(RoomMoveDetails singleCourseMoveInfo, bool revert)
        {
            var startPosition = revert ? singleCourseMoveInfo.NewPosition : singleCourseMoveInfo.OldPosition;
            var endPosition = revert ? singleCourseMoveInfo.OldPosition : singleCourseMoveInfo.NewPosition;
            var examClass = singleCourseMoveInfo.ExamClass;

            var startSlot = (startPosition.period);
            var endSlot = (endPosition.period);

            foreach (var student in examClass.Students)
            {
                P_slot_students[startSlot].SubtractOrDelete(student, 1);
                P_slot_students[endSlot].AddOrCreate(student, 1);
            }

            foreach (var student in examClass.Students)
            {
                P_student_slots[student].SubtractOrDelete(startSlot, 1);
                P_student_slots[student].AddOrCreate(endSlot, 1);
            }

            P_exam_slot[examClass] = [endSlot];

            P_exam_positions[examClass] = [endPosition];

        }

        public class RoomMoveDetails
        {
            public ExamClass ExamClass { get; set; }
            public (Period period, Room room) OldPosition { get; set; }
            public (Period period, Room room) NewPosition { get; set; }
        }
        private bool FindValidDestinationSlots(ExamClass examClass, List<Period> currentSlots, out List<Pond> allPossiblePonds)
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
                        examClass: examClass // This will be set later
                    );
            var ruleSetPondExamClass = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPondCourseRuleSet();
            ruleSetPondExamClass.AddNewContition((p, rb) => !currentSlots.Contains((p.Period)));
            I_lake.FindBestPond(ruleBookExamClass, ruleSetPondExamClass, out allPossiblePonds, out _, sort: false);

            return allPossiblePonds.Count > 0;
        }

        private bool FindValidDestinationRooms(ExamClass examClass, Pond pond, out List<Puddle> allPossiblePuddles)
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
                        examClass: examClass // This will be set later
                    );
            var ruleSetPuddle = ruleBookExamClass.WithHardConstraintsFromContext(Context).BuildDefaultPuddleExamRuleSet();
            pond.FindBestPuddle(ruleBookExamClass, ruleSetPuddle, null, out allPossiblePuddles, out var bestPuddle);
            return allPossiblePuddles.Count > 0;
        }
    }
}
