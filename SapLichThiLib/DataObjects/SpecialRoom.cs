using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class SpecialRoom
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }   
        public SpecialRoom(string id, string name, int capacity)
        {
            ID = id;
            Name = name;
            Capacity = capacity;
        }
    }
}
