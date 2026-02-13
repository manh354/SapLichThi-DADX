namespace SapLichThiCore.DataObjects
{
    public class Building
    {
        private string buildingId;

        public string BuildingId
        {
            get { return buildingId; }
            set { buildingId = value; }
        }

        public Building(string buildingId)
        {
            this.buildingId = buildingId;
        }
    }
}
