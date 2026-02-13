using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds
{
    public class ClassRoomObjectBuilder : BaseAlgorithmObject
    {
        public List<Room> I_rooms { get; set; }
        public List<Period> I_periods { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClassLinkages { get; set; }
        public Lake O_lake { get; set; }

        private Lake MakeClassRoomLake()
        {
            List<Pond> ponds = new List<Pond>();
            foreach (Period period in I_periods)
            {
                List<Puddle> puddles = new List<Puddle>();
                foreach (Room room in I_rooms)
                {
                    var puddle = MakeClassRoomPuddle(period, room);
                    puddles.Add(puddle);
                }
                Pond pond = MakeClassRoomPond(puddles, period);
                ponds.Add(pond);

            }

            Lake result = new Lake(ponds);
            return result;
        }

        private Pond MakeClassRoomPond(List<Puddle> puddles,  Period period)
        {
            Pond result = new Pond(puddles, period, new());
            return result;
        }
        private Puddle MakeClassRoomPuddle(Period period, Room room)
        {
            return new Puddle(period, room);
        }

        protected override void InitializeAllOutput()
        {
            // There aren't any output require initialization.
            // O_sea is dynamically generated.
        }

        protected override void ProcedureRun()
        {
            O_lake = MakeClassRoomLake();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_examClassLinkages = context.I_examClass_linkages;
            I_rooms = context.I_rooms;
            I_periods = context.I_periods;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_lake = O_lake;
        }
    }
}
