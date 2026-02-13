using AlgorithmExtensions;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization;
using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{

    /// <summary>
    /// A lake is used to contain all ponds, this is the highest layer in the State hierarchy. A sea can be converted to a schedule.
    /// </summary>
    public class Lake
    {
        public List<Pond> Ponds { get; set; }
        private Dictionary<(DateOnly date, int shift), Pond> P_indexer { get; set; }
        public Lake(List<Pond> ponds)
        {
            this.Ponds = ponds;
            var dates = ponds.Select(x => x.DateIndex).ToHashSet();
            var shifts = ponds.Select(x => x.ShiftIndex).ToHashSet();
            P_chosenPondsCount = new (int, int)[dates.Count];
            P_pondAvailableMap = new bool[dates.Count][];
            P_indexer = Ponds.ToDictionary(p => (p.Period.Date, p.Period.Shift), p => p);
            for (int i = 0; i < dates.Count; i++)
            {
                P_chosenPondsCount[i] = (i, 0);
                P_pondAvailableMap[i] = new bool[shifts.Count];
                Array.Fill(P_pondAvailableMap[i], true);
            }

        }


        public bool FindBestPond<T>(T t, PondRuleSet<T> ruleSet, out List<Pond> allSuitablePonds, out Pond? bestPond, bool sort = true)
        {
            allSuitablePonds = sort ? ruleSet.ApplyTransformation(Ponds, t) : ruleSet.ApplyConditionWithoutSorting(Ponds, t);
            if (allSuitablePonds.Count == 0)
            {
                bestPond = null;
                return false;
            }
            bestPond = allSuitablePonds[0];
            return true;
        }

        public bool FindBestPondConsecutive<T>(T t, PondRuleSet<T> ruleSet, out List<Pond> allSuitablePonds, int count, out List<Pond>? bestPonds, bool sort = true)
        {
            allSuitablePonds = sort ? ruleSet.ApplyTransformation(Ponds, t) : ruleSet.ApplyConditionWithoutSorting(Ponds, t);
            if (allSuitablePonds.Count < count)
            {
                bestPonds = null;
                return false;
            }
            var referencePonds = allSuitablePonds.OrderBy(s => s.DateIndex).ThenBy(s => s.ShiftIndex).ToList();
            Func<Pond, Pond, bool> IsPondNext;
            if (count <= this.Ponds.Select(x => x.ShiftIndex).Distinct().Count())
            {
                IsPondNext = (Pond a, Pond b) => { return a.DateIndex == b.DateIndex && b.ShiftIndex == a.ShiftIndex + 1; };
            }
            else
            {
                IsPondNext = (Pond a, Pond b) => { return (b.DateIndex == a.DateIndex && b.ShiftIndex == a.ShiftIndex + 1) || (b.DateIndex == a.DateIndex + 1 && b.ShiftIndex == 0); };
            }
            var bestConsecutivePondsFound = allSuitablePonds.FindConsecutiveSequences(IsPondNext, count).ToList();
            if (bestConsecutivePondsFound.Count == 0)
            {
                bestPonds = null;
                return false;
            }
            bestPonds = bestConsecutivePondsFound.OrderBy(cps => cps.Sum(p => p.GetRemainingCapacity())).First().ToList();
            if (bestPonds != null && bestPonds.Count > 0)
            {
                return true;
            }
            return false;
        }


        private (int index, int pondCount)[] P_chosenPondsCount { get; set; }
        private bool[][] P_pondAvailableMap { get; set; }
        private int P_jumpIndex = 3;


        public Pond GetPond(DateOnly date, int shift)
        {
            return P_indexer[(date, shift)];
        }

        public Pond GetPond((DateOnly date, int shift) slot)
        {
            return P_indexer[slot];
        }

        public Pond GetPond(Period period)
        {
            return P_indexer[(period.Date, period.Shift)];
        }

        public static bool FindNConsecutiveTrueValue(bool[] array, int k, out int indexFound)
        {
            indexFound = -1;
            for (int i = 0, consecutiveCount = 0; i < array.Length; i++)
            {
                consecutiveCount = array[i] ? consecutiveCount + 1 : 0;
                if (consecutiveCount == k)
                {
                    indexFound = i - k + 1;
                    return true;
                }
            }
            return false;
        }


        public Lake DeepCloneLake()
        {

            var original = this;

            // Clone ponds (uses Pond.DeepClone which clones puddles and elements list references)
            var clonedPonds = original.Ponds.Select(p => Pond.DeepClone(p)).ToList();

            // Create new lake from cloned ponds and cloned slots map
            var clonedLake = new Lake(clonedPonds);


            return clonedLake;
        }

        //public static Lake FromExamSchedule(ExamSchedule schedule)
        //{
        //    var dates = schedule.Dates;
        //    var shifts = schedule.Shifts;
        //    var rooms = schedule.Rooms;

        //    List<Pond> ponds = new();
        //    foreach (var (date, dateIndex) in dates.Zip(Enumerable.Range(0, dates.Length)))
        //    {
        //        foreach (var shift in shifts)
        //        {
        //            var slotKey = (date, shift);
        //            List<Puddle> puddles = new();
        //            foreach (var room in rooms)
        //            {
        //                Puddle puddle = new(date, shift, room, schedule.GetCell(date, shift, room).ExamClasses);
        //                puddles.Add(puddle);
        //            }
        //            Pond thisPond = new Pond(puddles, date, dateIndex, shift, shift, puddles.SelectMany(x => x.Elements).ToList());
        //            ponds.Add(thisPond);
        //        }
        //    }
        //    Lake result = new(ponds, new());

        //    return result;
        //}

    }
}
