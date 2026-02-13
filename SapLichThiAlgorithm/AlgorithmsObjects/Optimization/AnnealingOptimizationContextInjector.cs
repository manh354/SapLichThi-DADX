using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization
{
        public class AnnealingOptimizationParamContextInjector : BaseContextInjector
    {
        
        public double I_STUDENT_CONFLICT_PENALTY = 100.0;
        public double I_COURSE_SEPARATION_PENALTY = 5.0;
        public double I_NON_CONSECUTIVE_SHIFT_PENALTY = 2.0;
        public double I_SAME_DAY_EXAM_PENALTY = 5.0;
        public double I_CONSECUTIVE_DAY_EXAM_PENALTY = 2.0;
        public double I_STUDENT_YEAR_PREFERENCE_PENALTY = 0.5;

        public bool RunAnnealing { get; set; } = true;
        public double Temperature { get; set; } = 1f;
        public double TemperatureDecrement { get; set; } = 0.99;
        public double TerminateTemperature { get; set; } = 0.1;
        public int MarkovChainLength { get; set; } = 10;
        public double RunTime { get; set; } = 600.0; // in seconds

        public AnnealingOptimizationParamContextInjector(
            double i_STUDENT_CONFLICT_PENALTY, 
            double i_COURSE_SEPARATION_PENALTY, 
            double i_NON_CONSECUTIVE_SHIFT_PENALTY, 
            double i_SAME_DAY_EXAM_PENALTY, 
            double i_CONSECUTIVE_DAY_EXAM_PENALTY, 
            double i_STUDENT_YEAR_PREFERENCE_PENALTY,
            bool i_runAnnealing,
            double i_temperature,
            double i_temperatureDecrement,
            double i_terminateTemperature,
            int i_markovChainLength,
            double i_runTime)
        {
            I_STUDENT_CONFLICT_PENALTY = i_STUDENT_CONFLICT_PENALTY;
            I_COURSE_SEPARATION_PENALTY = i_COURSE_SEPARATION_PENALTY;
            I_NON_CONSECUTIVE_SHIFT_PENALTY = i_NON_CONSECUTIVE_SHIFT_PENALTY;
            I_SAME_DAY_EXAM_PENALTY = i_SAME_DAY_EXAM_PENALTY;
            I_CONSECUTIVE_DAY_EXAM_PENALTY = i_CONSECUTIVE_DAY_EXAM_PENALTY;
            I_STUDENT_YEAR_PREFERENCE_PENALTY = i_STUDENT_YEAR_PREFERENCE_PENALTY;
            RunAnnealing = i_runAnnealing;
            Temperature = i_temperature;
            TemperatureDecrement = i_temperatureDecrement;
            TerminateTemperature = i_terminateTemperature;
            MarkovChainLength = i_markovChainLength;
            RunTime = i_runTime;
        }

        protected override void Run()
        {
            Context.I_STUDENT_CONFLICT_PENALTY = I_STUDENT_CONFLICT_PENALTY;
            Context.I_COURSE_SEPARATION_PENALTY = I_COURSE_SEPARATION_PENALTY;
            Context.I_NON_CONSECUTIVE_SHIFT_PENALTY = I_NON_CONSECUTIVE_SHIFT_PENALTY;
            Context.I_SAME_DAY_EXAM_PENALTY = I_SAME_DAY_EXAM_PENALTY;
            Context.I_STUDENT_YEAR_PREFERENCE_PENALTY = I_STUDENT_YEAR_PREFERENCE_PENALTY;
            Context.I_CONSECUTIVE_DAY_EXAM_PENALTY = I_CONSECUTIVE_DAY_EXAM_PENALTY;
            
            // Set both course and shift annealing properties
            Context.I_A_useSimulatedAnnealingCourse = RunAnnealing;
            Context.I_A_temperatureCourse = Temperature;
            Context.I_A_temperature_decrementCourse = TemperatureDecrement;
            Context.I_A_terminate_temperatureCourse = TerminateTemperature;
            Context.I_A_markovChain_lengthCourse = MarkovChainLength;
            
            Context.I_A_useSimulatedAnnealingShift = RunAnnealing;
            Context.I_A_temperatureShift = Temperature;
            Context.I_A_temperature_decrementShift = TemperatureDecrement;
            Context.I_A_terminate_temperatureShift = TerminateTemperature;
            Context.I_A_markovChain_lengthShift = MarkovChainLength;

            Context.I_timeBreakSeconds = RunTime;
        }
    }
}
