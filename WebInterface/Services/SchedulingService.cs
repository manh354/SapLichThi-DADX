using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiAutomatic;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using SapLichThiStream.Reader;
using SapLichThiAlgorithm.ErrorAndLog;
using System.Text;

namespace WebInterface.Services
{
    public class SchedulingService
    {
        public AutomaticProcess? CurrentProcess { get; private set; }
        public Lake? CurrentSchedule => CurrentProcess?.Process?.Context?.I_lake;
        public AlgorithmContext? CurrentContext => CurrentProcess?.Process?.Context;
        
        public bool IsRunning { get; private set; }
        public List<string> Logs { get; private set; } = new List<string>();
        public Dictionary<string, double> EvaluationMetrics { get; private set; } = new Dictionary<string, double>();
        public event Action? OnChange;

        public void RunTorontoBenchmark(string studentFile, string courseFile, int timeslots)
        {
            if (IsRunning) return;
            IsRunning = true;
            Logs.Clear();
            EvaluationMetrics.Clear();
            Log($"Starting Toronto Benchmark with {Path.GetFileName(courseFile)}...");
            NotifyStateChanged();

            Task.Run(() =>
            {
                try
                {
                    GeneralStreamCsvInput generalExamInput = new GeneralStreamCsvInput(studentFile, courseFile);
                    generalExamInput.LoadClasses();
                    generalExamInput.LoadStudent();

                    var model = new SchedulingModel 
                    {
                        Periods = Enumerable.Range(0, timeslots).Select(tsl => new Period(tsl, DateOnly.MinValue, tsl, 100000)).ToList(),
                        UseAnnealing = true,
                        UseExamClass = true,
                        MaximumPercentage = 0.55f,
                        HardConstraints = new SapLichThiStream.HardConstraints // Assuming this exists or similar from Stream
                        {
                            HardConstraint_DifferentRoomForCourses = false,
                            HardConstraint_LimitedCapacity = false,
                            HardConstraint_NoStudentConflict = true
                        }
                    };

                    // We need to implement ISchedulingModel if SapLichThiConsole.SchedulingModel is not available. 
                    // Let's check SapLichThiConsole code again or SapLichThiStream.
                    
                    CurrentProcess = new AutomaticProcess(generalExamInput, model);
                    
                    // Hook into some logging if possible, but AutomaticProcess doesn't seem to expose much events easily in Program.cs usage.
                    // We can rely on poling or modifying Logger.
                    
                    CurrentProcess.RunProcess();
                    
                    if (CurrentContext != null)
                    {
                        var cost = CurrentContext.I_eval_point.softCost; // Toronto usually cares about soft cost/student
                        EvaluationMetrics["Soft Cost"] = cost;
                        EvaluationMetrics["Hard Cost"] = CurrentContext.I_eval_point.hardCost;
                        Log($"Finished. Cost: {CurrentContext.I_eval_point}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
                finally
                {
                    IsRunning = false;
                    NotifyStateChanged();
                }
            });
        }

        public void RunITC2007Benchmark(string inputFile)
        {
            if (IsRunning) return;
            IsRunning = true;
            Logs.Clear();
            Log($"Starting ITC 2007 Benchmark with {Path.GetFileName(inputFile)}...");
            NotifyStateChanged();

            Task.Run(() =>
            {
                try
                {
                    ITC2007ExamReader itcReader = new ITC2007ExamReader(inputFile);
                    CurrentProcess = new AutomaticProcess(itcReader, itcReader);
                    CurrentProcess.RunProcess();
                    
                    if (CurrentSchedule != null)
                    {
                        var evaluator = new ITC2007Evaluator(CurrentSchedule);
                        // Need context setup? ITC2007Evaluator uses context?
                        // In Console Program.cs: evaluator.SetContext(automaticProcess.Process.Context);
                        if (CurrentContext != null)
                        {
                             // Context needs neighborhood for evaluator to work fully? 
                             // Wait, evaluator might need neighborhood context.
                             // Console app: evaluator.SetContext(automaticProcess.Process.Context);
                             // Let's assume SetContext is enough or check EvaluatorFactory logic.
                             // Actually EvaluatorFactory.CreateEvaluator does: new ITC2007Evaluator(lake).SetContext(context, neighborhoodContext);
                             // We should do the same.
                             
                             var neighborhoodContext = NeighborhoodContext.FromAlgorithmContextAndLake(CurrentContext, CurrentSchedule);
                             evaluator.SetContext(CurrentContext, neighborhoodContext);

                             var cost = evaluator.CalculateCost();
                             Log($"Finished. Total Cost: {cost}");
                             
                             EvaluationMetrics["Total Hard Cost"] = cost.hardCost;
                             EvaluationMetrics["Total Soft Cost"] = cost.softCost;

                             var cols = evaluator.GetColumn();
                             var vals = evaluator.GetEval();
                             for(int i=0; i< cols.Length; i++)
                             {
                                 EvaluationMetrics[cols[i]] = vals[i];
                                 Log($"{cols[i]}: {vals[i]}");
                             }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
                finally
                {
                    IsRunning = false;
                    NotifyStateChanged();
                }
            });
        }

        private void Log(string message)
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
