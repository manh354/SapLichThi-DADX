using System.Text.RegularExpressions;

namespace SapLichThiCore.DataObjects
{
    // Define roomType small for capacity < 100, med 120 to 160, large > 180 (not clearly defined yet)
    public enum RoomType
    {
        small, medium, large
    }

    public partial class Room
    {
        // Define the roomId.
        string roomId;
        public string Id => roomId;

        // Define the maximum students the room is capable of holding.
        int capacity;
        public int Capacity => capacity;

        // Define the room type of the current room 
        RoomType roomType;
        public RoomType RoomType => roomType;

        // Define the building of which the room belongs to.
        Building building;
        public Building Building => building;

        // Define the floor on which the room is situated.
        public int floor;
        public int Floor => floor;

        // Define if the room is the spareRoom 
        public bool isSpare;
        public bool IsSpare => isSpare;
        public void SetIsSpare(bool isSpare)
        {
            this.isSpare = isSpare;
        }

        public Room(string roomId, int capacity, RoomType roomType, Building building, bool isSpare, int floor = 0)
        {
            this.roomId = roomId;
            this.capacity = capacity;
            this.roomType = roomType;
            this.building = building;
            this.isSpare = isSpare;
            this.floor = floor;
        }

        public override string ToString()
        {
            return $"id:{roomId}, c:{capacity}";
        }


        static readonly Regex _buildingRegex = MyRegex();
        /// <summary>
        /// Use regex to try match building Id and Floor number of a given roomId.
        /// </summary>
        /// <param name="roomIdString">Given roomId as string</param>
        /// <param name="buildingId">Output building Id</param>
        /// <param name="floorNum">Output floor number</param>
        /// <returns>Whether this function find a match or not.</returns>
        public static bool MatchRegexBuildingAndFloor(string roomIdString, out string? buildingId, out string? floorNum)
        {
            Match match = _buildingRegex.Match(roomIdString);
            if (match.Success) 
            {
                buildingId = match.Groups[1].Value;
                floorNum = match.Groups[2].Value;
                return true;
            }
            buildingId = null;
            floorNum = null;
            return false;
        }
        public static RoomType GetRoomTypeFromCapacity(int capacity)
        {
            RoomType roomType = capacity > 150 ? RoomType.large : capacity > 100 ? RoomType.medium : RoomType.small;
            return roomType;
        }

        [GeneratedRegex(@"(^[\w]+).(\d)")]
        private static partial Regex MyRegex();
    }
}
