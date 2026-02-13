using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class SlotInfoMapper
    {
        public ExamSchedule I_schedule { get; set; }
        public List<Curriculum> I_curricula { get; set; }
        public List<ExamGroup> I_hardRails { get; set; }
        public List<StudentYear> I_studentYears { get; set; }
        public Dictionary<Course, List<(int date, int shift)>> O_course_slots { get; set; } = new();
        public Dictionary<(int date, int shift), List<Course>> O_slot_courses { get; set; } = new();
        public Dictionary<(int date, int shift), StudentYear> O_slot_largestYears { get; set; } = new();
        public Dictionary<(int date, int shift), int> O_slot_largestYearCount { get; set; } = new();
        public Dictionary<(int date, int shift), bool> O_slot_movability { get; set; } = new();
        // Not sure to implement this
        // private List<(int date, int shift)> P_movableDateShift { get; set; } = new(); 
        public Dictionary<Course, StudentYear> I_course_mainStudentYear { get; set; }
        private (int date, int shift)[,] P_positionChangedArray { get; set; }
        public void Run()
        {
            int dateLength = I_schedule.dates.Length;
            int shiftLength = I_schedule.shifts.Length;
            int roomLength = I_schedule.rooms.Length;
            P_positionChangedArray = new (int date, int shift)[dateLength, shiftLength];
            for (int date = 0; date < dateLength; date++)
            {
                for (var shift = 0; shift < shiftLength; shift++)
                {

                    Dictionary<StudentYear, int> year_examClassCount = new();
                    foreach (StudentYear studentYear in I_studentYears)
                    {
                        year_examClassCount.Add(studentYear, 0);
                    }
                    HashSet<Course> courses = new HashSet<Course>();
                    for (int room = 0; room < roomLength; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell == null)
                        {
                            continue;
                        }
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            var thisCourse = examClass.StudyClass.Course;
                            year_examClassCount[I_course_mainStudentYear[thisCourse]] += 1;
                            O_course_slots.TryAdd(thisCourse, new List<(int date, int shift)>());
                            if (!O_course_slots[thisCourse].Contains((date, shift)))
                                O_course_slots[thisCourse].Add((date, shift));
                            courses.Add(thisCourse);
                        }
                    }
                    O_slot_courses.TryAdd((date, shift), courses.ToList());
                    O_slot_movability.TryAdd((date, shift), true);
                    P_positionChangedArray[date, shift] = (date, shift);
                    if (year_examClassCount.Count > 0)
                    {
                        var (largestYear, count) = year_examClassCount.MaxBy(x => x.Value);
                        O_slot_largestYears.Add((date, shift), largestYear);
                        O_slot_largestYearCount.Add((date, shift), count);
                    }
                    string s = year_examClassCount.Select(x => x.Key.Name + " " + x.Value).Aggregate((x, y) => x + " - " + y);
                    Console.Write(s);
                    Console.WriteLine($"  main: {O_slot_largestYears[(date, shift)].Name}, count : {O_slot_largestYearCount[(date, shift)]}, index:{P_positionChangedArray[date, shift]}");
                }
            }

            // Eliminate all hardrail courses from moving
            foreach (ExamGroup hardRail in I_hardRails)
            {
                foreach (var course in hardRail.Courses)
                {
                    if (!O_course_slots.ContainsKey(course))
                    {
                        Console.WriteLine($"Course {course.ID} not found in I_course_slots.");
                        continue;
                    }
                    foreach (var (date, shift) in O_course_slots[course])
                    {
                        O_slot_movability[(date, shift)] = false;
                        Console.WriteLine($"Marking slot ({date}, {shift}) for course {course.ID} as non-movable.");
                    }
                }
            }

            // Not sure to implement this, should only be revised when performance is inadequate.
            //P_movableDateShift = P_slot_movability.Where(x => x.Value == true).Select(x => x.Key).ToList();
        }
    }
}
