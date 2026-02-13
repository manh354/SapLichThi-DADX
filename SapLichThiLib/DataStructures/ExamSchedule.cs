using SapLichThiLib.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataStructures
{
    public class ArrayCell
    {
        public List<ExamClass> ExamClasses { get; set; }
        
        public int Capacity { get; set; }
        public int TotalStudent { get; set; }
        public ArrayCell(List<ExamClass> examClasses,int capacity)
        {
            ExamClasses = examClasses;
            Capacity = capacity;
        }
        public void AddToCell(ExamClass examClass)
        {
            ExamClasses.Add(examClass);
            TotalStudent += examClass.Count;
            if(TotalStudent > Capacity)
            {
                Console.WriteLine("Canh Bao, QUA TAI SINH VIEN");
            }
        }
        public void Clear()
        {
            ExamClasses.Clear();
        }
        public bool IsEmpty()
        {
            return ExamClasses.Count == 0;
        }
        // Unsafe, used after having checked empty
        public List<Course> GetCourses()
        {
            return ExamClasses.Select(examClass => examClass.StudyClass.Course).ToList() ;
        }
    }

    public class ExamSchedule : IEnumerable<ArrayCell?>
    {
        public DateOnly[] dates;
        public int[] datesAsInt;
        public int[] shifts;
        public DateOnly GetDateOfIndex(int x)
        {
            try
            {
                return dates[x];
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Error out of range datetime");
                throw;
            }
        }
        public Room[] rooms;
        public Room GetRoomOfIndex(int x)
        {
            try
            {
                return rooms[x];
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Error out of range roomIndex");
                throw;
            }
        }

        public Dictionary<Room, int> roomToRoomIndex;
        public Dictionary<DateOnly, int> dateOnlyToDayIndex;
        // All the data of the schedule is stored here.
        private ArrayCell?[][][] date_Shift_RoomTable;
        // Get and set array cell.
        protected ArrayCell? this[int dayIndex, int shiftIndex, int roomIndex]
        {
            get { return date_Shift_RoomTable[dayIndex][shiftIndex][roomIndex]; }
            set { date_Shift_RoomTable[dayIndex][shiftIndex][roomIndex] = value; }
        }

        protected ArrayCell[] this[int dayIndex, int shiftIndex]
        {
            get { return date_Shift_RoomTable[dayIndex][shiftIndex]; }
        }

        protected ArrayCell? this[int dayIndex, int shiftIndex, Room room]
        {
            get {
                try 
                { 
                    return date_Shift_RoomTable[dayIndex][shiftIndex][roomToRoomIndex[room]]; 
                }
                catch (KeyNotFoundException E ) 
                {
                    Console.WriteLine($"Room id: {room.RoomId}, hash: {room.GetHashCode()}");
                    string id = room.RoomId;
                    foreach (var item in roomToRoomIndex)
                    {
                        if(item.Key.RoomId == id)
                        {
                            Console.WriteLine($"found the same room ID in the dictionary, hash :{item.Key.GetHashCode()}");
                        }
                    }
                    throw E;
                }
            }
            set { date_Shift_RoomTable[dayIndex][shiftIndex][roomToRoomIndex[room]] = value; }
        }


        /// <summary>
        /// Create new instance of ShiftDateRoomArray !!!ROOMS SHOULD BE SORTED!!!
        /// </summary>
        /// <param name="dates">all the available dates</param>
        /// <param name="rooms">all the available rooms</param>
        public ExamSchedule(DateOnly[] dates, int[] shifts,Room[] rooms)
        {
            this.dates = dates;
            this.rooms = rooms;
            this.shifts = shifts;
            roomToRoomIndex = new Dictionary<Room, int>();
            dateOnlyToDayIndex = new();
            for (int i = 0; i < rooms.Length; i++)
            {
                roomToRoomIndex.Add(rooms[i], i);
            }
            for (int i = 0; i < dates.Length; i++)
            {
                dateOnlyToDayIndex.Add(dates[i], i);
            }
            var temp1 = new ArrayCell[dates.Length][][];
            for (int i = 0; i < dates.Length; i++)
            {
                var temp2 = new ArrayCell[shifts.Length][];
                for (int j = 0; j < shifts.Length; j++)
                {
                    var temp3 = new ArrayCell[rooms.Length];
                    temp2[j] = temp3;
                }
                temp1[i] = temp2;
            }
            date_Shift_RoomTable = temp1;
            InitSchedule();
        }

        private void InitSchedule()
        {
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    foreach (var room in rooms)
                    {
                        var cell = this[date, shift, room]= new ArrayCell(new List<ExamClass>(), room.Capacity);
                        cell.Capacity = room.Capacity;
                    }
                }
            }
        }

        public ArrayCell? GetCell(int dayIndex, int shiftIndex, Room roomIndex)
        {
            return this[dayIndex, shiftIndex, roomToRoomIndex[roomIndex]];
        }

        public ArrayCell? GetCell(DateOnly dayIndex, int shiftIndex, Room roomIndex)
        {
            return this[dateOnlyToDayIndex[dayIndex], shiftIndex, roomToRoomIndex[roomIndex]];
        }

        public ArrayCell? GetCell(int dayIndex, int shiftIndex, int roomIndex)
        {
            return this[dayIndex, shiftIndex, roomIndex];
        }

        public ArrayCell[] GetCellAtDateAndShift(int dayIndex, int shiftIndex)
        {
            return this[dayIndex,shiftIndex];
        }

        public ArrayCell[] GetCellAtDateAndShift(DateOnly dateOnly, int shiftIndex)
        {
            return this[dateOnlyToDayIndex[dateOnly], shiftIndex];
        }

        public void AddToThisCell(int dayIndex, int shiftIndex, Room roomIndex, ExamClass examClass)
        {
            var cell = this[dayIndex, shiftIndex, roomIndex];
            cell.AddToCell(examClass);
        }

        public void AddToThisCell(int dayIndex, int shiftIndex, Room roomIndex, IEnumerable<ExamClass> examClasses)
        {
            var cell = this[dayIndex, shiftIndex, roomIndex];
            foreach (var item in examClasses)
            {
                cell.AddToCell(item);
            }
        }

        public void AddToThisCell(DateOnly dayIndex, int shiftIndex, Room roomIndex, IEnumerable<ExamClass> examClasses)
        {
            var cell = this[dateOnlyToDayIndex[dayIndex], shiftIndex, roomIndex];
            foreach (var item in examClasses)
            {
                cell.AddToCell(item);
            }
        }

        public void AddToThisCell(DateOnly dayIndex, int shiftIndex, Room roomIndex, ExamClass examClass)
        {
            var cell = this[dateOnlyToDayIndex[dayIndex], shiftIndex, roomIndex];
            cell.AddToCell(examClass);
        }


        public void EmptyThisCell(int dayIndex, int shiftIndex, Room roomIndex)
        {
            this[dayIndex, shiftIndex, roomIndex].ExamClasses = new List<ExamClass>();
        }

        public bool IsCellEmpty(int dayIndex, int shiftIndex, Room room)
        {
            return this[dayIndex, shiftIndex, room].ExamClasses.Count == 0;
        }


        public void ChangeCellCapacity(int dayIndex, int shiftIndex, Room roomIndex, int Capacity)
        {
            this[dayIndex,shiftIndex,roomIndex].Capacity = Capacity;
        }

        public void SwapTwoSlotInSchedule((int date, int shift) slotPosition1, (int date,int shift) slotPosition2)
        {
            var (date_1, shift_1) = slotPosition1;
            var (date_2, shift_2) = slotPosition2;
            (date_Shift_RoomTable[date_2][shift_2], date_Shift_RoomTable[date_1][shift_1]) = (date_Shift_RoomTable[date_1][shift_1], date_Shift_RoomTable[date_2][shift_2]);
        }

        public int CountAllClasses()
        {
            int count = 0;
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift <  shifts.Length; shift++)
                {
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = this[date, shift, room];
                        if (thisCell == null)
                            continue;
                        if (thisCell.ExamClasses.Count >= 1)
                        {
                            foreach (var examClass in thisCell.ExamClasses)
                            {
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public IEnumerator<ArrayCell?> GetEnumerator()
        {
            for (int i = 0; i < dates.Length; i++)
            {
                for (int j = 0; j < shifts.Length; j++)
                {
                    for (int k = 0; k < rooms.Length; k++)
                    {
                        yield return this[i, j, k];
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
