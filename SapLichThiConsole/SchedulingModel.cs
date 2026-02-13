using SapLichThiCore.DataObjects;
using SapLichThiStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiWebConsole
{
    public class SchedulingModel : ISchedulingModel
    {
        public List<Period> Periods { get; set; } = new();
        public bool UseAnnealing { get; set; }
        public float MaximumPercentage { get; set; }
        public bool UseExamClass { get; set; }
        public int StartId { get; set; }

        public Dictionary<ExamClass, Dictionary<Period, int>> ExamClass_ValidSlotsPenalties { get; set; } = new();
        public Dictionary<ExamClass, Dictionary<Room, int>> ExamClass_ValidRoomsPenalties { get; set; } = new();
        public Dictionary<Room, Dictionary<Period, int>> Room_ValidSlotsPenalties { get; set; } = new();
        public Dictionary<Period, int> Slot_Penalties { get; set; } = new();
        public List<BinaryConstraint> BinaryConstraints { get; set; } = new();
        public List<UnaryConstraint> UnaryConstraints { get; set; } = new();
        public HardConstraints HardConstraints { get; set; } = new HardConstraints();

        public float GetOptimalPercentage()
        {
            return MaximumPercentage;
        }

        public List<Period> GetPeriods()
        {
            return Periods;
        }

        public int GetStartId()
        {
            return StartId;
        }

        public bool GetUseAnnealing()
        {
            return UseAnnealing;
        }

        public bool GetUseExamClass()
        {
            return UseExamClass;
        }

        public Dictionary<ExamClass, Dictionary<Period, int>> GetExamClassValidSlots()
        {
            return ExamClass_ValidSlotsPenalties;
        }
        public Dictionary<ExamClass, Dictionary<Room, int>> GetExamClassValidRooms()
        {
            return ExamClass_ValidRoomsPenalties;
        }
        public Dictionary<Room, Dictionary<Period, int>> GetRoomValidSlots()
        {
            return Room_ValidSlotsPenalties;
        }
        public Dictionary<Period, int> GetSlotPriority()
        {
            return Slot_Penalties;
        }

        public List<BinaryConstraint> GetBinaryConstraints()
        {
            return BinaryConstraints;
        }

        public List<UnaryConstraint> GetUnaryConstraints()
        {
            return UnaryConstraints;
        }

        public HardConstraints GetHardConstraints()
        {
            return HardConstraints;
        }
    }
}
