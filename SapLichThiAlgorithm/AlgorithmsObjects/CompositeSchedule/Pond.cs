using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System.Xml.Linq;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{
    /// <summary>
    /// Pond is the next structure in the Hierarchy. it contains many Puddle.
    /// Represent a structure of ( rooms + 1 shift )
    /// </summary>
    public class Pond
    {
        Dictionary<Room, Puddle> roomPuddleMap = new();
        List<Puddle> puddles;
        Period period; public Period Period => period;
        int dateIndex; public int DateIndex => dateIndex;
        int shiftIndex; public int ShiftIndex => shiftIndex;
        public HashSet<ExamClass> ExamClassesInPond { get; set; } = new HashSet<ExamClass>();
        int totalCapacity;
        int remainingCapacity;
        int usedCapacity;
        public List<Puddle> Puddles => puddles;
        public float[] quantativeProperties = new float[4];

        public Pond(List<Puddle> puddles, Period period, List<ExamClass> examClassesInPond)
        {
            this.puddles = puddles;
            totalCapacity = CalculateCapacity();
            remainingCapacity = CalculateRemainingCapacity();
            usedCapacity = CalculateUsedCapacity();
            this.period = period;

            this.ExamClassesInPond = examClassesInPond.ToHashSet();
            this.roomPuddleMap = puddles.ToDictionary(x => x.Room, x => x);
        }


        public static Pond DeepClone(Pond pond)
        {
            List<Puddle> newPuddles = pond.Puddles.Select(x => Puddle.DeepClone(x)).ToList();
            Pond newPond = new(newPuddles, pond.period, new(pond.ExamClassesInPond));
            return newPond;
        }

        public Pond DeepClone()
        {
            List<Puddle> newPuddles = this.Puddles.Select(x => x.DeepClone()).ToList();
            Pond newPond = new(newPuddles, this.period, new(this.ExamClassesInPond));
            return newPond;
        }

        public void CopyFrom(Pond pond)
        {
            this.totalCapacity = pond.totalCapacity;
            this.remainingCapacity = pond.remainingCapacity;
            this.usedCapacity = pond.usedCapacity;
            this.ExamClassesInPond = pond.ExamClassesInPond;
            foreach (var (puddle, puddleOther) in this.puddles.Zip(pond.Puddles))
            {
                puddle.CopyFrom(puddleOther);
            }
        }

        public Puddle GetPuddle(Room room)
        {
            return roomPuddleMap[room];
        }

        public bool FindBestPuddle<T>(T t, PuddleRuleSet<T> puddleRuleSet, Puddle? currentPuddle, out List<Puddle> suitablePuddles, out Puddle? bestPuddle)
        {
            suitablePuddles = puddleRuleSet.ApplyTransformation(Puddles, t);
            if (suitablePuddles.Count == 0)
            {
                bestPuddle = null;
                return false;
            }
            bestPuddle = suitablePuddles[0];
            return true;
        }

        private int CalculateCapacity()
        {
            int sum = 0;
            foreach (Puddle puddle in puddles)
            {
                sum += puddle.GetRoomCapacity();
            }
            return sum;
        }


        public int GetUsedCapacity()
        {
            return usedCapacity;
        }

        private int CalculateUsedCapacity()
        {
            int sum = 0;
            foreach (Puddle puddle in puddles)
            {
                sum += puddle.GetUsedCapacity();
            }
            return sum;
        }
        private int CalculateRemainingCapacity()
        {
            int sum = 0;
            foreach (Puddle puddle in puddles)
            {
                sum += puddle.GetRemainingCapacity();
            }
            return sum;
        }

        public int GetRemainingCapacity()
        {
            return remainingCapacity;
        }


        public int GetUsableRemainingCapacityForThreshhold(int threshhold)
        {
            var total = puddles.Where(x => x.GetRemainingCapacity() >= threshhold).Sum(x => x.GetRemainingCapacity());
            return total;
        }


        public Puddle RemoveExamClassFromPond(ExamClass examClass)
        {
            ExamClassesInPond.Remove(examClass);
            Puddle? placedPuddle = null;
            foreach (Puddle puddle in puddles)
            {
                if (puddle.Elements.Contains(examClass))
                {
                    placedPuddle = puddle;
                    break;
                }
            }
            if (placedPuddle != null)
            {
                usedCapacity -= Puddle.GetElementSize(examClass);
                remainingCapacity += Puddle.GetElementSize(examClass);
                placedPuddle.RemoveFromPuddle(examClass);
            }
            return placedPuddle!;
        }

        public bool RestoreCourseToPond(Dictionary<ExamClass, Puddle> examClassesMap)
        {
            ExamClassesInPond.UnionWith(examClassesMap.Keys);
            foreach (var (examClass, puddle) in examClassesMap)
            {
                if (!this.TryAddElementToPond(examClass, puddle))
                {
                    return false;
                }
            }
            return true;
        }


        public List<ExamClass> RemoveExamClassesFromPond(HashSet<ExamClass> examClasses)
        {
            ExamClassesInPond.SymmetricExceptWith(examClasses);
            List<ExamClass> result = new();
            foreach (var puddle in puddles)
            {
                result.AddRange(puddle.RemoveAllExamClasses(examClasses));
            }
            totalCapacity = CalculateCapacity();
            remainingCapacity = CalculateRemainingCapacity();
            usedCapacity = CalculateUsedCapacity();
            return result;
        }


        public bool TryAddElementToPond(ExamClass element, Puddle chosenPuddle)
        {
            ExamClassesInPond.Add(element);
            int elementSize = Puddle.GetElementSize(element);
            bool check = chosenPuddle.TryAddToPuddle(element);
            if (check)
            {
                usedCapacity += elementSize;
                remainingCapacity -= elementSize;
                ExamClassesInPond.Add(element);
            }
            return check;
        }

        public bool TryRemoveElementFromPond(ExamClass element, Puddle? placedPuddle = null)
        {
            ExamClassesInPond.Remove(element);
            if (placedPuddle != null)
            {
                usedCapacity -= Puddle.GetElementSize(element);
                if (placedPuddle.RemoveFromPuddle(element))
                    return true;
                return false;
            }
            foreach (Puddle puddle in puddles)
            {
                if (!puddle.RemoveFromPuddle(element))
                    continue;
                return true;
            }
            return false;
        }

        public void RemoveEverythingFromPuddle(Puddle puddle)
        {
            foreach (var element in puddle.Elements)
            {
                ExamClassesInPond.Remove(element);
                remainingCapacity += Puddle.GetElementSize(element);
                usedCapacity -= Puddle.GetElementSize(element);
            }
            puddle.RemoveAllElement();
        }

        public void RemoveEverythingFromPond()
        {
            puddles.ForEach(x => x.RemoveAllElement());
            totalCapacity = CalculateCapacity();
            remainingCapacity = CalculateRemainingCapacity();
            usedCapacity = CalculateUsedCapacity();
            this.ExamClassesInPond.Clear();
        }

        public List<ExamClass> RemoveEveryThingFromPondWithExamClasses()
        {
            var result = new List<ExamClass>();
            var allExamClasses = puddles.Select(x => x.RemoveAllElement());
            foreach (var examClassesInPuddle in allExamClasses)
            {
                result.AddRange(examClassesInPuddle);
            }
            totalCapacity = CalculateCapacity();
            remainingCapacity = CalculateRemainingCapacity();
            usedCapacity = CalculateUsedCapacity();
            return result;
        }



    }
}
