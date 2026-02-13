namespace SapLichThiCore.DataObjects
{
    public class DistanceArray
    {
        Dictionary<(Building, Building), int> Data;
        public int GetBuildingDistance(Building building1, Building building2)
        {
            return Data[(building1, building2)];
        }
        public static DistanceArray FromTupleList(List<(Building building1, Building building2, int distance)> values)
        {
            Dictionary<(Building, Building), int> Data = new();
            foreach (var value in values)
            {
                Data.Add((value.building1, value.building2), value.distance);
                Data.Add((value.building2, value.building1), value.distance);
            }
            return new DistanceArray()
            {
                Data = Data,
            };
        }
        public static DistanceArray FromLists(List<Building> list1, List<Building> list2, List<int> distances)
        {
            int len1 = list1.Count;
            int len2 = list2.Count;
            int len3 = distances.Count;

            if (len1 != len2 || len2 != len3 || len3 != len1)
            {
                throw new Exception("Độ dài 3 list không match");
            }
            Dictionary<(Building, Building), int> Data = new();
            for (int i = 0; i < len1; i++)
            {
                Data.Add((list1[i], list2[i]), distances[i]);
                Data.Add((list2[i], list1[i]), distances[i]);
            }
            return new DistanceArray()
            {
                Data = Data,
            };
        }
    }
}
