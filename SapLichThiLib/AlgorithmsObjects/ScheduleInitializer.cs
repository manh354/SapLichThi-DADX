using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class ScheduleInitializer
    {
        //Input
        public List<Room> RoomList { get; set; }
        public List<DateOnly> DateList { get; set; }
        public List<int> ShiftList { get; set; }
        // Output
        public ExamSchedule O_schedule { get; set; }
        public Dictionary<(int, int), bool> DateShift_Reserved_Dictionary { get; set; }
        public int TotalLargeRoomCapacity { get; set; }
        public void CreateEmptySchedule()
        {
            O_schedule = new(DateList.ToArray(), ShiftList.ToArray(), RoomList.ToArray());
            DateShift_Reserved_Dictionary = new Dictionary<(int, int), bool>();
            TotalLargeRoomCapacity = GetTotalLargeRoomCapacity();
        }

        private int GetTotalLargeRoomCapacity()
        {
            return O_schedule.rooms.Sum(x => x.RoomType == RoomType.large ? x.Capacity : 0);
        }
    }
}
