using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Extensions
{
    public static class RandomExtension
    {
        static Random random = new Random();
        public static T PickRandomFromList<T>(this List<T> list)
        {
            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// Trả về true hoặc false, tuỳ theo điều kiện. Nếu random nhỏ hơn giá trị thì trả về True, không thì False.
        /// </summary>
        /// <param name="trueOverFalse"></param>
        /// <returns></returns>
        public static bool ChooseProbability(double trueOverFalse)
        {
            return random.NextDouble() < trueOverFalse;
        }
        public static int ChooseRandomInt(int start, int end)
        {
            return random.Next(start, end);
        }
        public static T PickRandomFromListNotFromExistedSet<T>(this List<T> list, HashSet<T> _existedSet)
        {
            T t;
            for (t = list.PickRandomFromList(); _existedSet.Contains(t);)
            {
                continue;
            }
            return t;
        }
        public static T PickRandomFromListAndAddToExistedSet<T>(this List<T> list, HashSet<T> _existedSet)
        {
            T t = PickRandomFromListNotFromExistedSet(list,_existedSet);
            _existedSet.Add(t);
            return t;
        }    

        public static List<T> PickRandomSubsetAsList<T>(this List<T> values, int num)
        {
            if(num > values.Count())
            {
                throw new Exception("Error Subset is Bigger than Set");
            }    
            HashSet <T> existedSet = new HashSet<T>();
            List<T> result = new();
            for (int i = 0; i < num; i++)
            {
                result.Add(values.PickRandomFromListAndAddToExistedSet(existedSet));
            }
            return result;
        }    
        public static T PickRandomFromIEnumerable<T> (this IEnumerable<T> values)
        {
            return values.ElementAt(random.Next(values.Count()));
        }
    }
}
