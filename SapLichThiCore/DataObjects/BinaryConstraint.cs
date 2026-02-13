using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiCore.DataObjects
{
    public class BinaryConstraint
    {
        public ExamClass ExamClass1 { get; set; }
        public ExamClass ExamClass2 { get; set; }
        public BinaryConstraintType ConstraintType { get; set; }
        public bool IsHard { get; set; } = true;
    }

    public class UnaryConstraint
    {
        public ExamClass ExamClass { get; set; }
        public UnaryConstraintType ConstraintType { get; set; }
        public bool IsHard { get; set; } = true;
    }


    public enum BinaryConstraintType
    {
        SAME_SLOT,
        DIFFERENT_SLOT,
        AFTER,
    }

    public enum UnaryConstraintType
    {
        ROOM_EXCLUSIVE,
    }
}
