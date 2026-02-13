using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{
    /// <summary>
    /// Puddle is the lowest level in the State hierarchy. A puddle represent a class that is responsible for managing its internal state.
    /// </summary>
    public class Puddle
    {
        private Room room;
        public Room Room => room;
        Period period; public Period Period => period;
        private List<ExamClass> elements;
        public List<ExamClass> Elements => elements;
        //private ExamSpanElement? spanElement;
        //public ExamSpanElement? SpanElement => spanElement;
        private int capacity;
        private int remainingCapacity;
        private int usedCapacity;

        public static Puddle DeepClone(Puddle puddle)
        {
            Puddle newPuddle = new Puddle(puddle.period, puddle.room, new(puddle.elements));
            return newPuddle;
        }

        public Puddle DeepClone()
        {
            Puddle newPuddle = new Puddle(this.period, this.room, new(this.elements));
            return newPuddle;
        }

        public Puddle CopyFrom(Puddle puddle)
        {
            this.period = puddle.period;
            this.room = puddle.room;
            this.elements = puddle.elements;
            this.capacity = puddle.capacity;
            this.remainingCapacity = puddle.remainingCapacity;
            this.usedCapacity = puddle.usedCapacity;
            return this;
        }

        public Puddle(Pond pond, Room room)
        {
            this.period = pond.Period;
            this.room = room;
            elements = new List<ExamClass>();
            capacity = GetRoomCapacity();
            usedCapacity = 0;
            remainingCapacity = capacity;
        }
        public Puddle(Pond pond, Room room, List<ExamClass> elements)
        {
            this.period = pond.Period;
            this.room = room;
            this.elements = elements;
            UpdateUsedCapacity();
            capacity = GetRoomCapacity();
            UpdateRemainingCapacity();

        }

        public Puddle(Period period, Room room, List<ExamClass>? elements = null)
        {
            this.period = period;
            this.room = room;
            this.elements = elements == null ? new() : elements;
            UpdateUsedCapacity();
            capacity = GetRoomCapacity();
            UpdateRemainingCapacity();
        }

        public int GetProctor()
        {
            return usedCapacity >= 70 ? 2 : 1;
        }

        private void UpdateRemainingCapacity()
        {
            remainingCapacity = capacity - usedCapacity;
        }

        public static int GetElementSize(ExamClass e)
        {
            return e.Count;
        }
        public int GetRoomCapacity()
        {
            return (int)(Math.Ceiling(room.Capacity * CompositeSettings.MaximumPercentage));
        }
        private void UpdateUsedCapacity()
        {
            int sum = 0;
            foreach (var item in elements)
            {
                sum += item.Count;
            }
            usedCapacity = sum;
        }
        public int GetRemainingCapacity()
        {
            return remainingCapacity;
        }
        public int GetUsedCapacity()
        {
            return usedCapacity;
        }
        public int GetRelaxedRemainingCapacity(double relaxedCoef)
        {
            return (int)(relaxedCoef * capacity) - usedCapacity;
        }
        public bool IsEmpty()
        {
            return usedCapacity == 0;
        }

        public virtual bool TryAddToPuddle(ExamClass e)
        {
            int elementSize = GetElementSize(e);
            elements.Add(e);
            this.remainingCapacity -= elementSize;
            usedCapacity += elementSize;
            return true;
        }
        public virtual bool RemoveFromPuddle(ExamClass e)
        {
            bool removed = elements.Remove(e);
            if (removed)
            {
                int elementSize = GetElementSize(e);
                remainingCapacity += elementSize;
                usedCapacity -= elementSize;
            }
            return removed;
        }
        public List<ExamClass> RemoveAllElement()
        {
            if (elements.Count == 0)
                return new();
            var result = new List<ExamClass>(elements);
            elements.Clear();
            UpdateUsedCapacity();
            UpdateRemainingCapacity();
            return result;
        }

        public List<ExamClass> RemoveAllExamClasses(HashSet<ExamClass> examClasses)
        {
            if (elements.Count == 0)
                return new();
            var result = new List<ExamClass>();
            foreach (var examClass in elements)
            {
                if (examClasses.Contains(examClass))
                {
                    result.Add(examClass);
                }
            }
            elements.RemoveAll(examClasses.Contains);
            return result;
        }


    }
}
