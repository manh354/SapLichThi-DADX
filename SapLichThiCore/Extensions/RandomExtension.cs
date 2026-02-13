namespace SapLichThiAlgorithm.Extensions
{
    public static class RandomExtension
    {
        static int _seed = 0;
        static Random random = new Random();
        public static T? PickRandomFromList<T>(this List<T> list)
        {
            if(list.Count == 0) return default;
            return list[random.Next(list.Count)];
        }

        public static int NextInt(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        public static int NextInt(int maxValue)
        {
            return random.Next(maxValue);
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

        public static double ChooseRandomDouble()
        {
            return random.NextDouble();
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
            T t = list.PickRandomFromListNotFromExistedSet(_existedSet);
            _existedSet.Add(t);
            return t;
        }

        public static T PickRandomFromIEnumerable<T>(this IEnumerable<T> values)
        {
            try
            { 
                return values.ElementAt(random.Next(values.Count()));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static T PickRandomFromIEnumerable<T>(this IEnumerable<T> values, int elementCount)
        {
            return values.ElementAt(random.Next(elementCount));
        }


        public static List<T>? PickRandomSubsetAsList<T>(this List<T> values, int num)
        {
            if( num > values.Count)
            {
                return null;
            }
            HashSet<T> existedSet = new HashSet<T>();
            for (int i = 0; i < num; i++)
            {
                existedSet.Add(values.Where(x=>!existedSet.Contains(x)).PickRandomFromIEnumerable(values.Count - existedSet.Count));
            }
            return existedSet.ToList();
        }

        public static T? PickRandomFromIEnumerable<T>(this IEnumerable<T> values, Func<T, bool> condition)
        {
            List<T> list = values.Where(condition).ToList();
            return list.PickRandomFromList();
        }
    }
}
