using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiStream.Helpers
{
    public static class FuncHelper
    {
        public static Func<T, bool> PutInParent<T> ( this Func<T, bool> child, Func<T, bool> parent)
        {
            return (T t) =>
            {
                if (!parent(t))
                    return child(t);
                return true;
            };
        }

        public static Func<T, bool> WrapOverChild<T>(this Func<T, bool> parent, Func<T, bool> child)
        {
            return (T t) =>
            {
                if (!parent(t))
                    return child(t);
                return true;
            };
        }

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
        {
            return (T t) => left(t) && right(t);
        }

        public static Func<T, bool> Or<T> (this Func<T, bool> left, Func<T, bool> right)
        {
            return (T t) => left(t) || right(t);
        }
    }
}
