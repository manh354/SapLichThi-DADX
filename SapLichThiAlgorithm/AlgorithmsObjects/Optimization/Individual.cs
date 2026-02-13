using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization
{
    public class Individual
    {
        public Dictionary<(DateOnly date, int shift), Dictionary<Student, int>> P_slot_students { get; set; } = new();

        public Individual()
        {
        }

        public Individual DeepCopy()
        {
            var copy = new Individual();
            // Deep copy course_slots
            
            // Deep copy slot_students
            foreach (var kv in P_slot_students)
            {
                copy.P_slot_students[kv.Key] = new Dictionary<Student, int>(kv.Value);
            }
            return copy;
        }
    }
}
