using CsvHelper;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataStructures
{
    public class ShiftArrayCell
    {
        public StudyClass StudyClass { get; set; }
        public List<ExamClass> ExamClasses { get; set; }
        public Shift ThisCellShift { get; set; }
        public DateOnly ThisCellDateOnly { get; set; }
        public Room ThisCellRoom { get; set; }
        public ShiftArrayCell(StudyClass studyClass,List<ExamClass> examClasses)
        {
            StudyClass = studyClass;
            ExamClasses = examClasses;
        }
        public ShiftArrayCell(StudyClass studyClass, List<ExamClass> examClasses, Shift thisShift, int thisDayNum, Room thisRoom) : this(studyClass, examClasses)
        {
            ThisCellShift = thisShift;
            ThisCellDateOnly = DateOnly.FromDayNumber(thisDayNum);
            ThisCellRoom = thisRoom;
        }
        public ShiftArrayCell(StudyClass studyClass, List<ExamClass> examClasses, Shift thisShift, DateOnly thisDayNum, Room thisRoom) : this(studyClass, examClasses)
        {
            ThisCellShift = thisShift;
            ThisCellDateOnly = thisDayNum;
            ThisCellRoom = thisRoom;
        }
    }
    public class ShiftDateRoomArray
    {
        public DateOnly[] dates;
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
        public Dictionary<Shift, ShiftArrayCell?[,]> courseAndShiftTable;
        public ShiftArrayCell? this[Shift shiftIndex,int dayIndex, int roomIndex]
        {
            get { return courseAndShiftTable[shiftIndex][dayIndex, roomIndex]; }
            set { courseAndShiftTable[shiftIndex][dayIndex, roomIndex] = value; }
        }

        public ShiftArrayCell? this[Shift shiftIndex, int dayIndex, Room room]
        {
            get { return courseAndShiftTable[shiftIndex][dayIndex, roomToRoomIndex[room]]; }
            set { courseAndShiftTable[shiftIndex][dayIndex, roomToRoomIndex[room]] = value; }
        }

        public ShiftArrayCell?[,] this[Shift shiftIndex]
        {
            get { return courseAndShiftTable[shiftIndex]; }
            set { courseAndShiftTable[shiftIndex] = value; }
        }

        /// <summary>
        /// Create new instance of ShiftDateRoomArray !!!ROOMS SHOULD BE SORTED!!!
        /// </summary>
        /// <param name="dates">all the available dates</param>
        /// <param name="rooms">all the available rooms</param>
        public ShiftDateRoomArray(DateOnly[] dates, Room[] rooms)
        {
            this.dates = dates;
            this.rooms = rooms;
            roomToRoomIndex = new Dictionary<Room, int>();
            for (int i = 0; i < rooms.Length; i++)
            {
                roomToRoomIndex.Add(rooms[i], i);
            }
            courseAndShiftTable = new()
            {
                { Shift.shift1, new ShiftArrayCell?[dates.Length, rooms.Length] },
                { Shift.shift2, new ShiftArrayCell?[dates.Length, rooms.Length] },
                { Shift.shift3, new ShiftArrayCell?[dates.Length, rooms.Length] },
                { Shift.shift4, new ShiftArrayCell?[dates.Length, rooms.Length] },
                { Shift.shift5, new ShiftArrayCell?[dates.Length, rooms.Length] }
            };
        }
        public bool MakeCellNull(Shift shiftIndex, int dayIndex, Room roomIndex)
        {
            if (IsThisCellOccupied(shiftIndex, dayIndex, roomIndex))
            {
                this[shiftIndex, dayIndex, roomIndex] = null;
                return true;
            }
            return false;
        }    

        public bool IsThisCellOccupied(Shift shiftIndex, int dayIndex, int roomIndex)
        {
            return courseAndShiftTable[shiftIndex][dayIndex, roomIndex] is not null;
        }


        public bool IsThisCellOccupied(Shift shiftIndex, int dayIndex, Room room)
        {
            return courseAndShiftTable[shiftIndex][dayIndex, roomToRoomIndex[room]] is not null;
        }
        public int CountAllClasses()
        {
            int count = 0;
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = Shift.shift1; shift <= Shift.shift5; shift++)
                {
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = this[shift, date, room];
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
    }
}
