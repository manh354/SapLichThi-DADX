using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    // Define roomType small for capacity < 100, med 120 to 160, large > 180 (not clearly defined yet)
    public enum RoomType
    {
        small = 0,medium = 1, large = 2
    }
    
    public class Room
    {
        // Define the roomId.
        string roomId;
        public string RoomId => roomId;

        // Define the maximum students the room is capable of holding.
        int capacity;
        public int Capacity => capacity;

        // Define the room type of the current room 
        RoomType roomType;
        public RoomType RoomType =>  roomType;

        // Define the building of which the room belongs to.
        Building building;
        public Building Building => building;

        // Define the floor on which the room is situated.
        public int floor;
        public int Floor => floor;

        public Room( string roomId, int capacity, RoomType roomType, Building building)
        {
            this.roomId = roomId;
            this.capacity = capacity;
            this.roomType = roomType;
            this.building = building;
        }
    }
}
