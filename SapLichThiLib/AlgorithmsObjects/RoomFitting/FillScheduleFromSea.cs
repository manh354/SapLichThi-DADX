using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects.DynamicPooling
{
    internal class FillScheduleFromSea
    {
        public ClassRoomSea I_classRoomSea;
        public ExamSchedule I_schedule;
        public void Run()
        {
            foreach (var pool in I_classRoomSea.Pools)
            {
                var date = pool.Date;
                var shift = pool.Shift;
                foreach (var container in pool.Containers)
                {
                    Room room = container.Box;
                    I_schedule.AddToThisCell(date, shift, room, container.Elements);
                }
            }
        }
    }
}
