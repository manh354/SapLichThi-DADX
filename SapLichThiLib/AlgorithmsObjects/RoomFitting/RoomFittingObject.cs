using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SapLichThiLib.AlgorithmsObjects.DynamicPooling
{
    /// <summary>
    /// Container is the lowest level in the State hirachy. A container represent a class that is responsible for managing its internal state.
    /// </summary>
    public class ClassRoomContainer
    {
        private Room box;
        public Room Box => box;
        public List<ExamClass> Elements => elements;
        private List<ExamClass> elements;
        private int capacity;
        private int remainingCapacity;
        private int usedCapacity;
        public ClassRoomContainer(Room box)
        {
            this.box = box;
            elements = new List<ExamClass>();
            capacity = GetBoxCapacity();
            usedCapacity = 0;
            remainingCapacity = capacity;
        }
        public ClassRoomContainer(Room box, List<ExamClass> elements)
        {
            this.box = box;
            this.elements = elements;
            UpdateUsedCapacity();
            capacity = GetBoxCapacity();
            UpdateRemainingCapacity();

        }

        private void UpdateRemainingCapacity()
        {
            remainingCapacity = capacity - usedCapacity;
        }

        public static int GetElementSize(ExamClass e)
        {
            return e.Count;
        }
        public int GetBoxCapacity()
        {
            return box.Capacity * 66/100;
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
        public bool GetElementCompatibilityWithBox(ExamClass e)
        {
            if (elements.Count == 0) return true;
            if (elements.Any(x => x.StudyClass.Course != e.StudyClass.Course))
                return false;
            return true;
        }
        public virtual bool TryAddToContainer(ExamClass e)
        {
            bool compatibility = GetElementCompatibilityWithBox(e);
            if (!compatibility)
            {
                return false;
            }
            int remainingCapacity = GetRemainingCapacity();
            int elementSize = GetElementSize(e);
            bool elementFit = elementSize <= remainingCapacity;
            if (!elementFit)
            {
                return false;
            }
            elements.Add(e);
            this.remainingCapacity -= elementSize;
            usedCapacity += elementSize;
            return true;
        }
        public virtual bool RemoveFromContainer(ExamClass e)
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
        public bool RemoveAllElement()
        {
            if(elements.Count == 0) 
                return false;
            elements.Clear();
            return true;
        }
    }

    /// <summary>
    /// Pool is the next structure in the Hirachy. it contains many Container.
    /// Represent a structure of ( rooms + 1 shift )
    /// </summary>
    public class ClassRoomPool
    {
        List<ClassRoomContainer> containers;
        int date; public int Date => date;
        int shift; public int Shift => shift;
        public HashSet<Course> coursesInPool = new HashSet<Course>();
        int totalCapacity;
        int remainingCapacity;
        int usedCapacity;
        public List<ClassRoomContainer> Containers => containers;
        
        public ClassRoomPool(List<ClassRoomContainer> containers, int date, int shift, List<Course> coursesInPool)
        {
            this.containers = containers;
            totalCapacity = CalculateCapacity();
            remainingCapacity = CalculateRemainingCapacity();
            usedCapacity = CalculateUsedCapacity();
            this.date = date;
            this.shift = shift;
            this.coursesInPool = coursesInPool.ToHashSet();
        }
        public bool FindBestContainer(ExamClass examClass, ClassRoomContainer currentContainer, out ClassRoomContainer? bestContainer)
        {
            bestContainer = null;
            if (currentContainer != null)
            {
                if (currentContainer.GetRemainingCapacity() > examClass.Count
                    && currentContainer.GetElementCompatibilityWithBox(examClass))
                {
                    bestContainer = currentContainer;
                    return true;
                }
            }
            int smallestFit = int.MaxValue;
            foreach (ClassRoomContainer container in containers)
            {
                int thisContainerRemainingCapacity = container.GetRemainingCapacity();
                if (thisContainerRemainingCapacity > examClass.Count
                    && thisContainerRemainingCapacity < smallestFit
                    && container.GetElementCompatibilityWithBox(examClass))
                {
                    smallestFit = thisContainerRemainingCapacity;
                    bestContainer = container;
                }
            }
            if(bestContainer != null )
            {
                return true;
            }
            return false;
        }

        
        public void UpdateAddValue(ExamClass examClass)
        {
            remainingCapacity -= examClass.Count;
            usedCapacity += examClass.Count;
            coursesInPool.Add(examClass.StudyClass.Course);
        }
        private int CalculateCapacity()
        {
            int sum = 0;
            foreach (ClassRoomContainer container in containers)
            {
                sum += container.GetBoxCapacity();
            }
            return sum;
        }
        private int CalculateUsedCapacity()
        {
            int sum = 0;
            foreach (ClassRoomContainer container in containers)
            {
                sum += container.GetUsedCapacity() ;
            }
            return sum;
        }
        private int CalculateRemainingCapacity()
        {
            int sum = 0;
            foreach (ClassRoomContainer container in containers)
            {
                sum += container.GetRemainingCapacity();
            }
            return sum;
        }

        public int GetRemainingCapacity()
        {
            return remainingCapacity;
        }

        public int GetUsableRemainingCapacityForThreshhold(int threshhold)
        {
            var total = containers.FindAll(x => x.GetRemainingCapacity() >= threshhold).Sum(x => x.GetRemainingCapacity());
            return total;
        }

        public bool AddElementToPoolAbitrary(ExamClass element, out ClassRoomContainer? chosenContainer)
        {
            foreach (ClassRoomContainer container in containers)
            {
                if (!container.TryAddToContainer(element))
                    continue;
                chosenContainer = container;
                return true;
            }
            chosenContainer = null;
            return false;
        }
        public bool TryAddElementToPool(ExamClass element, ClassRoomContainer chosenContainer)
        {
            int elementSize = ClassRoomContainer.GetElementSize(element);
            bool check = chosenContainer.TryAddToContainer(element);
            if(check)
            {
                usedCapacity += elementSize;
                remainingCapacity -= elementSize;
                coursesInPool.Add(element.StudyClass.Course);
            }
            return check;
        }
        public bool TryRemoveElementFromPool(ExamClass element, ClassRoomContainer? placedContainer = null)
        {
            if(placedContainer != null)
            {
                totalCapacity -= ClassRoomContainer.GetElementSize(element);
                
                placedContainer.RemoveFromContainer(element);
            }
            foreach (ClassRoomContainer container in containers)
            {
                if (!container.RemoveFromContainer(element))
                    continue;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A sea is used to contain all pools, this is the highest layer in the State hirachy. A sea can be converted to a schedule.
    /// </summary>
    public class ClassRoomSea
    {
        List<ClassRoomPool> pools;
        public List<ClassRoomPool> Pools => pools;
        public Dictionary<Course, HashSet<Course>> I_courseLinkage { get; set; }
        public ClassRoomSea(List<ClassRoomPool> pools)
        {
            this.pools = pools;
        }
        public bool FindBestPool(List<Course> coursesInColor, Dictionary<Course, StudentYear> course_mainStudentYear,int classCount, int totalStudentSize, int smallestClassSize, out ClassRoomPool? bestPool, out List<ClassRoomPool> allSuitablePools)
        {
            // HashSet<Course> courseHashSet = coursesInColor.ToHashSet();
            /* bestPool = pools.FindAll(x =>
             {
                 foreach (var course in coursesInColor)
                 {
                     foreach (var courseInPool in x.coursesInPool)
                     {
                         if (I_courseLinkage[courseInPool].Contains(course))
                             return false;
                     }
                 }
                 return true;
             }).FindAll(x => x.GetRemainingCapacity() >= totalStudentSize).OrderBy(x => x.GetRemainingCapacity()).FirstOrDefault();
 */

            allSuitablePools = pools.FindAll(x =>
            {
                foreach (var courseInColor in coursesInColor)
                {
                    foreach (var courseInPool in x.coursesInPool)
                    {
                        if (!I_courseLinkage.TryGetValue(courseInPool, out var courses))
                            continue;
                        if (courses.Contains(courseInColor))
                        {
                            return false;
                        }
                    }
                }
                return true;
            })
                .FindAll(pool => pool.Containers.Any(container => container.GetRemainingCapacity() > smallestClassSize))
                // Prioritize empty shifts first
                .OrderBy(pool => pool.Containers.Any(containter => containter.Elements.Count > 0))
                .ThenByDescending(pool => pool.coursesInPool.Any(courseInPool => coursesInColor.Any(courseInColor => course_mainStudentYear[courseInColor] == course_mainStudentYear[courseInPool] )))
                .ThenByDescending(pool => pool.Containers.Count(container => container.GetRemainingCapacity() > smallestClassSize) > classCount)
                // First find all pools that can contain whole color Then find all pools that can contain the smallest class
                .ThenByDescending(x => x.GetRemainingCapacity() > totalStudentSize) 
                // Order them by capacity
                .ThenBy(x => x.GetUsableRemainingCapacityForThreshhold(smallestClassSize))
                .ToList();
            bestPool = allSuitablePools.FirstOrDefault();
            // Console.WriteLine("count = {0}, bestPool size = {1}, totalStudentSize = {2}", allSuitablePools.ToList().Count, bestPool == null ? 0 : bestPool.GetRemainingCapacity(), totalStudentSize);
            return bestPool != null;
        }


    }


    public class Command
    {
        ClassRoomContainer Container { get; set; }
        ClassRoomPool Pool { get; set; }
        ExamClass Class { get; set; }
        public void Execute()
        {
            Pool.TryAddElementToPool(Class, Container);
        }
        public void Undo()
        {
            Pool.TryRemoveElementFromPool(Class, Container);
        }
        public static Command CreatCommand(ClassRoomContainer container, ClassRoomPool pool, ExamClass examClass )
        {
            return new Command()
            {
                Container = container,
                Pool = pool,
                Class = examClass
            };
            
        }
    }
    
}
