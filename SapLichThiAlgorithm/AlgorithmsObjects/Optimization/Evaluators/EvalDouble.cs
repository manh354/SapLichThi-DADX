using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    public struct EvalDouble : IComparable<EvalDouble>
    {
        public double hardCost;
        public double softCost;
        public EvalDouble(double hardCost, double softCost)
        {
            this.hardCost = hardCost;
            this.softCost = softCost;
        }

        public static bool operator >(EvalDouble left, EvalDouble right)
        {
            return left.hardCost > right.hardCost || left.hardCost == right.hardCost && left.softCost > right.softCost;
        }
        public static bool operator <(EvalDouble left, EvalDouble right)
        {
            return left.hardCost < right.hardCost || left.hardCost == right.hardCost && left.softCost < right.softCost;
        }

        public static bool operator ==(EvalDouble left, EvalDouble right)
        {
            return left.hardCost == right.hardCost && left.softCost == right.softCost;
        }
        public static bool operator !=(EvalDouble left, EvalDouble right)
        {
            return left.hardCost != right.hardCost || left.softCost != right.softCost;
        }
        public static bool operator >=(EvalDouble left, EvalDouble right)
        {
            return left > right || left == right;
        }
        public static bool operator <=(EvalDouble left, EvalDouble right)
        {
            return left < right || left == right;
        }
        public static bool operator >=(EvalDouble left, double right)
        {
            return left.hardCost >= right || left.hardCost == right && left.softCost >= right;
        }
        public static bool operator <=(EvalDouble left, double right)
        {
            return left.hardCost <= right || left.hardCost == right && left.softCost <= right;
        }
        public static bool operator >(EvalDouble left, double right)
        {
            return left.hardCost > right || left.hardCost == right && left.softCost > right;
        }
        public static bool operator <(EvalDouble left, double right)
        {
            return left.hardCost < right || left.hardCost == right && left.softCost < right;
        }

        public static EvalDouble operator -(EvalDouble left, EvalDouble right)
        {
            return new EvalDouble(left.hardCost - right.hardCost, left.softCost - right.softCost);
        }
        public static EvalDouble operator +(EvalDouble left, EvalDouble right)
        {
            return new EvalDouble(left.hardCost + right.hardCost, left.softCost + right.softCost);
        }

        public static EvalDouble operator *(EvalDouble left, double right)
        {
            return new EvalDouble(left.hardCost * right, left.softCost * right);
        }

        public static EvalDouble MaxValue => new EvalDouble(double.MaxValue, double.MaxValue);

        public int CompareTo(EvalDouble other)
        {
            if(this > other) return 1;
            else if(this < other) return -1;
            else return 0;
        }

        public override string ToString()
        {
            return $"(Hard: {hardCost}, Soft: {softCost})";
        }
    }
}
