using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class RoomShiftSlot
    {
        public Room room;
        public int shift;
        public RoomShiftSlot(Room room, int shift)
        {
            this.room = room;
            this.shift = shift;
        }
    }
    public class RoomShiftScheme
    {
        public Room Room;
        public int Shift;
        public List<ExamClass> ExamClasses;
        private float _usagePercentage;
        private int _sum;
        public RoomShiftScheme(Room room, int shift, List<ExamClass> examClasses)
        {
            this.Room = room;
            this.Shift = shift;
            this.ExamClasses = examClasses;
        }
        public float GetPercentage()
        {
            return _usagePercentage = (float)GetSum() / Room.Capacity;
        }
        public int GetSum()
        {
            return _sum = ExamClasses.Sum(x => x.Count);

        }
    }
}
