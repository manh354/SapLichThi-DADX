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

    public class RoomTest: ITest<RoomTestObject>
    {
        ExamSchedule schedule;
        List<RoomTestObject> allObjects = new ();
        string OutputPath = @"Outputs\Test_Rooms.csv";
        public RoomTest(ExamSchedule schedule)
        {
            this.schedule = schedule;
        }

        public IEnumerable<RoomTestObject> GiveTestResult()
        {
            Test();
            return allObjects;
        }

        public void Test()
        {
            var dates = schedule.dates;
            var shifts = schedule.shifts;
            var rooms = schedule.rooms;
            for (int date = 0; date < dates.Length; date++)
            {
                for (var shift = 0; shift < shifts.Length; shift++)
                {
                    for (int room = 0; room < rooms.Length; room++)
                    {
                        var thisCell = schedule.GetCell(date, shift, room);
                        var count = 0;
                        if (thisCell == null)
                            continue;
                        foreach (var examClass in thisCell.ExamClasses)
                        {
                            count += examClass.Count;
                        }
                        var condition = true;
                        if (count > schedule.rooms[room].Capacity * (2f / 3f))
                        {
                            condition = false;
                        }
                        allObjects.Add(
                                new RoomTestObject()
                                {
                                    ArrayCell = thisCell,
                                    Date = dates[date],
                                    Shift = shifts[shift],
                                    Room = rooms[room],
                                    Condition = condition,
                                    Ratio = count / (float)schedule.rooms[room].Capacity
                                });
                    }
                }
            }
            return ;
        }
    }
}
