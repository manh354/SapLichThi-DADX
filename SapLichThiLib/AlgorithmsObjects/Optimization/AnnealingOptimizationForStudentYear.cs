using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.AnnealingOptimizations
{
    public class AnnealingOptimizationForStudentYear : IAlgorithmObject
    {
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<StudentYear, int> I_studentYear_priorityShift { get; set; }
        public List<ExamGroup> I_examGroups { get; set; }
        private Dictionary<Course, List<(int date, int shift)>> P_course_slots { get; set; } = new();
        private Dictionary<Course, StudentYear> P_course_mainStudentYear { get; set; }
        private Dictionary<(int date, int shift), StudentYear?> P_slot_largestYears { get; set; } = new();
        private Dictionary<(int date, int shift), int> P_slot_classCount { get; set; } = new();
        public List<StudentYear> I_studentYears { get; set; }
        private Dictionary<(int date, int shift), bool> P_slot_movability { get; set; } = new();
        private int largeAndMediumRoomCount { get; set; }
        // Not sure to implement this
        // private List<(int date, int shift)> P_movableDateShift { get; set; } = new(); 
        private Dictionary<Course, int> P_course_examClassCount { get; set; } = new();
        private (int date, int shift)[,] P_positionChangedArray { get; set; }
        public int I_markovChain_length { get; set; } = 100;
        public float I_temperature { get; set; } = 1f;
        public float I_terminate_temperature { get; set; } = 0.001f;
        public float I_temperature_decrement { get; set; } = 0.99f;
        public void Initialize()
        {
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            int dateLength = dates.Length;
            int shiftLength = shifts.Length;
            int roomLength = rooms.Length;
            P_positionChangedArray = new (int date, int shift)[dateLength, shiftLength];
            for (int date = 0; date < dateLength; date++)
            {
                for (var shift = 0; shift < shiftLength; shift++)
                {
                    P_slot_classCount.Add((date, shift), 0);
                    Dictionary<StudentYear, int> largestYear = new();
                    foreach(StudentYear studentYear in I_studentYears)
                    {
                        largestYear.Add(studentYear, 0);
                    }
                    for (int room = 0; room < roomLength; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        var count = 0;
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            var thisCourse = examClass.StudyClass.Course;
                            largestYear[examClass.StudyClass.StudentYear] += 1;
                            P_course_slots.TryAdd(thisCourse, new List<(int date, int shift)>());
                            if (!P_course_slots[thisCourse].Contains((date, shift)))
                                P_course_slots[thisCourse].Add((date, shift));
                            P_slot_classCount[(date, shift)] += 1;
                        }
                    }
                    P_slot_movability.TryAdd((date, shift), true);
                    if(largestYear.Count > 0)
                    {
                        P_slot_largestYears.Add((date, shift), largestYear.MaxBy(x => x.Value).Key);
                    }
                    else
                    {
                        P_slot_largestYears.Add((date, shift), null);
                    }
                    P_positionChangedArray[date, shift] = (date, shift);
                }
            }

            foreach (ExamGroup examGroup in I_examGroups)
            {
                foreach (var course in examGroup.Courses)
                {
                    if (!P_course_slots.ContainsKey(course))
                        continue;
                    foreach (var (date, shift) in P_course_slots[course])
                    {
                        P_slot_movability[(date, shift)] = false;
                    }
                }
            }
        }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            Initialize();
#if DEBUG
            var dates = I_schedule.dates;
            for (int i = 0; i < dates.Length; i++)
            {
                int[] index = CountPositionalDatas(i);
                Console.WriteLine(index.Select(x=>x.ToString()).Aggregate((x,y) => x+" "+y));
            }
#endif
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            if (I_schedule == null)
                throw new InvalidDataException();
        }

        public void InitializeAllOutput()
        {
            
        }

        public void ProcedureRun()
        {
            OptimizeTable();
            MakeChangesToSchedule();
        }

        private void MakeChangesToSchedule()
        {
            for (int date = 0; date < P_positionChangedArray.GetLength(0); date++)
            {
                for (int shift = 0; shift < P_positionChangedArray.GetLength(1); shift++)
                {
                    var (date_2, shift_2) = P_positionChangedArray[date, shift];
                    I_schedule.SwapTwoSlotInSchedule((date, shift), (date_2, shift_2));
                }
            }
        }

        private void FindNeighboringSolutionForSeperationPoint(int date, out (int date, int shift) out_position_1, out (int date, int shift) out_position_2)
        {
            // Pick at random 2 entries of the table, then try to swap and calculate the solution values.
            var (position_1, year_1) = P_slot_largestYears.Where(x=>x.Key.date == date).PickRandomFromIEnumerable();
            while (!P_slot_movability[position_1])
            {
                (position_1, year_1) = P_slot_largestYears.Where(x => x.Key.date == date).PickRandomFromIEnumerable();
            }
            var (position_2, year_2) = P_slot_largestYears.Where(x=>x.Key.date == position_1.date).Where(x=>x.Key.shift != position_1.shift).PickRandomFromIEnumerable();
            while (position_1 == position_2 || !P_slot_movability[position_2])
            {
                (position_2, year_2) = P_slot_largestYears.Where(x => x.Key.date == position_1.date).Where(x => x.Key.shift != position_1.shift).PickRandomFromIEnumerable();
            }
            // We swap places of courses_1 and courses_2 here
            if (!P_slot_movability[position_1] || !P_slot_movability[position_2])
                Console.WriteLine("WTH");
            SwapPositionOfDataOfSeperationPoint(position_1, position_2);
            out_position_1 = position_1;
            out_position_2 = position_2;
        }

        private int[] CountPositionalDatas(int date)
        {
            var shifts = I_schedule.shifts;
            int[] indexCount = new int[I_studentYears.Count];
            for (int i = 0; i < I_studentYears.Count; i++)
            {
                for (int shift = 0; shift < shifts.Length; shift++)
                {
                    if (P_slot_largestYears[(date, shift)] == I_studentYears[i])
                        indexCount[i] += 1;
                }
            }
            return indexCount;
        }

        private void SwapPositionOfDataOfSeperationPoint((int date, int shift) position_1, (int date, int shift) position_2)
        {
            var year_1 = P_slot_largestYears[position_1];
            var year_2 = P_slot_largestYears[position_2];
            P_slot_largestYears[position_1] = year_2;
            P_slot_largestYears[position_2] = year_1;
            P_positionChangedArray[position_1.date, position_1.shift] = position_2;
            P_positionChangedArray[position_2.date, position_2.shift] = position_1;
        }

        private double ProbabilityFunction(int oldPoint, int newPoint, double temperature)
        {
            return Math.Exp((oldPoint - newPoint) / temperature);
        }

        public void OptimizeTable()
        {
            int currentSolutionPoint = EvaluateCurrentSolutionForSeperativePoint(out int curr_studentPoint);
            Console.WriteLine("Current solution Val : {0}, consecutive : {1}", currentSolutionPoint, curr_studentPoint);

            for (int date = 0; date < I_schedule.dates.Length; date++)
            {
                I_temperature = 1;
                while (I_temperature > I_terminate_temperature)
                {
                    // var stepCount = StepCountFunction(I_temperature);
                    for (int i = 0; i < I_markovChain_length; i++)
                    {
                        // FindAnotherSolutionForSeperationPointMultipleSteps(stepCount, out var out_positions_1, out var out_positions_2);
                        // DISCARDED function
                        (int, int) out_position_1, out_position_2;
                        /*if (RandomExtension.ChoosePropability(guidedPropability))
                            FindNeighboringrSolutionForSeperationPoint_GUIDED(out out_position_1, out out_position_2);
                        else*/


                        FindNeighboringSolutionForSeperationPoint(date, out out_position_1, out out_position_2);
                        int nextSolutionPoint = EvaluateCurrentSolutionForSeperativePoint(out int next_consecutivePoint);
                        // TODO: MAKE FUNCITON
                        if (nextSolutionPoint < currentSolutionPoint)
                        {
                            Console.WriteLine("Better Solution Chosen by swapping {0} -> {1} ", out_position_1, out_position_2);
                            currentSolutionPoint = nextSolutionPoint;
                        }
                        else
                        {
                            var probabiltyValue = ProbabilityFunction(currentSolutionPoint, nextSolutionPoint, I_temperature);
                            bool chooseTheSolution = RandomExtension.ChooseProbability(probabiltyValue);
                            if (chooseTheSolution)
                            {
                                if (probabiltyValue < 1)
                                    Console.WriteLine("Worse Solution Chosen at : {0} propbability", probabiltyValue);
                                currentSolutionPoint = nextSolutionPoint;
                            }
                            else
                            {
                                // DISCARDED function
                                RollBackSolution(out_position_1, out_position_2);
                                //RollBackSolutionMultipleStep(stepCount, out_positions_1, out_positions_2);
                            }

                        }
                        // END TODO
                    }
                    Console.WriteLine("Current solution Val : {0}, temperature {1}", currentSolutionPoint, I_temperature);
                    I_temperature *= I_temperature_decrement;
                }
            }
        }

        private int EvaluateCurrentSolutionForSeperativePoint(out int out_sumYearPoint)
        {
            // Check if a course is divided into 2 dates, or not in consecutive shifts.
            out_sumYearPoint = 0;
            foreach(var (slot,year ) in P_slot_largestYears)
            {
                if(slot.shift != I_studentYear_priorityShift[year])
                {
                    out_sumYearPoint += P_slot_classCount[slot];
                }
            }
            return out_sumYearPoint;
        }

        private void RollBackSolution((int date, int shift) position_1, (int date, int shift) position_2)
        {
            var courses_1 = P_slot_largestYears[position_1];
            var courses_2 = P_slot_largestYears[position_2];
            // We swap places of courses_1 and courses_2 here
            P_slot_largestYears[position_1] = courses_2;
            P_slot_largestYears[position_2] = courses_1;
            P_positionChangedArray[position_1.date, position_1.shift] = position_2;
            P_positionChangedArray[position_2.date, position_2.shift] = position_1;
        }
    }
}
