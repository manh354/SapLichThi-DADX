using AlgorithmExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{
    /// <summary>
    /// Represent a set of rules for transforming a collection of objects of type T with conditional object of type U.
    /// </summary>
    /// <typeparam name="T">The list of object that need transformation</typeparam>
    /// <typeparam name="U">The conditional object</typeparam>
    public class RuleSet<T, U>
    {
        public Func<T, U, bool> Condition { get; set; }
        public List<KeySelector<T, U>> KeySelectors { get; set; }

        /// <summary>
        /// Applies a transformation to a collection of objects based on a specified condition and sorting criteria.
        /// </summary>
        /// <remarks>This method filters the input collection based on a condition defined by the
        /// <c>Condition</c> method and then sorts the filtered results in descending order using the
        /// <c>SortDescending</c> method. The specific behavior of <c>Condition</c> and <c>SortDescending</c> depends on
        /// their implementation.</remarks>
        /// <param name="objs">The collection of objects to be filtered and sorted. Cannot be null.</param>
        /// <param name="u">An additional parameter used to evaluate the condition and determine the sorting order. Cannot be null.</param>
        /// <returns>A new list containing the objects that satisfy the condition, sorted in descending order based on the
        /// specified key selectors.</returns>
        public List<T> ApplyTransformation(List<T> objs, U u)
        {
            return objs.Where(o => Condition(o, u)).SortDescending(u, KeySelectors).ToList();
        }

        public List<T> ApplyConditionWithoutSorting(List<T> objs, U u)
        {
            return objs.Where(o => Condition(o, u)).ToList();
        }

        public List<List<T>> ApplyConditionWithSortingByEachKeySelector(List<T> objs, U u)
        {
            List<List<T>> result = new();
            for (int i = 1; i <= KeySelectors.Count; i++)
            {
                result.Add(objs.Where(o => Condition(o, u)).SortDescending(u, KeySelectors[0..i]));
            }
            return result;
        }

        public RuleSet<T,U> AddNewContition(Func<T,U, bool> newCondition)
        {
            if(Condition == null)
            {
                Condition = newCondition;
                return this;
            }
            Condition = Condition.And(newCondition);
            return this;
        }

        public RuleSet<T, U> AddNewKeySelector(KeySelector<T, U> newKeySelector)
        {
            if (KeySelectors == null)
            {
                KeySelectors = new List<KeySelector<T, U>>();
            }
            KeySelectors.Add(newKeySelector);
            return this;
        }

        public RuleSet<T, U> AddNewKeySelector(int index, KeySelector<T, U> newKeySelector)
        {
            if (KeySelectors == null)
            {
                KeySelectors = new List<KeySelector<T, U>>();
            }
            KeySelectors.Insert(index, newKeySelector);
            return this;
        }

    }


    public class PuddleRuleSet<T> : RuleSet<Puddle, T>
    {

    }
    public class PondRuleSet<T> : RuleSet<Pond, T>
    {

    }
}
