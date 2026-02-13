using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public static class ConsecutiveExtension
    {

        public static List<T[]> FindConsecutiveSequences<T>(
            this List<T> list,
            Func<T, T, bool> isNext,
            int n)
        {
            if (list == null || n <= 0 || list.Count < n)
                return new List<T[]>();

            return Enumerable.Range(0, list.Count - n + 1)
                             .Where(i => IsConsecutiveSequence(list, i, n, isNext))
                             .Select(i => list.Skip(i).Take(n).ToArray())
                             .ToList();
        }

        private static bool IsConsecutiveSequence<T>(
            List<T> list,
            int startIndex,
            int length,
            Func<T, T, bool> isNext)
        {
            for (int i = 0; i < length - 1; i++)
            {
                if (!isNext(list[startIndex + i], list[startIndex + i + 1]))
                    return false;
            }
            return true;
        }
    }
}
