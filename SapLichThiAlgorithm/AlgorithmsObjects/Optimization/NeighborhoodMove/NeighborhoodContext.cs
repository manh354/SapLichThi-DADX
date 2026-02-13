using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.DataStructureExtension;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove
{
    public class NeighborhoodContext
    {
        public Dictionary<Period, Dictionary<Student, int>> P_slot_students { get; set; } = new();
        public Dictionary<Student, Dictionary<Period, int>> P_student_slots { get; set; } = new();
        public Dictionary<ExamClass, List<Period>> P_old_exam_slot { get; set; } = new();
        public Dictionary<ExamClass, List<Period>> P_exam_slot { get; set; } = new();
        public Dictionary<ExamClass, List<(Period period, Room room)>> P_exam_positions { get; set; } = new();
        public static NeighborhoodContext FromAlgorithmContext(AlgorithmContext context)
        {

            NeighborhoodContext newContext = new();
            newContext.P_slot_students = new();
            newContext.P_student_slots = new();
            // Iterate over every pond in the lake.
            foreach (var pond in context.I_lake.Ponds)
            {
                // Iterate over every puddle in the pond.
                foreach (var puddle in pond.Puddles)
                {
                    // Assume that each puddle carries its Date and Shift information.
                    // (If the properties are named differently, update accordingly.)
                    Period period = pond.Period;

                    // Ensure the slot dictionaries have an entry.
                    if (!newContext.P_slot_students.ContainsKey(period))
                        newContext.P_slot_students[period] = new Dictionary<Student, int>();

                    // Iterate through each exam class in this puddle.
                    foreach (var examClass in puddle.Elements)
                    {

                        // Look for a Students property on the exam class.
                        // (This code assumes examClass exposes a IEnumerable<Student> property named "Students".)
                        var students = examClass.Students;
                        foreach (var student in students)
                        {
                            // Also update the slot-to-students dictionary.
                            newContext.P_slot_students[period].AddOrCreate(student, 1);
                            // Also update the student-to-slots dictionary.
                            newContext.P_student_slots.TryAdd(student, new());
                            newContext.P_student_slots[student].AddOrCreate(period, 1);
                        }

                        newContext.P_exam_slot.TryAdd(examClass, new());
                        newContext.P_exam_slot[examClass].Add(period);

                        newContext.P_old_exam_slot.TryAdd(examClass, new());
                        newContext.P_old_exam_slot[examClass].Add(period);

                        newContext.P_exam_positions.TryAdd(examClass, new());
                        newContext.P_exam_positions[examClass].Add((period, puddle.Room));
                    }
                }

            }

            return newContext;
        }

        public static NeighborhoodContext FromAlgorithmContextAndLake(AlgorithmContext context, Lake lake)
        {

            NeighborhoodContext newContext = new();
            newContext.P_slot_students = new();
            newContext.P_student_slots = new();
            // Iterate over every pond in the lake.
            foreach (var pond in lake.Ponds)
            {
                // Iterate over every puddle in the pond.
                foreach (var puddle in pond.Puddles)
                {
                    // Assume that each puddle carries its Date and Shift information.
                    // (If the properties are named differently, update accordingly.)
                    Period period = pond.Period;

                    if (!newContext.P_slot_students.ContainsKey(period))
                        newContext.P_slot_students[period] = new Dictionary<Student, int>();

                    // Iterate through each exam class in this puddle.
                    foreach (var examClass in puddle.Elements)
                    {

                        // Look for a Students property on the exam class.
                        // (This code assumes examClass exposes a IEnumerable<Student> property named "Students".)
                        var students = examClass.Students;
                        // Update course-to-students dictionary.
                        foreach (var student in students)
                        {
                            // Also update the slot-to-students dictionary.
                            newContext.P_slot_students[period].AddOrCreate(student, 1);
                            // Update student-to-slots dictionary.
                            newContext.P_student_slots.TryAdd(student, new());
                            newContext.P_student_slots[student].AddOrCreate(period, 1);
                        }

                        newContext.P_exam_slot.TryAdd(examClass, new());
                        newContext.P_exam_slot[examClass].Add(period);

                        newContext.P_old_exam_slot.TryAdd(examClass, new());
                        newContext.P_old_exam_slot[examClass].Add(period);

                        newContext.P_exam_positions.TryAdd(examClass, new());
                        newContext.P_exam_positions[examClass].Add((period, puddle.Room));

                    }
                }

            }

            return newContext;
        }
    }
}
