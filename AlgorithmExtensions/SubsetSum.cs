using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AlgorithmExtensions
{
    public class SubsetSum
    {

        public static List<int> FindMaxSubsetIndexes(List<int> numbers, int N)
        {
            int count = numbers.Count;

            // DP table to track maximum sum achievable with a capacity of `j`
            int[,] dp = new int[count + 1, N + 1];

            // To reconstruct the solution
            bool[,] selected = new bool[count + 1, N + 1];

            // Fill the DP table
            for (int i = 1; i <= count; i++)
            {
                for (int j = 0; j <= N; j++)
                {
                    if (numbers[i - 1] <= j) // Current number fits in the capacity
                    {
                        // Check if including the current number gives a higher sum
                        if (dp[i - 1, j - numbers[i - 1]] + numbers[i - 1] > dp[i - 1, j])
                        {
                            dp[i, j] = dp[i - 1, j - numbers[i - 1]] + numbers[i - 1];
                            selected[i, j] = true; // Mark this number as selected
                        }
                        else
                        {
                            dp[i, j] = dp[i - 1, j];
                        }
                    }
                    else
                    {
                        dp[i, j] = dp[i - 1, j];
                    }
                }
            }

            // Backtrack to find the indexes of the numbers in the subset
            List<int> resultIndexes = new List<int>();
            int remainingCapacity = N;
            for (int i = count; i > 0; i--)
            {
                if (selected[i, remainingCapacity])
                {
                    resultIndexes.Add(i - 1); // Add the index of the selected number
                    remainingCapacity -= numbers[i - 1];
                }
            }

            resultIndexes.Reverse(); // Optional: Reverse to maintain the input order
            return resultIndexes;
        }


        public static List<T> FindMatchingSumSubset<T>(List<T> values, Func<T, int> keyCalculator, int Sum)
        {
            var listInt = values.Select(keyCalculator).ToList();
            var indexes = FindMaxSubsetIndexes(listInt, Sum);
            return indexes.Select(i => values[i]).ToList();
        }


        /// <summary>
        /// The list ought to be sorted in ascending order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="keyCalculator"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static List<List<List<T>>>? DivideListOfListIntoNewListOfListOfList<T>(
            List<List<T>> values,
            Func<T, int> keyCalculator,
            int sum,
            int requiredDivisionCount,
            out bool sumCanBeMatched,
            float lowerBound = 0.9f)
        {
            var result = new List<List<List<T>>>();
            var currentList = new List<List<T>>();
            int currentSum = 0, currentDivision = 1;

            int keySelectorLarge(List<T> list)
            {
                return list.Sum(keyCalculator);
            }


            currentList = FindMaxSumByOrder(values, keySelectorLarge, sum);
            result.Add(currentList);
            var remainingList = values.Except(currentList).ToList(); // It does preseve the order of the list.
            currentSum = currentList.Sum(keySelectorLarge);
            var requiredSum = sum - currentSum;
            // Breakup the smaller list.
            foreach (var list in remainingList.ToList())
            {
                var currentSmallerList = FindMatchingSumSubset(list, keyCalculator, requiredSum);
                if (currentSmallerList.Count == 0)
                    continue;
                else
                {
                    currentSum += currentSmallerList.Sum(keyCalculator);
                    var remainingSmallerList = list.Except(currentSmallerList).ToList();
                    remainingList[remainingList.IndexOf(list)] = list.Except(currentSmallerList).ToList();
                    currentList.Add(currentSmallerList);
                    break;
                }
            }

            if (currentList.Sum(keySelectorLarge) / (float)sum < lowerBound)
            {
                sumCanBeMatched = false;
                result = null;
            }
            else
            {
                sumCanBeMatched = true;
                if (remainingList.Count > 0)
                    result.Add(remainingList);
            }

            if (result != null)
            {
                var DEBUG_result = result.SelectMany(x => x.SelectMany(x => x)).ToHashSet();
                var DEBUG_values = values.SelectMany(x => x).ToHashSet();
                if (!DEBUG_result.SetEquals(DEBUG_values))
                {
                    throw new Exception("Mismatch in values after division.");
                }
            }

            return result;
        }

        private static List<T> FindMaxSumByOrder<T>(List<T> values, Func<T, int> keySelector, int sum)
        {

            var currentSum = 0;
            var result = new List<T>();
            foreach (var item in values)
            {
                currentSum += keySelector(item);
                if (currentSum <= sum)
                    result.Add(item);
            }
            return result;
        }


        /// <summary>
        /// The list ought to be sorted in ascending order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="keyCalculator"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static List<List<List<T>>>? DivideListOfListIntoNewListOfListOfList<T>(
            List<List<T>> values,
            Func<T, int> keyCalculator,
            out bool sumCanBeMatched,
            float[] percentages,
            float lowerBound = 0.9f)
        {
            // Calculate target sums for each division based on percentages
            int totalSum = values.SelectMany(list => list).Sum(keyCalculator);
            var targetSums = percentages.Select(p => (int)(totalSum * p)).ToList();

            sumCanBeMatched = true;
            var result = new List<List<List<T>>>();

            int keySelectorLarge(List<T> list)
            {
                return list.Sum(keyCalculator);
            }

            foreach (var targetSum in targetSums)
            {
                int currentSum = 0;
                var currentList = FindMaxSumByOrder(values, keySelectorLarge, targetSum);
                var remainingList = values.Except(currentList).ToList(); // It does preseve the order of the list.
                currentSum = currentList.Sum(keySelectorLarge);
                var requiredSum = targetSum - currentSum;
                // Breakup the smaller list.
                foreach (var list in remainingList.ToList())
                {
                    var currentSmallerList = FindMatchingSumSubset(list, keyCalculator, requiredSum);
                    if (currentSmallerList.Count == 0)
                        continue;
                    else
                    {
                        currentSum += currentSmallerList.Sum(keyCalculator);
                        var remainingSmallerList = list.Except(currentSmallerList).ToList();
                        remainingList[remainingList.IndexOf(list)] = remainingSmallerList;
                        currentList.Add(currentSmallerList);
                        break;
                    }
                }

                if (currentList.Sum(keySelectorLarge) / (float)targetSum < lowerBound)
                {
                    sumCanBeMatched = false;
                    result = null;
                    break;
                }
                else
                {
                    values = remainingList;
                }
                result.Add(currentList);

            }

            if (values.Count > 0 && sumCanBeMatched && result != null)
            {
                // Add any remaining items to the last division
                if (result.Count > 0)
                {
                    result[result.Count - 1].AddRange(values);
                }
                else
                {
                    result.Add(values);
                }
            }

            return result;
        }

        public static List<int> FindMaxSubsetIndexesBacktrack(List<int> ints, int sum)
        {
            List<int> closestSublist = new List<int>();
            List<int> closestIndexes = new List<int>();
            int closestSum = int.MinValue;

            void Backtrack(List<int> currentList, List<int> currentIndexes, int currentIndex, int currentSum)
            {
                if (currentSum > sum) return; // Prune if the current sum exceeds the target

                if (currentSum > closestSum)
                {
                    closestSum = currentSum;
                    closestSublist = new List<int>(currentList);
                    closestIndexes = new List<int>(currentIndexes);
                }

                for (int i = currentIndex; i < ints.Count; i++)
                {
                    currentList.Add(ints[i]);
                    currentIndexes.Add(i);
                    Backtrack(currentList, currentIndexes, i + 1, currentSum + ints[i]);
                    currentList.RemoveAt(currentList.Count - 1); // Backtrack
                    currentIndexes.RemoveAt(currentIndexes.Count - 1); // Backtrack
                }
            }

            Backtrack(new List<int>(), new List<int>(), 0, 0);
            return closestIndexes;
        }

    }

}
