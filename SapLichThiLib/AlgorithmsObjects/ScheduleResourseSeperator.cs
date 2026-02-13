using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public struct PartialEmptySlot
    {
        public int Date { get; set; }
        public int Shift { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Course> Courses { get; set; }
    }
    public struct EmptySlot
    {
        public int Date { get; set; }
        public int Shift { get; set; }
    }
    public class ScheduleResourcesSeperator : IAlgorithmObject
    {
        public ExamSchedule I_schedule { get; set; }
        public List<EmptySlot> O_emptySlots { get; set; }
        public List<PartialEmptySlot> O_partialEmptySlots { get; set; } 
        public Dictionary<Course, List<PartialEmptySlot>> O_commonCourse_partialEmptySlots { get; set; }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
        }


        public void CheckAllInput()
        {
            if(I_schedule == null)
            {
                throw new Exception(GetType().ToString() + "Not properly initialized.");
            }
        }

        public void InitializeAllOutput()
        {
            O_emptySlots = new();
            O_partialEmptySlots = new();
            O_commonCourse_partialEmptySlots = new();
        }
        public void ProcedureRun()
        {
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;

            O_emptySlots = new();
            O_partialEmptySlots = new();

            foreach(var date in dates)
            {
                foreach (var shift in shifts)
                {
                    
                    foreach (var room in rooms)
                    {
                        var cell = I_schedule.GetCell(date, shift, room);
                        if (!cell.IsEmpty())
                        {
                            goto PARTIALLY_EMPTY;
                        }
                    }
                    O_emptySlots.Add(new() { Date = I_schedule.dateOnlyToDayIndex[date], Shift = shift });

                    continue;

                    PARTIALLY_EMPTY:
                    PartialEmptySlot partialSlot = new();
                    List<Room> allEmptyRooms = new List<Room>();
                    HashSet<Course> theseCourses = new();
                    foreach (var room in rooms)
                    {
                        var cell = I_schedule.GetCell(date, shift, room);
                        if (!cell.IsEmpty())
                        {
                            List<Course> thisCourses = cell.GetCourses();
                            foreach (var thisCourse in thisCourses)
                            {
                                theseCourses.Add(thisCourse);
                            }
                        }
                        else
                        {
                            allEmptyRooms.Add(room);
                        }
                    }
                    partialSlot.Date = I_schedule.dateOnlyToDayIndex[date];
                    partialSlot.Shift = shift;
                    partialSlot.Rooms = allEmptyRooms;
                    partialSlot.Courses = theseCourses.ToList();
                    foreach (var thisCourse in theseCourses)
                    {
                        if (O_commonCourse_partialEmptySlots.ContainsKey(thisCourse))
                        {
                            O_commonCourse_partialEmptySlots[thisCourse].Add(partialSlot);
                        }
                        else
                        {
                            O_commonCourse_partialEmptySlots.Add(thisCourse, new[] { partialSlot }.ToList());
                        }
                    }
                    O_partialEmptySlots.Add(partialSlot);
                }
            }
        }
    }
}
