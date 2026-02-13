using SapLichThiAlgorithm.ErrorAndLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.DataStructureExtension
{
    internal static class DictionaryExtension
    {
        public static void AddOrCreate<T>(this Dictionary<T, int> object_count, T index, int count)
        {
            object_count[index] = object_count.GetValueOrDefault(index, 0) + count;
        }

        public static void SubtractOrDelete<T>(this Dictionary<T, int> object_count, T index, int count)
        {
            try
            {
                var curVal = object_count[index] -= count;
                if (curVal == 0)
                {
                    object_count.Remove(index);
                }
            }
            catch (Exception e)
            {
                Logger.LogMessage(e.Message);
                throw;
            }
        }
    }
}
