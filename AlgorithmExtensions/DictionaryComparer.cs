using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public static class DictionaryComparer
    {
        public static bool DictionaryEquals<TKey, TValue>(this Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;
            foreach (var kvp in dict1)
            {
                if (!dict2.TryGetValue(kvp.Key, out var value2))
                    return false;
                if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, value2))
                    return false;
            }
            return true;
        }
    }
}
