using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using SapLichThiLib.Tests.TestObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SapLichThiLib.Tests
{
    public class StudentConflictTest : ITest<StudentConflictTestObject>
    {
        public ExamSchedule I_schedule { get; set; }
        public Dictionary<string, StudyClass> StudyClassId_StudyClass_Dict { get; set; }
        public Dictionary<Student, HashSet<(ArrayCell, ArrayCell)>> Student_ConflictedPairOfClasses_Dict { get; set; }
        public Dictionary<Student, StudentConflictTestObject> Student_StudentConflictTestObject_Dict { get; set; }
        public List<StudentConflictTestObject> StudentConflictTestObjects { get; set; }
        public StudentConflictTest(ExamSchedule schedule, Dictionary<string, StudyClass> studyClassId_StudyClass_Dict)
        {
            I_schedule = schedule;
            StudyClassId_StudyClass_Dict = studyClassId_StudyClass_Dict;
        }

        private List<Student> GetAllConflictingStudentOfTwoClasses(StudyClass class1, StudyClass class2)
        {
            if (class1.Students == null || class2.Students == null)
                return null;
            var result = new List<Student>();
            foreach (var class1_student in class1.Students)
            {
                foreach (var class2_student in class2.Students)
                {
                    if(class1_student == class2_student)
                        result.Add(class1_student);
                }
            }
            if (result.Count > 0)
                return result;
            return null;
        }
        public void Test()
        {
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            

            Student_ConflictedPairOfClasses_Dict = new();
            Student_StudentConflictTestObject_Dict = new();
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date, shift, room);
                        if (thisCell.IsEmpty())
                            continue;
                        for (int room2 = room; room2 < rooms.Length; room2++)
                        {
                            var theOtherCell = I_schedule.GetCell(date, shift, room2);
                            if (theOtherCell == thisCell)
                                continue;
                            if (theOtherCell.IsEmpty())
                                continue;
                            foreach (var examClassInThisCell in thisCell.ExamClasses)
                            {
                                foreach (var examClassInTheOtherCell in theOtherCell.ExamClasses)
                                {
                                    if (examClassInThisCell.StudyClass == examClassInTheOtherCell.StudyClass) 
                                        continue;
                                    var AllConflictingStudents = GetAllConflictingStudentOfTwoClasses(examClassInThisCell.StudyClass, examClassInTheOtherCell.StudyClass);
                                    if (AllConflictingStudents == null)
                                        continue;
                                    foreach (var student in AllConflictingStudents)
                                    {
                                        if (!Student_ConflictedPairOfClasses_Dict.ContainsKey(student))
                                        {
                                            Student_ConflictedPairOfClasses_Dict.Add(student, new());
                                            Student_StudentConflictTestObject_Dict.Add(student, new() { ConflictPairs = new(), Student = student });
                                        }
                                        if (!Student_ConflictedPairOfClasses_Dict[student].Contains((theOtherCell, thisCell)))
                                        {
                                            Student_ConflictedPairOfClasses_Dict[student].Add((thisCell, theOtherCell));
                                            Student_StudentConflictTestObject_Dict[student].ConflictPairs.Add(((thisCell, dates[date], shifts[shift], rooms[room]), (theOtherCell, dates[date], shifts[shift], rooms[room])));
                                        }
                                    }
                                }
                            }
                        }    
                    }
                }
            }
        }


        public IEnumerable<StudentConflictTestObject> GiveTestResult()
        {
            Test();
            return Student_StudentConflictTestObject_Dict.Values;
        }
    }
}
