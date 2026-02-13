using SapLichThiCore.DataObjects;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public class RoomSeperator : BaseAlgorithmObject
    {
        public List<Room> I_allRooms { get; set; }
        public List<Room> I_spareRooms { get; set; }
        public List<Room> O_prioritizedRooms { get; set; }

        protected override void InitializeAllOutput()
        {
            O_prioritizedRooms = new List<Room>();
        }

        protected override void ProcedureRun()
        {
            var spareRoomsHashSet = I_spareRooms.ToHashSet();
            O_prioritizedRooms = I_allRooms.Where(x=>spareRoomsHashSet.Contains(x)).ToList();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_allRooms = context.I_allRooms;
            I_spareRooms = context.I_spareRooms;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_prioritizedRooms = context.I_prioritizedRooms;
        }

    }
}