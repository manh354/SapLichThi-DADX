using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{
    public class RuleBookExamClass
    {
        public bool LargestRoomFirst { get; set; }
        public bool PrimaryRoomFirst { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> ExamClassLinkages { get; set; }
        public Dictionary<Room, Dictionary<Period, int>> Room_ValidSlotsPenalties { get; set; }
        public Dictionary<ExamClass, Dictionary<Room, int>> ExamClass_ValidRoomsPenalties { get; set; }
        public Dictionary<ExamClass, Dictionary<Period, int>> ExamClass_ValidSlotsPenalties { get; set; }
        public Dictionary<Period, int> Slot_Penalties { get; set; }
        public ExamClass ExamClass { get; set; }
        public bool HardConstraint_LimitedCapacity { get; set; } = true;
        public bool HardConstraint_DifferentRoomForCourses { get; set; } = true;
        public bool HardConstraint_NoStudentConflict { get; set; } = true;
        public bool HardConstraint_OnlyOneExamClassPerRoom { get; set; } = true;
        public float RelaxedCoef { get; set; } = 1f;

        public RuleBookExamClass(
            bool largestRoomFirst,
            bool primaryRoomFirst,
            Dictionary<ExamClass, HashSet<ExamClass>> examClassLinkage,
            Dictionary<Room, Dictionary<Period, int>> room_ValidSlotsPenalties,
            Dictionary<ExamClass, Dictionary<Room, int>> examClass_ValidRoomsPenalties,
            Dictionary<ExamClass, Dictionary<Period, int>> examClass_ValidSlotsPenalties,
            Dictionary<Period, int> slot_Penalties,
            ExamClass examClass,
            bool hardConstraint_LimitedCapacity,
            bool hardConstraint_DifferentRoomForCourses,
            bool hardConstraint_NoStudentConflict,
            bool hardConstraint_OnlyOneExamClassPerRoom,
            float relaxedCoef)
        {
            LargestRoomFirst = largestRoomFirst;
            PrimaryRoomFirst = primaryRoomFirst;
            ExamClassLinkages = examClassLinkage;
            Room_ValidSlotsPenalties = room_ValidSlotsPenalties;
            ExamClass_ValidRoomsPenalties = examClass_ValidRoomsPenalties;
            ExamClass_ValidSlotsPenalties = examClass_ValidSlotsPenalties;
            Slot_Penalties = slot_Penalties;
            ExamClass = examClass;
            HardConstraint_LimitedCapacity = hardConstraint_LimitedCapacity;
            HardConstraint_DifferentRoomForCourses = hardConstraint_DifferentRoomForCourses;
            HardConstraint_NoStudentConflict = hardConstraint_NoStudentConflict;
            HardConstraint_OnlyOneExamClassPerRoom = hardConstraint_OnlyOneExamClassPerRoom;
            RelaxedCoef = relaxedCoef;
        }   
    }
}
