using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.Tests.TestObject;

namespace SapLichThiLib.AlgorithmsObjects.AnnealingOptimizations
{
    internal class AnealingOptimizationForSameShiftCourses : IAlgorithmObject
    {
        private Dictionary<Course, List<((int date, int shift) slot, int count)>> P_optimize_table { get; set; }
        
        public ExamSchedule I_schedule { get; set; }
        public void InitializeAllInternals()
        {
            P_optimize_table = new();
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    Dictionary<Course, int> examClassCountForEachShift = new();
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        var count = 0;
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            Course thisCourse = examClass.StudyClass.Course;
                            examClassCountForEachShift.TryAdd(thisCourse, new());
                            examClassCountForEachShift[thisCourse] += 1;
                        }
                    }

                    foreach (var (course,value) in examClassCountForEachShift)
                    {
                        P_optimize_table.TryAdd(course, new());
                        P_optimize_table[course].Add(((date, shift), value));
                    }
                }
            }
        }
        public void Run()
        {
            CheckAllInput();
            InitializeAllInternals();
            InitializeAllOutput();
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            if(I_schedule == null)
                throw new InvalidDataException();
        }

        public void InitializeAllOutput()
        {
            return;
        }

        public void ProcedureRun()
        {
            throw new NotImplementedException();
        }

        public void Solver1(Course course)
        {
            var slots = P_optimize_table[course];
            //slots = P_optimize_table[cou]
        }
        //private Dictionary<Course, List<()>>
    }
}
