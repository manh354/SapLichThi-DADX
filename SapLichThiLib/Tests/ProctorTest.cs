using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using SapLichThiLib.Tests.TestObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests
{
    public class ProctorTestObject
    {
        public DateOnly Date { get; set; }
        public int Shift { get; set; }
        public Dictionary<School, int> School_ProctorRemainCount {  get; set; }
    }
    public class ProctorTest : ITest<ProctorTestObject>
    {
        public ExamSchedule I_schedule {get;set;}
        public List<School> I_schools { get;set;}

        public List<ProctorTestObject> O_result = new();

        public void Test()
        {
            // Initialize output
            O_result = new();
            var dates = I_schedule.dates;
            var shifts = I_schedule.shifts;
            var rooms = I_schedule.rooms;
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    Dictionary<School, int> school_ProctorRemainCount = new();
                    foreach (School school in I_schools)
                    {
                        school_ProctorRemainCount.Add(school, school.MaxProctor);
                    }
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = I_schedule.GetCell(date,shift,room);
                        if (thisCell == null)
                            continue;
                        var examClass = thisCell.ExamClasses.FirstOrDefault();
                        if (examClass == null)
                        {
                            continue;
                        }
                        int proctorOfRoomCount = 1;
                        var roomType = rooms[room].RoomType;
                        if (roomType == RoomType.small)
                            proctorOfRoomCount = 1;
                        else if(roomType == RoomType.medium || roomType == RoomType.large)
                            if(thisCell.ExamClasses.Count >= 2 || thisCell.ExamClasses.Sum(x=>x.Count) >= 70)
                                proctorOfRoomCount = 2;

                        school_ProctorRemainCount[examClass.StudyClass.Course.School] -= proctorOfRoomCount;
                    }
                    O_result.Add(new ProctorTestObject
                    {
                        Date = dates[date],
                        Shift = shifts[shift],
                        School_ProctorRemainCount = school_ProctorRemainCount,
                    });
                }
            }
        }

        public IEnumerable<ProctorTestObject> GiveTestResult()
        {
            Test();
            return O_result;
        }

    }
}
