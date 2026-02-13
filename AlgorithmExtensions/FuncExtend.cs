using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public static class FuncExtend
    {
        public static Func<T,U, bool> And<T, U>(this Func<T,U, bool> primary, Func<T, U, bool> secondary)
        {
            return (t, u) =>
            {
                return primary(t, u) && secondary(t, u); 
            };
        }

        public static Func<T,U, bool> Or<T,U> (this Func<T,U, bool> primary, Func<T,U, bool> secondary)
        {
            return (t, u) =>
            {
                return primary(t, u) || secondary(t, u);
            };
        }

        public static Func<T,U,bool> Xor<T,U> (this Func<T, U, bool> primary, Func<T, U, bool> secondary)
        {
            return (t, u) =>
            {
                return primary(t, u) ^ secondary(t, u);
            };
        }
    }
}
