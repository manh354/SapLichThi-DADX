using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.DynamicPooling
{
    public class ClassRoomObjectBuilder 
    {
        public List<DateOnly> I_dates;
        public List<int> I_shifts;
        public List<Room> I_rooms;
        public List<PartialEmptySlot> I_partialEmptySlots;
        public List<EmptySlot> I_emptySlots;
        public Dictionary<Course, HashSet<Course>> I_courseLinkage { get; set; }
        public ClassRoomSea O_sea;
        public void Run()
        {
            O_sea = MakeClassRoomSea();
        }
        private ClassRoomSea MakeClassRoomSea()
        {
            List<ClassRoomPool> pools = new List<ClassRoomPool>();
            foreach(PartialEmptySlot partialEmptySlot in I_partialEmptySlots) 
            { 
                List<ClassRoomContainer> containers = new List<ClassRoomContainer>();
                foreach(Room room in partialEmptySlot.Rooms)
                {
                    var container = MakeClassRoomContainer(room);
                    containers.Add(container);
                }
                ClassRoomPool pool = MakeClassRoomPool(containers, partialEmptySlot.Date, partialEmptySlot.Shift, partialEmptySlot.Courses);
                pools.Add(pool);
            }
            foreach(EmptySlot emptySlot in I_emptySlots)
            {
                List<ClassRoomContainer> containers = new List<ClassRoomContainer>();
                foreach (Room room in I_rooms)
                {
                    var container = MakeClassRoomContainer(room);
                    containers.Add(container);
                }
                ClassRoomPool pool = MakeClassRoomPool(containers, emptySlot.Date, emptySlot.Shift, new List<Course>());
                pools.Add(pool);
            }

            ClassRoomSea result = new ClassRoomSea(pools) 
            { 
                I_courseLinkage = I_courseLinkage,
            };
            return result;
        }
        private ClassRoomPool MakeClassRoomPool(List<ClassRoomContainer> containers, int date, int shift, List<Course> coursesInPool)
        {
            ClassRoomPool result = new ClassRoomPool(containers, date, shift, coursesInPool);
            return result;
        }
        private ClassRoomContainer MakeClassRoomContainer(Room room)
        {
            return new ClassRoomContainer(room);
        }
        
    }
}
