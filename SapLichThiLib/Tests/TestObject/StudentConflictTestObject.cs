
using SapLichThiLib.DataObjects;
using SapLichThiLib.DataStructures;
using System.Collections.Generic;

public class StudentConflictTestObject
{
    public Student Student { get; set; }
    public List<((ArrayCell cell, DateOnly date, int shift, Room room) obj1, (ArrayCell cell, DateOnly date, int shift, Room room) obj2)> ConflictPairs { get; set; }
}