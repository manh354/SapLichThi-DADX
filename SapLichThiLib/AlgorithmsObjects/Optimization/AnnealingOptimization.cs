using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.ErrorAndLog;
using SapLichThiLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SapLichThiLib.AlgorithmsObjects.AnnealingOptimizations
{
    public class AnnealingOptimization : IAlgorithmObject
    {
        public ExamSchedule I_schedule { get; set; }
        public List<Curriculum> I_curricula { get; set; }
        public List<ExamGroup> I_hardRails { get; set; }
        public Dictionary<School, int> I_school_teachersCount { get; set; }
        public List<StudentYear> I_studentYears { get; set; }
        public Dictionary<StudentYear, int> I_studentYear_prioritizedShift { get; set; }
        public Dictionary<Course, StudentYear> I_course_mainStudentYear { get; set; }
        public double I_temperature { get; set; } = 1f;
        public double I_temperature_decrement { get; set; } = 0.99;
        public double I_terminate_temperature { get; set; } = 0.05;
        public int I_markovChain_length { get; set; } = 50;
        public int O_iterationCount { get; set; }
        public double CalculatePoints()
        {
            throw new NotImplementedException();
            var seperationPoint = CalculateSeperationPoint();
            var teacherPoint = CalculateTeachersCountPoint();
            var sameDatePoint = CalculateSameDayCoursePoint();
            return seperationPoint + teacherPoint + sameDatePoint;
        }
        public double CalculateSameDayCoursePoint()
        {
            throw new NotImplementedException();
        }
        public double CalculateSeperationPoint()
        {
            throw new NotImplementedException();
        }
        public double CalculateTeachersCountPoint()
        {
            throw new NotImplementedException();
        }

        public Dictionary<Course, List<(int date,int shift)>> I_course_slots { get; set; } = new();
        public Dictionary<(int date, int shift), List<Course>> I_slot_courses { get; set; } = new();
        public Dictionary<(int date, int shift), StudentYear> I_slot_largestYears { get; set; } = new();
        public Dictionary<(int date, int shift), int> I_slot_largestYearCount { get; set; } = new();
        public Dictionary<(int date, int shift), bool> I_slot_movability { get; set; } = new();
        private int largeAndMediumRoomCount { get; set; } 
        // Not sure to implement this
        // private List<(int date, int shift)> P_movableDateShift { get; set; } = new(); 
        private Dictionary<Course, int> P_course_examClassCount { get; set; } = new();
        private (int date,int shift)[,] P_positionChangedArray { get; set; }
        public void Inittialize()
        {
            int dateLength = I_schedule.dates.Length;
            int shiftLength = I_schedule.shifts.Length;
            int roomLength = I_schedule.rooms.Length;
            P_positionChangedArray = new (int date, int shift)[dateLength, shiftLength];
            largeAndMediumRoomCount = I_schedule.rooms.Sum(x => x.RoomType == RoomType.medium ? 1 : x.RoomType == RoomType.large ? 2 : 0);
            P_allIsSmallerThanSmallestCorrectShift = new bool[I_studentYears.Count];
            P_allShiftIsCorrectlyFull = new bool[I_studentYears.Count];
            for (int date = 0; date < dateLength; date++)
            {
                for (var shift = 0; shift < shiftLength; shift++)
                {

                    Dictionary<StudentYear, int> year_examClassCount = new();
                    foreach (StudentYear studentYear in I_studentYears)
                    {
                        year_examClassCount.Add(studentYear, 0);
                    }
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
                            P_course_examClassCount.TryAdd(thisCourse, 0);
                            P_course_examClassCount[thisCourse] += 1;
                        }
                    }
                    P_positionChangedArray[date, shift] = (date,shift);
                }
            }
        }

        private List<Course> P_courseOrderedByExamClassCount { get; set; }
        private int P_shiftMax;
        private void InitializeGuildline()
        {
            P_courseOrderedByExamClassCount = P_course_examClassCount.OrderBy(x=>x.Value).Select(x=> x.Key).ToList();
            P_shiftMax = I_schedule.shifts.Length - 1;
        }

        private (int date, int shift) FindRandomNeighborSlot(List<(int date, int shift)> positions)
        {
            bool chooseEarlyShift = RandomExtension.ChooseProbability(0.5);
            if (positions.Count == 1)
            {
                return FindRandomNeighborSlot(positions[0]);
            }

            // Position count >=  2
            var (dateOpt2, shiftF) = positions[0]; // Date first and shift first
            var (_, shiftL) = positions[positions.Count - 1]; // date last and shift last
            if(shiftL - shiftF == P_shiftMax)
            {
                return (-1, -1);
            }
            if(chooseEarlyShift)
            {
                if(shiftF != 0)
                {
                    return (dateOpt2, shiftF - 1);
                }
                else
                {
                    return (dateOpt2, shiftL + 1);
                }
            }
            
            else
            {
                if(shiftL != P_shiftMax)
                {
                    return (dateOpt2, shiftL + 1);
                }
                else
                {
                    return (dateOpt2, shiftF - 1);
                }
            }

        }

        private (int date, int shift) FindRandomNeighborSlot((int date, int shift) position)
        {
            bool chooseEarlyShift = RandomExtension.ChooseProbability(0.5);
            var (dateOpt1, shift) = position;
            if (shift == P_shiftMax)
                return (dateOpt1, P_shiftMax - 1);
            if (shift == 0)
                return (dateOpt1, 0);
            if (chooseEarlyShift)
                return (dateOpt1, shift - 1);
            else
                return (dateOpt1, shift + 1);
        }

        private void FindNeighboringrSolutionForSeperationPoint_GUIDED(out (int date, int shift) out_position_1, out (int date, int shift) out_position_2)
        {
            out_position_1 = (-1, -1);
            out_position_2 = (-1, -1);
            for (int i = 0; i < P_courseOrderedByExamClassCount.Count; i++)
            {
                Course course = P_courseOrderedByExamClassCount[i];
                var positions = I_course_slots[course];
                if (positions.Count <= 1)
                    continue;
                var (date_1, shift_1) = positions[0];
                var invalidDateFound = false;
                var invalidShiftFound = false;
                var (date_2, shift_2) = (-1, -1);
                for (int j = 1; j < positions.Count; j++)
                {
                    (date_2, shift_2) = positions[j];
                    if (date_1 == date_2)
                        continue;
                    invalidDateFound = true;
                    break;
                }
                if (!invalidDateFound)
                {
                    var shifts = positions.Select(x => x.shift).OrderBy(x => x).ToList();
                    for (int k = 1; k < positions.Count; k++)
                    {
                        if (shifts[k] - shifts[k - 1] != 1)
                        {
                            (date_2, shift_2) = (date_1, shifts[k]);
                            invalidShiftFound = true;
                            break;
                        }
                    }
                }
                if (invalidDateFound)
                {
                    (int date, int shift) position_1, position_2;
                    var positions_1 = positions.Where(x => x.date == date_1).ToList();
                    var positions_2 = positions.Where(x => x.date == date_2).ToList();
                    if(positions_1.Count >= positions_2.Count)
                    {
                        position_1 = FindRandomNeighborSlot(positions_1);
                        position_2 = positions_2[0];
                    }
                    else
                    {
                        position_1 = positions_1[0];
                        position_2 = FindRandomNeighborSlot(positions_2);
                    }
                    
                    var courses_1 = I_slot_courses[position_1];
                    var courses_2 = I_slot_courses[position_2];
                    SwapPositionOfDataOfSeperationPoint(position_1, position_2);
                    out_position_1 = position_1;
                    out_position_2 = position_2;
                    return;
                }
            }
        }

        private void SwapPositionOfDataOfSeperationPoint((int date, int shift) position_1, (int date, int shift) position_2)
        {
            var courses_1 = I_slot_courses[position_1];
            var courses_2 = I_slot_courses[position_2];
            I_slot_courses[position_1] = courses_2;
            I_slot_courses[position_2] = courses_1;
            foreach (var course_in_1 in courses_1)
            {
                I_course_slots[course_in_1].Remove(position_1);
                I_course_slots[course_in_1].Add(position_2);
            }
            foreach (var course_in_2 in courses_2)
            {
                I_course_slots[course_in_2].Remove(position_2);
                I_course_slots[course_in_2].Add(position_1);
            }
            (I_slot_largestYears[position_1], I_slot_largestYears[position_2]) = (I_slot_largestYears[position_2], I_slot_largestYears[position_1]);
            (I_slot_largestYearCount[position_1], I_slot_largestYearCount[position_2]) = (I_slot_largestYearCount[position_2], I_slot_largestYearCount[position_1]);
            (P_positionChangedArray[position_1.date, position_1.shift], P_positionChangedArray[position_2.date, position_2.shift])
                = (P_positionChangedArray[position_2.date, position_2.shift], P_positionChangedArray[position_1.date, position_1.shift]);
        }

        bool allRemainingMisplacedShiftIsSmallerThanSmallestCorrectShift = false;
        bool allShiftIsCorrectlyFull = false;
        bool[] P_allShiftIsCorrectlyFull { get; set; }
        bool[] P_allIsSmallerThanSmallestCorrectShift { get; set; }
        private void FindNeighboringSolutionForSeperationPoint(out (int date, int shift) out_position_1, out (int date, int shift) out_position_2, out bool improvePlacement)
        {
            (int date, int shift) position_1 = (-1,-1);
            (int date, int shift) position_2;
            List<Course> courses_1;
            List<Course> courses_2;
            IEnumerable<KeyValuePair<(int date, int shift), List<Course>>> listForPosition2 = new List<KeyValuePair<(int date, int shift), List<Course>>>();
            improvePlacement = false;
            var allAvailableSlot = I_slot_courses.Where(x => I_slot_movability[x.Key]);

            CASE1:
            if (!allShiftIsCorrectlyFull)
            {
                StudentYear? consideringStudentYear = null;
                for (int i = 0; i < I_studentYears.Count; i++)
                {
                    if (!P_allShiftIsCorrectlyFull[i])
                    {
                        consideringStudentYear = I_studentYears[i];
                        break;
                    }
                }
                if(consideringStudentYear == null)
                {
                    allShiftIsCorrectlyFull = true;
                    Console.WriteLine("END CASE 1, no more incorrect shift found");
                    goto CASE2;
                }
                var prioritizedShiftForThisStudentYear = I_studentYear_prioritizedShift[consideringStudentYear];
                var allIncorrectSlotOfStudentYear = allAvailableSlot
                    .Where(x => I_slot_largestYears[x.Key] == consideringStudentYear)
                    .Where(x => x.Key.shift != prioritizedShiftForThisStudentYear);
                
                if (!allIncorrectSlotOfStudentYear.Any())
                {
                    P_allShiftIsCorrectlyFull[I_studentYears.IndexOf(consideringStudentYear)] = true;
                    P_allIsSmallerThanSmallestCorrectShift[I_studentYears.IndexOf(consideringStudentYear)] = true;
                    goto CASE1;
                }
                var allIncorrectSlotAtPrioritizedShift = allAvailableSlot
                    .Where(x => x.Key.shift == prioritizedShiftForThisStudentYear)
                    .Where(x => I_slot_largestYears[x.Key] != consideringStudentYear);
                
                // Console.WriteLine($"count : {allIncorrectSlotAtPrioritizedShift.Count()}");
                if (!allIncorrectSlotAtPrioritizedShift.Any())
                {
                    P_allShiftIsCorrectlyFull[I_studentYears.IndexOf(consideringStudentYear)] = true;
                    goto CASE1;
                }

                (position_1, courses_1) = allIncorrectSlotOfStudentYear.MaxBy(x => I_slot_largestYearCount[x.Key]);
                listForPosition2 = allIncorrectSlotAtPrioritizedShift;
                improvePlacement = true;
            }


            CASE2:
            if (!allRemainingMisplacedShiftIsSmallerThanSmallestCorrectShift && allShiftIsCorrectlyFull)
            {
                StudentYear? consideringStudentYear = null;
                for (int i = 0; i < I_studentYears.Count; i++)
                {
                    if (!P_allIsSmallerThanSmallestCorrectShift[i])
                    {
                        consideringStudentYear = I_studentYears[i];
                        break;
                    }
                }
                if (consideringStudentYear == null)
                {
                    allRemainingMisplacedShiftIsSmallerThanSmallestCorrectShift = true;
                    Console.WriteLine("END CASE 2, no more small correct shift found");
                    goto CASE3;
                }
                var prioritizedShiftOfThisStudentYear = I_studentYear_prioritizedShift[consideringStudentYear];
                var allMisplacedSlotOfThisStudentYear = allAvailableSlot
                    .Where(x => I_slot_largestYears[x.Key] == consideringStudentYear)
                    .Where(x => x.Key.shift != prioritizedShiftOfThisStudentYear);
                if (!allMisplacedSlotOfThisStudentYear.Any())
                {
                    P_allIsSmallerThanSmallestCorrectShift[I_studentYears.IndexOf(consideringStudentYear)] = true;
                    goto CASE2;
                }
                var largestIncorrectShift = allMisplacedSlotOfThisStudentYear.MaxBy(x => I_slot_largestYearCount[x.Key]).Key;
                var largestIncorrectShiftCount = I_slot_largestYearCount[largestIncorrectShift];
                var allSmallerCorrectShift = allAvailableSlot
                    .Where(x => I_slot_largestYearCount[x.Key] < largestIncorrectShiftCount)
                    .Where(x => x.Key.shift == prioritizedShiftOfThisStudentYear);
                if(!allSmallerCorrectShift.Any())
                {
                    P_allIsSmallerThanSmallestCorrectShift[I_studentYears.IndexOf(consideringStudentYear)] = true;
                    goto CASE2;
                }
                (position_1, courses_1) = (largestIncorrectShift, I_slot_courses[largestIncorrectShift]);
                listForPosition2 = allSmallerCorrectShift;
                improvePlacement = true;
            }

            CASE3:
            if(allRemainingMisplacedShiftIsSmallerThanSmallestCorrectShift && allShiftIsCorrectlyFull)
            {
                (position_1, courses_1) = allAvailableSlot.PickRandomFromIEnumerable();
                if (position_1.shift == I_studentYear_prioritizedShift[I_slot_largestYears[position_1]])
                {
                    listForPosition2 = allAvailableSlot.Where(x => I_studentYear_prioritizedShift[I_slot_largestYears[position_1]] == x.Key.shift);
                }
                else
                {
                    listForPosition2 = allAvailableSlot.Where(x => I_studentYear_prioritizedShift[I_slot_largestYears[x.Key]] != x.Key.shift);
                }
            }
            // Pick at random 2 entries of the table, then try to swap and calculate the solution values.
            /*var allPosititons = P_slot_courses.Where(x => P_slot_movability[x.Key]);
            var (position_1, courses_1) = allPosititons.PickRandomFromIEnumerable();
            bool correctShift = false;
            if (position_1.shift == I_studentYear_prioritizedShift[O_slot_largestYears[position_1]])
            {
                correctShift = true;
            }
            var list = P_slot_courses.Where(x => P_slot_movability[x.Key]);
            if (correctShift)
            {
                list = list.Where(x => I_studentYear_prioritizedShift[O_slot_largestYears[position_1]] == x.Key.shift);
            }
            else
            {
                var listtemp = list.Where(x => I_studentYear_prioritizedShift[O_slot_largestYears[position_1]] == x.Key.shift && I_studentYear_prioritizedShift[O_slot_largestYears[x.Key]] != x.Key.shift);
                if (listtemp.Any())
                {
                    list = listtemp;
                    improvePlacement = true;
                }
                else
                {
                    var pos1_count = P_slot_largestYearCount[position_1];
                    listtemp = list.Where(x => P_slot_largestYearCount[x.Key] < pos1_count);
                    if(listtemp.Any())
                    {
                        list = listtemp;
                        improvePlacement = true;
                    }
                    else
                    {
                        list = list;
                    }
                }
            }*/
            
            if(!listForPosition2.Any()) 
            {
                throw new Exception("INVALID");
            }

            (position_2, courses_2) = listForPosition2.PickRandomFromIEnumerable();
            
            if (!I_slot_movability[position_1] || !I_slot_movability[position_2])
                Console.WriteLine("WTH");
            // We swap places of courses_1 and courses_2 here
            SwapPositionOfDataOfSeperationPoint(position_1, position_2 );
            out_position_1 = position_1;
            out_position_2 = position_2;
        }

        private void FindAnotherSolutionForSeperationPointMultipleSteps(int stepCount, out List<(int date, int shift)> out_positions_1, out List<(int date, int shift)> out_positions_2)
        {
            out_positions_1 = new();
            out_positions_2 = new();
            for (int i = 0; i < stepCount; i++)
            {
                FindNeighboringSolutionForSeperationPoint(out var out_position_1, out var out_position_2, out var improvePlacement);
                out_positions_1.Add(out_position_1);
                out_positions_2.Add(out_position_2);
            }
        }

        private void RollBackSolution((int date, int shift) position_1,  (int date, int shift) position_2)
        {
            var courses_1 = I_slot_courses[position_1];
            var courses_2 = I_slot_courses[position_2];
            // We swap places of courses_1 and courses_2 here
            I_slot_courses[position_1] = courses_2;
            I_slot_courses[position_2] = courses_1;
            foreach (var course_in_1 in courses_1)
            {
                I_course_slots[course_in_1].Remove(position_1);
                I_course_slots[course_in_1].Add(position_2);
            }
            foreach (var course_in_2 in courses_2)
            {
                I_course_slots[course_in_2].Remove(position_2);
                I_course_slots[course_in_2].Add(position_1);
            }
            (I_slot_largestYears[position_1], I_slot_largestYears[position_2]) = (I_slot_largestYears[position_2], I_slot_largestYears[position_1]);
            (I_slot_largestYearCount[position_1],I_slot_largestYearCount[position_2]) = (I_slot_largestYearCount[position_2],I_slot_largestYearCount[position_1]);
            (P_positionChangedArray[position_1.date, position_1.shift], P_positionChangedArray[position_2.date, position_2.shift])
                = (P_positionChangedArray[position_2.date, position_2.shift], P_positionChangedArray[position_1.date, position_1.shift]);
        }

        private void RollBackSolutionMultipleStep(int stepCount, List<(int date, int shift)> positions_1, List<(int date, int shift)> positions_2)
        {
            for (int i = stepCount -1; i > 0; i--)
            {
                RollBackSolution(positions_1[i], positions_2[i]);
            }
        }

        // Evaluation
        int evalConsecutivePoint = 1;
        int evalSeperationPoint = 1;

        private int EvaluateCurrentSolutionForSeperativePoint(out int out_sumOfConsecutivePoint, out int out_sumOfSeperationPoint)
        {
            // Check if a course is divided into 2 dates, or not in consecutive shifts.
            int sumOfConsecutivePoint = 0;
            foreach (var (course, positions) in I_course_slots)
            {
                int evalConsecutiveCoef = 0;
                int examClassOfCourseCount = P_course_examClassCount[course];
                var (oldDate,shift) = positions[0];
                bool invalidDate = false;
                for (int i = 1; i < positions.Count; i++)
                {
                    if(oldDate != positions[i].date)
                    {
                        evalConsecutiveCoef = 1;
                        invalidDate = true;
                        break;
                    }    
                }
                if (!invalidDate)
                {
                    // Console.WriteLine("Invalid date {0}", course.Name);
                    var shifts = positions.Select(x => x.shift).OrderBy(x => x).ToList();
                    for (int i = 1; i < positions.Count; i++)
                    {
                        if (shifts[i] - shifts[i - 1] != 1)
                        {
                            evalConsecutiveCoef = 1;
                            invalidDate = true;
                            break;
                        }
                    }
                }

                /**if(!invalidDate)
                {
                    Console.WriteLine("Invalid date {0} contribute {1}", course.ID,examClassOfCourseCount);
                }*/
                int pointOfThisCourse = evalConsecutiveCoef * evalConsecutivePoint * examClassOfCourseCount ;
                sumOfConsecutivePoint += pointOfThisCourse;
            }
            int sumOfSeperationPoint = 0;
            // Right below is the function to calculate code for seperation test for each curricula
            foreach (var curriculum in I_curricula)
            {
                int evalSeperationCoef = 0;

                var courseOfCurriculum = curriculum.Courses.Where(x => I_course_slots.ContainsKey(x));
                var courseGroupByYear = courseOfCurriculum.GroupBy(x => I_course_mainStudentYear[x]);
                foreach (var item in courseGroupByYear)
                {
                    var studentYear = item.Key;
                    List<(Course course, int date)> coursesAndDates = item
                        .Select(x => (x, I_course_slots[x][0].date)) // We choose the first existing date of the course
                        .OrderBy(x => x.date).ToList(); // We order by dates to obtain the easier way to solve the problem
                    int courseCount = coursesAndDates.Count;
                    if (courseCount == 0)
                        continue;
                    int[] diffArray = new int[courseCount - 1];
                    for (int i = 0; i < diffArray.Length; i++)
                    {
                        diffArray[i] = coursesAndDates[i + 1].date - coursesAndDates[i].date;
                    }
                    for (int i = 0; i < diffArray.Length; i++)
                    {
                        switch (diffArray[i])
                        {
                            case 0:
                                evalSeperationCoef = 2;
                                break;
                            case 1:
                                evalSeperationCoef = 1;
                                break;
                            default:
                                evalSeperationCoef = 0;
                                break;
                        }
                        int seperationPoint = evalSeperationPoint * evalSeperationCoef;
                        // The below codes add total examClass for doubling effects. The result is kind of messy with horendous performance so I dont want anything to do with it anymore.
                        /** (P_course_examClassCount[coursesAndDates[i].course] > P_course_examClassCount[coursesAndDates[i + 1].course]
                        ? P_course_examClassCount[coursesAndDates[i].course]
                        : P_course_examClassCount[coursesAndDates[i + 1].course]); **/
                        // We add total examClass of two course for doubling effects.
                        sumOfSeperationPoint += seperationPoint;
                    }
                }
                /*List<(Course course, int date)> coursesAndDates = courseOfCurriculum
                    .Where(x => P_course_slots.ContainsKey(x)) // We first eliminate null values
                    .Select(x => (x, P_course_slots[x][0].date)) // We choose the first existing date of the course
                    .OrderBy(x => x.date).ToList(); // We order by dates to obtain the easier way to solve the problem
                int courseCount = coursesAndDates.Count;
                if (courseCount == 0)
                    continue;
                int[] diffArray = new int[courseCount - 1];
                for (int i = 0; i < diffArray.Length; i++)
                {
                    diffArray[i] = coursesAndDates[i + 1].date - coursesAndDates[i].date;
                }
                for (int i = 0; i < diffArray.Length; i++)
                {
                    switch (diffArray[i])
                    {
                        case 0:
                            evalSeperationCoef = 2;
                            break;
                        case 1:
                            evalSeperationCoef = 1;
                            break;
                        default:
                            evalSeperationCoef = 0;
                            break;
                    }
                    int seperationPoint = evalSeperationPoint * evalSeperationCoef;
                    // The below codes add total examClass for doubling effects. The result is kind of messy with horendous performance so I dont want anything to do with it anymore.
                        (P_course_examClassCount[coursesAndDates[i].course] > P_course_examClassCount[coursesAndDates[i + 1].course]
                        ? P_course_examClassCount[coursesAndDates[i].course]
                        : P_course_examClassCount[coursesAndDates[i + 1].course]); 
                    // We add total examClass of two course for doubling effects.
                    sumOfSeperationPoint += seperationPoint;
                }*/
            }
            // Right Above is the function to calculate code for seperation test for each curricula
            out_sumOfConsecutivePoint = sumOfConsecutivePoint;
            out_sumOfSeperationPoint = sumOfSeperationPoint;
            return sumOfConsecutivePoint + sumOfSeperationPoint;
        }
        
        private double overallPointCoef = 1;
        private double guidedPropability = 0.5;
        private double ProbilityFunction(int oldPoint, int newPoint, double temperature)
        {
            return Math.Exp((oldPoint - newPoint) * overallPointCoef / temperature);
        }
        private int StepCountFunction(double temperature)
        {
            return (int)Math.Ceiling(temperature * 20);
        }

        public void OptimizeTable()
        {
            int currentSolutionPoint = EvaluateCurrentSolutionForSeperativePoint(out int curr_consecutivePoint, out int curr_seperationPoint);
            Console.WriteLine("Current solution Val : {0}, consecutive : {1}, seperation : {2}.", currentSolutionPoint, curr_consecutivePoint, curr_seperationPoint);

            while (I_temperature > I_terminate_temperature)
            {
                // var stepCount = StepCountFunction(I_temperature);
                bool improvePlacement = false;
                for (int i = 0; i < I_markovChain_length; i++)
                {
                    // FindAnotherSolutionForSeperationPointMultipleSteps(stepCount, out var out_positions_1, out var out_positions_2);
                    // DISCARDED function
                    (int, int) out_position_1, out_position_2;
                    /*if (RandomExtension.ChooseProbability(guidedPropability))
                        FindNeighboringrSolutionForSeperationPoint_GUIDED(out out_position_1, out out_position_2);
                    else*/
                    FindNeighboringSolutionForSeperationPoint(out out_position_1, out out_position_2, out improvePlacement);
                    int nextSolutionPoint = EvaluateCurrentSolutionForSeperativePoint(out int next_consecutivePoint, out int next_seperationPoint);
                    // TODO: MAKE FUNCITON
                    if (nextSolutionPoint < currentSolutionPoint || improvePlacement )
                    {
                        Console.WriteLine("Better Solution Chosen by swapping {0} -> {1} ", out_position_1, out_position_2);
                        currentSolutionPoint = nextSolutionPoint;
                        curr_consecutivePoint = next_consecutivePoint;
                        curr_seperationPoint = next_seperationPoint;
                    }
                    else
                    {
                        var probabiltyValue = ProbilityFunction(currentSolutionPoint, nextSolutionPoint, I_temperature);
                        bool chooseTheSolution = RandomExtension.ChooseProbability(probabiltyValue);
                        if (chooseTheSolution)
                        {
                            if(probabiltyValue < 1)
                                Console.WriteLine("Worse Solution Chosen at : {0} propbability", probabiltyValue);
                            currentSolutionPoint = nextSolutionPoint;
                            curr_consecutivePoint = next_consecutivePoint;
                            curr_seperationPoint = next_seperationPoint;
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
                Console.WriteLine("Current solution Val : {0}, consecutive : {1}, seperation : {2}, temperature {3}", currentSolutionPoint, curr_consecutivePoint, curr_seperationPoint, I_temperature);
                if (!improvePlacement)
                    I_temperature *= I_temperature_decrement;
            }
        }

        private void MakeChangesToSchedule()
        {
            HashSet<(int date, int shift)> unchangedShifts = new();
            for (int date = 0; date < I_schedule.dates.Length; date++)
            {
                for (int shift = 0; shift < I_schedule.shifts.Length; shift++)
                {
                    unchangedShifts.Add((date, shift));
                }
            }
            while (unchangedShifts.Count > 0)
            {
                var (date, shift) = unchangedShifts.First();
                var (date_2, shift_2) = P_positionChangedArray[date, shift];
                while (unchangedShifts.Contains((date_2, shift_2)))
                {
                    I_schedule.SwapTwoSlotInSchedule((date, shift), (date_2, shift_2));
                    unchangedShifts.Remove((date, shift));
                    unchangedShifts.Remove((date_2, shift_2));
                    (date, shift) = (date_2, shift_2);
                    (date_2, shift_2) = P_positionChangedArray[date_2, shift_2];
                }
            }
        }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
#if DEBUG
            LogDebug();
#endif
        }

        public void CheckAllInput()
        {
            if (I_curricula == null || I_curricula.Count == 0)
                throw new Exception("Null Input");
            if (I_schedule == null)
                throw new Exception("Null Input");
            if (I_hardRails == null)
                throw new Exception("Null Input");
        }

        public void InitializeAllOutput()
        {
            
        }
        private void LogDebug()
        { 

            Logger.logger.LogMessage("AnnealingOptimization.cs: Các lớp đang bị xếp lịch rời nhau :");
            foreach (var (course, positions) in I_course_slots)
            {
                var (oldDate, shift) = positions[0];
                bool invalidDate = false;
                for (int i = 1; i < positions.Count; i++)
                {
                    if (oldDate != positions[i].date)
                    {
                        invalidDate = true;
                        break;
                    }
                }
                if (!invalidDate)
                {
                    // Console.WriteLine("Invalid date {0}", course.Name);
                    var shifts = positions.Select(x => x.shift).OrderBy(x => x).ToList();
                    for (int i = 1; i < positions.Count; i++)
                    {
                        if (shifts[i] - shifts[i - 1] != 1)
                        {
                            invalidDate = true;
                            break;
                        }
                    }
                }
                if (invalidDate)
                {
                    Logger.logger.LogMessage("---------------------------------");
                    Logger.logger.LogMessage("Course id: {0}, Course name {1}", course.ID, course.Name);
                    for (int i = 0; i < positions.Count; i++)
                    {
                        Logger.logger.LogMessage("(D,S) {0} : ( {1} , {2} )", i+1, positions[i].date, positions[i].shift);
                    }
                    Logger.logger.LogMessage("---------------------------------");

                }
            }
            for (int date = 0; date < I_schedule.dates.Length; date++)
            {
                for (int shift = 0; shift < I_schedule.shifts.Length; shift++)
                {
                    var (date_2, shift_2) = P_positionChangedArray[date, shift];
                    Console.WriteLine($"{date}, {shift} - {date_2}, {shift_2}");
                }
            }

            for (int date = 0; date < I_schedule.dates.Length; date++)
            {
                for (int shift = 0; shift < I_schedule.shifts.Length; shift++)
                {
                    Dictionary<StudentYear, int> year_examClassCount = new();
                    foreach (StudentYear studentYear in I_studentYears)
                    {
                        year_examClassCount.Add(studentYear, 0);
                    }
                    for (int room = 0; room < I_schedule.rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell == null)
                        {
                            continue;
                        }
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            year_examClassCount[I_course_mainStudentYear[examClass.StudyClass.Course]] += 1;
                        }
                    }
                    string s = year_examClassCount.Select(x => x.Key.Name + " " + x.Value).Aggregate((x, y) => x + " - " + y);
                    Console.Write(s);
                    Console.WriteLine($"  main: {I_slot_largestYears[(date, shift)].Name}, count : {I_slot_largestYearCount[(date, shift)]}, index:{P_positionChangedArray[date, shift]}");
                }
            }

        }
        public void ProcedureRun()
        {
            Inittialize();
            InitializeGuildline();
            OptimizeTable();
            MakeChangesToSchedule();
            I_slot_largestYears = I_slot_largestYears;
        }
    }
}
