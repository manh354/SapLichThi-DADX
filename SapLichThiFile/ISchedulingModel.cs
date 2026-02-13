using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiStream
{
    public interface ISchedulingModel
    {
        public List<Period> GetPeriods();
        public bool GetUseAnnealing();
        public float GetOptimalPercentage();
        public bool GetUseExamClass();
        public int GetStartId();
        public Dictionary<ExamClass, Dictionary<Period, int>> GetExamClassValidSlots();
        public Dictionary<ExamClass, Dictionary<Room, int>> GetExamClassValidRooms();
        public Dictionary<Room, Dictionary<Period, int>> GetRoomValidSlots();
        public Dictionary<Period, int> GetSlotPriority();
        public List<BinaryConstraint> GetBinaryConstraints();
        public List<UnaryConstraint> GetUnaryConstraints();
        public HardConstraints GetHardConstraints();
    }

    public class HardConstraints
    {
        public bool HardConstraint_NoStudentConflict { get; set; } = true;
        public bool HardConstraint_LimitedCapacity { get; set; } = true;
        public bool HardConstraint_DifferentRoomForCourses { get; set; } = true;
    }
}
