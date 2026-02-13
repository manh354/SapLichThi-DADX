using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public static class ListExtension
    {
        public static T GetLargest<T>(this List<T> list, Comparer<T> comparer)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty");

            T largest = list[0];
            foreach (var item in list)
            {
                if (comparer.Compare(item, largest) > 0)
                {
                    largest = item;
                }
            }
            return largest;
        }

        public static T GetSmallest<T>(this List<T> list, Comparer<T> comparer)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty");

            T smallest = list[0];
            foreach (var item in list)
            {
                if (comparer.Compare(item, smallest) < 0)
                {
                    smallest = item;
                }
            }
            return smallest;
        }

        public static List<T> SortDescending<T>(this List<T> list, IComparer<T> comparer)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty");

            list.Sort((a, b) => comparer.Compare(b, a));
            return list;
        }

        public static List<MainObject> SortDescending<MainObject, AdditionalObject>(this List<MainObject> list, AdditionalObject ao, ComparerExtend<MainObject, AdditionalObject> comparer)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null");

            comparer.SetConditionalObject(ao);
            list.Sort((a, b) => comparer.Compare(b, a));
            return list;
        }

        public static List<T> SortDescending<T>(this List<T> list, Comparison<T> comparision)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null");

            list.Sort(comparision);
            return list;
        }

        public static List<MainObject> SortDescending<MainObject, AdditionalObject>(this List<MainObject> list, AdditionalObject ao, List<KeySelector<MainObject, AdditionalObject>> keySelectors)
        {
            if (list == null)
                throw new ArgumentException("List cannot be null");

            if (keySelectors == null || keySelectors.Count == 0)
                return list;

            var firstKeySelector = keySelectors.First();
            firstKeySelector.SetConditionalObject(ao);
            var sorted = list.OrderByDescending(mo => firstKeySelector.GetKey(mo));

            foreach (var keySelector in keySelectors[1..])
            {
                keySelector.SetConditionalObject(ao);
                sorted = sorted.OrderByDescending(mo => keySelector.GetKey(mo));
            }
            return sorted.ToList();
            
        }

        public static List<MainObject> SortDescending<MainObject, AdditionalObject>(this IEnumerable<MainObject> enumerable, AdditionalObject ao, List<KeySelector<MainObject, AdditionalObject>> keySelectors)
        {
            if (enumerable == null)
                throw new ArgumentException("List cannot be null");

            if (keySelectors == null || keySelectors.Count == 0)
                return enumerable.ToList();

            var firstKeySelector = keySelectors.First();
            firstKeySelector.SetConditionalObject(ao);
            var sorted = enumerable.OrderByDescending(mo => firstKeySelector.GetKey(mo));

            foreach (var keySelector in keySelectors[1..])
            {
                keySelector.SetConditionalObject(ao);
                sorted = sorted.ThenByDescending(mo => keySelector.GetKey(mo));
            }
            return sorted.ToList();

        }
    }

}
