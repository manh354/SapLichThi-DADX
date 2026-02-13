using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.GeneralScheduling;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.AdditionalStructure;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Annealing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.Annealing
{
    public class APAnnealer : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Dãn cách lịch thi.";
        }
        protected override void BuildSubProcesses()
        {/*
            AddSubProcess(new APShiftAnnealing().SetContext(Context));
            AddSubProcess(new APCourseAnnealing().SetContext(Context));*/
        }

        protected override async Task BeforeSubprocessesAsync()
        {
            var (studentConflictPenalty, courseSeparationPenalty, nonConsecutiveShiftPenalty, sameDayExamPenalty, consecutiveDayExamPenalty, studentYearPreferencePenalty, runAnnealing, temperature, temperatureDecrement, terminateTemperature, markovChainLength, runTime) =
                    (APAnnealerQAndA)InputQAndAs.First();

            new AnnealingOptimizationParamContextInjector(studentConflictPenalty, courseSeparationPenalty, nonConsecutiveShiftPenalty, sameDayExamPenalty, consecutiveDayExamPenalty, studentYearPreferencePenalty, runAnnealing, temperature, temperatureDecrement, terminateTemperature, markovChainLength, runTime)
                .SetContext(Context).Run();

            // new GeneticMain().SetContext(Context).Run();
            new AnnealingMain().SetContext(Context).Run();
        }

        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            return new()
            {
                InputQAndAs = [new APAnnealerQAndA() { }]
            };
        }

        protected override ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context)
        {
            return null;
        }
    }


    public class APAnnealerQAndA : BaseInputQAndA
    {

        public double I_STUDENT_CONFLICT_PENALTY { get; set; } = 1000.0;
        public double I_COURSE_SEPARATION_PENALTY { get; set; } = 5.0;
        public double I_NON_CONSECUTIVE_SHIFT_PENALTY { get; set; } = 2.0;
        public double I_SAME_DAY_EXAM_PENALTY { get; set; } = 8.0;
        public double I_CONSECUTIVE_DAY_EXAM_PENALTY { get; set; } = 1.0;
        public double I_STUDENT_YEAR_PREFERENCE_PENALTY { get; set; } = 0.25;

        public bool RunAnnealing { get; set; } = true;
        public double Temperature { get; set; } = 10f;
        public double TemperatureDecrement { get; set; } = 0.95;
        public double TerminateTemperature { get; set; } = 0.1;
        public int MarkovChainLength { get; set; } = 10;
        public double RunTime { get; set; } = 600;
        
        public void Deconstruct(out double studentConflictPenalty, out double courseSeparationPenalty, out double nonConsecutiveShiftPenalty, out double sameDayExamPenalty, out double consecutiveDayExamPenalty, out double studentYearPreferencePenalty, out bool runAnnealing, out double temperature, out double temperatureDecrement, out double terminateTemperature, out int markovChainLength, out double runTime)
        {
            studentConflictPenalty = I_STUDENT_CONFLICT_PENALTY;
            courseSeparationPenalty = I_COURSE_SEPARATION_PENALTY;
            nonConsecutiveShiftPenalty = I_NON_CONSECUTIVE_SHIFT_PENALTY;
            sameDayExamPenalty = I_SAME_DAY_EXAM_PENALTY;
            consecutiveDayExamPenalty = I_CONSECUTIVE_DAY_EXAM_PENALTY;
            studentYearPreferencePenalty = I_STUDENT_YEAR_PREFERENCE_PENALTY;
            runAnnealing = RunAnnealing;
            temperature = Temperature;
            temperatureDecrement = TemperatureDecrement;
            terminateTemperature = TerminateTemperature;
            markovChainLength = MarkovChainLength;
            runTime = RunTime;
        }
    }
}
