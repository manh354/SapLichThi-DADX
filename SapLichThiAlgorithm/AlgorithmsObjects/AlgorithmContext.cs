
using SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Xml.Serialization;
using System.Xml;
using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiCore.DataType;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public class AlgorithmContext
    {
        public AlgorithmContext()
        {

        }
        // DEFINING DATA TYPE
        public required InputDataType I_inputDataType { get; set; }

        
        /// <summary>
        /// Contains all the examClasses including no schedule and normal
        /// classes.
        /// </summary>
        public required List<ExamClass> I_allExamClasses { get; set; }
        /// <summary>
        /// Contains only the normal classes.
        /// </summary>
        public List<ExamClass> I_examClasses { get; set; }
        
        /// <summary>
        /// Contains all the students.
        /// </summary>
        public required List<Student> I_students { get; set; }
        /// <summary>
        /// The periods in the schedule
        /// </summary>
        public required List<Period> I_periods { get; set; }
        /// <summary>
        /// All the available rooms in the University (including
        /// normal and non-prioritized rooms)
        /// </summary>
        public required List<Room> I_allRooms { get; set; }
        /// <summary>
        /// All the spare rooms (non-prioritized rooms)
        /// </summary>
        public required List<Room> I_spareRooms { get; set; }
        /// <summary>
        /// All the prioritized rooms
        /// </summary>
        public List<Room> I_prioritizedRooms { get; set; }

        public required int I_largeAndMediumRoomCount { get; set; }
        /// <summary>
        /// Represent the prefer shifts order
        /// </summary>
        public required int[] I_biasTable { get; set; } = { 1, 2, 3, 0, 4 };
        public required List<Room> I_largeAndMediumRooms { get; set; }

        // Scheduling Modelhóm môn thi
        public float I_optimalRoomCoef { get; set; } = 0.55f;

        /// <summary>
        /// The values of valid ROOMS corresponding with each COURSE. If there isn't a course in the dictionary
        /// then we assume the course is valid everywhere
        /// </summary>
        public Dictionary<ExamClass, Dictionary<Room, int>> I_examClass_validRoomsPenalties { get; set; }
        /// <summary>
        /// The values of valid SLOT corresponding with each COURSE. If there isn't a course in the dictionary
        /// then we assume the course is valid everywhere
        /// </summary>
        public Dictionary<ExamClass, Dictionary<Period, int>> I_examClass_validSlotsPenalties { get; set; }
        /// <summary>
        /// The values of valid SLOT corresponding with each ROOM. If there isn't a room in the dictionary
        /// then we assume the room is valid everywhere
        /// </summary>
        public Dictionary<Room, Dictionary<Period, int>> I_room_validSlotsPenalties { get; set; }
        /// <summary>
        /// The value for slot penalties
        /// </summary>
        public Dictionary<Period, int> I_slot_penalties { get; set; }
        /// <summary>
        /// Binary constraints between exam classes
        /// </summary>
        public List<BinaryConstraint> I_binaryConstraints { get; set; }
        /// <summary>
        /// Unary constraints of exam classes
        /// </summary>
        public List<UnaryConstraint> I_unaryConstraints { get; set; }


        // PREPROCESSED DATA SECTION
        public Dictionary<(int, int), bool> I_dateShift_reserved { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClass_linkages { get; set; }
        public Dictionary<ExamClass, Dictionary<ExamClass, int>> I_examClass_linkages_count { get; set; }

        // TIMEFIXED DATA SECTION
        public Dictionary<(int, int), bool> I_slots_available { get; set; }


        // STRUCTURAL BUILD DATA SECTION
        public ClassGraph I_classGraph { get; set; }
        public List<Room> I_rooms { get; set; }
        public Dictionary<int, HashSet<ExamClass>> I_color_examClasses { get; set; }
        /// <summary>
        /// This property is not yet required
        /// </summary>
        Dictionary<(Room, Room), float> distanceBetweenRooms { get; set; }
        public int MAX_ITER { get; set; } = CompositeSettings.MaximumIterations;
        public float MAXIMUM_PERCENTAGE { get; set; } = CompositeSettings.MaximumPercentage;
        public Lake I_lake { get; set; }


        // SIMULATED ANNEALING SECTION
        public Dictionary<(int date, int shift), int> I_slot_largestYearCount { get; set; }
        public Dictionary<(int date, int shift), bool> I_slot_movability { get; set; }
        public Dictionary<Student, HashSet<ExamClass>> I_student_relevantExamClasses { get; set; }
        public Dictionary<Student, HashSet<ExamClass>> I_student_allExamClasses { get; set; }

        public bool I_A_useSimulatedAnnealingCourse { get; set; } = false;

        public double I_A_temperatureCourse { get; set; } = 1f;
        public double I_A_temperature_decrementCourse { get; set; } = 0.99;
        public double I_A_terminate_temperatureCourse { get; set; } = 0.1;
        public int I_A_markovChain_lengthCourse { get; set; } = 10;

        public bool I_A_useSimulatedAnnealingShift { get; set; } = false;

        public double I_A_temperatureShift { get; set; } = 1f;
        public double I_A_temperature_decrementShift { get; set; } = 0.99;
        public double I_A_terminate_temperatureShift { get; set; } = 0.1;
        public int I_A_markovChain_lengthShift { get; set; } = 10;

        public EvalDouble I_eval_point { get; set; } = new EvalDouble(0,double.MaxValue);

        public double I_STUDENT_CONFLICT_PENALTY { get; set; } = 1000.0;
        public double I_COURSE_SEPARATION_PENALTY { get; set; } = 5.0;
        public double I_NON_CONSECUTIVE_SHIFT_PENALTY { get; set; } = 2.0;
        public double I_SAME_DAY_EXAM_PENALTY { get; set; } = 5.0;
        public double I_CONSECUTIVE_DAY_EXAM_PENALTY { get; set; } = 2.0;
        public double I_STUDENT_YEAR_PREFERENCE_PENALTY { get; set; } = 0.5;

        public double I_timeBreakSeconds { get; set; } = 600;
        public int I_patient { get; set; } = 40;

        // ADDITIONAL HARD CONSTRAINT VIOLATION SECTION
        public bool HardConstraint_NoStudentConflict { get; set; } = true;
        public bool HardConstraint_LimitedCapacity { get; set; } = true;
        public bool HardConstraint_DifferentRoomForCourses { get; set; } = true;
        public bool HardConstraint_OnlyOneExamClassPerRoom { get; set; } = false;

        public class ContextSettings
        {
            public List<DateOnly> I_dates;
            public List<int> I_shifts;
            public float I_optimalRoomCoef;
            public bool I_A_useSimulatedAnnealing;
        }

        public string SerializeSettingsAsJson()
        {
            var obj = new
            {
                I_optimalRoomCoef,
                I_A_useSimulatedAnnealingCourse,
            };
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        public string SerializeSettingsAsXml()
        {
            var obj = new ContextSettings()
            {
                I_optimalRoomCoef = I_optimalRoomCoef,
                I_A_useSimulatedAnnealing = I_A_useSimulatedAnnealingCourse,
            };
            return "";
        }

    }
}
