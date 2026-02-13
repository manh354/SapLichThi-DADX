using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
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
