using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators;
using SapLichThiAutomatic;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using SapLichThiStream;
using SapLichThiStream.Reader;
using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiAlgorithm.AlgorithmsObjects; // Added for AlgorithmContext
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove; // Added for NeighborhoodContext
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SapLichThiWebConsole
{
    internal class Program
    {
        static List<string> datasets = new List<string>
        {
            "car-f-92",
            "car-s-91",
            "ear-f-83",
            "hec-s-92",
            "kfu-s-93",
            "lse-f-91",
            "pur-s-93",
            "rye-s-93",
            "sta-f-83",
            "tre-s-92",
            "uta-s-92",
            "ute-s-92",
            "yor-f-83"
        };

        static List<int> timeslots = new List<int>
        {
            32,
            35,
            24,
            18,
            20,
            18,
            42,
            23,
            13,
            23,
            35,
            10,
            21
        };

        // ITC 2007 Datasets
        static List<string> itcDatasets = new List<string>
        {
            "exam_comp_set1",
            "exam_comp_set2",
            "exam_comp_set3",
            "exam_comp_set4",
            "exam_comp_set5",
            "exam_comp_set6",
            "exam_comp_set7",
            "exam_comp_set8"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Select benchmark to run:");
            Console.WriteLine("1. Toronto Benchmark");
            Console.WriteLine("2. ITC 2007 Benchmark");
            Console.WriteLine("3. Evaluate ITC 2007 Solutions");
            Console.Write("Enter your choice (1, 2 or 3): ");
            
            var key = Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            if (key.KeyChar == '1')
            {
                RunTorontoBenchmark();
            }
            else if (key.KeyChar == '2')
            {
                RunITCBenchmark();
            }
            else if (key.KeyChar == '3')
            {
                EvaluateITCSolutions();
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        static void RunTorontoBenchmark()
        {
            Dictionary<string, EvalDouble> bestResults = new Dictionary<string, EvalDouble>();
            Dictionary<string, EvalDouble> averageResults = new Dictionary<string, EvalDouble>();
            string? outputDirectory = null;

            foreach (var (dataSet, timeslot) in datasets.Zip(timeslots).Skip(0))
            {
                var bestResult = EvalDouble.MaxValue;
                var totalResult = new EvalDouble(0, 0);
                var runCount = 3;

                for (int i = 0; i < runCount; i++)
                {
                    var outputPath = $"F:\\downloads\\ExamTimetableEvaluation\\{dataSet}.sol";
                    outputDirectory ??= Path.GetDirectoryName(outputPath);
                    var classFilePath = $"E:\\SapXepLichThiPythonInput\\toronto_benchmark\\converted\\{dataSet}.crs.csv";
                    var studentFilePath = $"E:\\SapXepLichThiPythonInput\\toronto_benchmark\\converted\\{dataSet}.stu.csv";

                    GeneralStreamCsvInput generalExamInput = new GeneralStreamCsvInput(studentFilePath, classFilePath) { };
                    generalExamInput.LoadClasses();
                    generalExamInput.LoadStudent();
                    var model = new SchedulingModel
                    {
                        Periods = Enumerable.Range(0, timeslot).Select(tsl => new Period(tsl, DateOnly.MinValue, tsl, 100000)).ToList(),
                        UseAnnealing = true,
                        UseExamClass = true,
                        MaximumPercentage = 0.55f,
                        HardConstraints = new()
                        {
                            HardConstraint_DifferentRoomForCourses = false,
                            HardConstraint_LimitedCapacity = false,
                            HardConstraint_NoStudentConflict = true
                        }
                    };
                    AutomaticProcess automaticProcess = new AutomaticProcess(generalExamInput, model);
                    automaticProcess.RunProcess();
                    var finalSchedule = automaticProcess.Process.Context.I_lake;

                    var currentResult = automaticProcess.Process.Context.I_eval_point;
                    totalResult += currentResult;

                    // Update best result
                    if (currentResult < bestResult)
                    {
                        bestResult = currentResult;

                        // Export best schedule to file
                        if (finalSchedule != null)
                        {
                            ExportScheduleToFile(finalSchedule, outputPath);
                        }
                    }

                    Console.WriteLine($"{dataSet} - Run {i + 1}/{runCount}: {currentResult}");
                }

                // Record best and average results
                bestResults[dataSet] = bestResult;
                averageResults[dataSet] = totalResult * (1 / (double)runCount);
            }

            if (outputDirectory == null)
            {
                outputDirectory = Directory.GetCurrentDirectory();
            }

            var summaryBuilder = new StringBuilder();
            summaryBuilder.AppendLine("Dataset\tTimeslots\tBest\tAverage");
            summaryBuilder.AppendLine(new string('=', 64));
            foreach (var (dataSet, timeslot) in datasets.Zip(timeslots))
            {
                summaryBuilder.AppendLine($"{dataSet}\t{timeslot}\t{bestResults[dataSet]}\t{averageResults[dataSet]}");
            }

            var summaryPath = Path.Combine(outputDirectory, "best_results.txt");
            File.WriteAllText(summaryPath, summaryBuilder.ToString());
            Console.WriteLine($"Benchmarks completed. Summary saved to {summaryPath}");
        }

        static void RunITCBenchmark()
        {
            Dictionary<string, EvalDouble> bestResults = new Dictionary<string, EvalDouble>();
            Dictionary<string, EvalDouble> averageResults = new Dictionary<string, EvalDouble>();
            
            // To store V_ counters for the best run of each dataset
            // Key: dataset, Value: string representation of V_ counters
            Dictionary<string, string> bestRunDetails = new Dictionary<string, string>();
            
            string? outputDirectory = null;

            Console.WriteLine("Running ITC 2007 Benchmark...");

            foreach (var dataSet in itcDatasets)
            {
                var bestResult = EvalDouble.MaxValue;
                string bestResultDetails = "";
                var totalResult = new EvalDouble(0, 0);
                var runCount = 3;
                
                // Construct paths - assuming similar structure
                var outputPath = $"F:\\downloads\\ExamTimetableEvaluation\\ITC2007\\{dataSet}.sol";
                outputDirectory ??= Path.GetDirectoryName(outputPath);
                if (outputDirectory != null && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Input path - user might need to adjust this location
                var inputPath = $"E:\\SapXepLichThiPythonInput\\ITC2007\\{dataSet}.exam";

                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"Skipping {dataSet}: File not found at {inputPath}");
                    continue;
                }

                for (int i = 0; i < runCount; i++)
                {
                    // Redirect Logger to a file specific to this dataset and run
                    string logFileName = $"{dataSet}_run_{i+1}.txt";
                    string logPath = Path.Combine(outputDirectory ?? "Outputs", "Logs", logFileName); // e.g. Outputs/Logs/exam_comp_set1_run_1.txt
                    Logger.SetDataFilePath(logPath);
                    Logger.LogMessage($"Starting {dataSet} run {i+1}");

                    // Initialize ITC reader (loads data in constructor)
                    // It implements both IDataSource and ISchedulingModel
                    ITC2007ExamReader itcReader = new ITC2007ExamReader(inputPath);
                    
                    // Use reader as both data source and model
                    AutomaticProcess automaticProcess = new AutomaticProcess(itcReader, itcReader);
                    automaticProcess.RunProcess();
                    
                    var finalSchedule = automaticProcess.Process.Context.I_lake;
                    var currentResult = automaticProcess.Process.Context.I_eval_point;
                    totalResult += currentResult;

                    // Get detailed V_ counters for this run
                    string details = "";
                    if (finalSchedule != null)
                    {
                        var evaluator = new ITC2007Evaluator(finalSchedule);
                        evaluator.SetContext(automaticProcess.Process.Context);
                        evaluator.CalculateCost();
                        var vals = evaluator.GetEval();
                        var cols = evaluator.GetColumn();
                        details = string.Join("\t", vals);
                    }

                    // Update best result
                    if (currentResult < bestResult)
                    {
                        bestResult = currentResult;
                        bestResultDetails = details;

                        // Export best schedule to file
                        if (finalSchedule != null)
                        {
                            ExportScheduleToFile(finalSchedule, outputPath);
                        }
                    }

                    Console.WriteLine($"{dataSet} - Run {i + 1}/{runCount}: {currentResult}");
                }

                // Record best and average results 
                bestResults[dataSet] = bestResult;
                averageResults[dataSet] = totalResult * (1 / (double)runCount);
                if (!string.IsNullOrEmpty(bestResultDetails))
                {
                    bestRunDetails[dataSet] = bestResultDetails;
                }
            }

            if (outputDirectory == null)
            {
                // Fallback if no datasets ran
                outputDirectory = Directory.GetCurrentDirectory(); 
            }

            var summaryBuilder = new StringBuilder();
            
            // Get columns from a dummy evaluator to build header
            string[] headerCols = new string[0];
            if (itcDatasets.Count > 0)
            {
                // Just create one to get column names
                var dummyLake = new Lake(new List<Pond>());
                var dummyEval = new ITC2007Evaluator(dummyLake);
                headerCols = dummyEval.GetColumn();
            }
            
            summaryBuilder.Append("Dataset\tBest\tAverage");
            if (headerCols.Length > 0)
            {
                summaryBuilder.Append("\t" + string.Join("\t", headerCols));
            }
            summaryBuilder.AppendLine();
            summaryBuilder.AppendLine(new string('=', 64 + headerCols.Length * 10));

            foreach (var dataSet in itcDatasets)
            {
                if (bestResults.ContainsKey(dataSet))
                {
                    summaryBuilder.Append($"{dataSet}\t{bestResults[dataSet]}\t{averageResults[dataSet]}");
                    
                    if (bestRunDetails.ContainsKey(dataSet))
                    {
                        summaryBuilder.Append("\t" + bestRunDetails[dataSet]);
                    }
                    
                    summaryBuilder.AppendLine();
                }
            }

            var summaryPath = Path.Combine(outputDirectory, "best_results_itc.txt");
            File.WriteAllText(summaryPath, summaryBuilder.ToString());
            Console.WriteLine($"ITC Benchmarks completed. Summary saved to {summaryPath}");
        }

        static void EvaluateITCSolutions()
        {
            Console.WriteLine("Evaluating ITC 2007 Solutions...");
            
            foreach (var dataSet in itcDatasets)
            {
                // Input path
                var inputPath = $"E:\\SapXepLichThiPythonInput\\ITC2007\\{dataSet}.exam";
                // Solution path
                var solutionPath = $"F:\\downloads\\ExamTimetableEvaluation\\ITC2007\\{dataSet}.sol";

                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"Skipping {dataSet}: Input file not found at {inputPath}");
                    continue;
                }
                if (!File.Exists(solutionPath))
                {
                    Console.WriteLine($"Skipping {dataSet}: Solution file not found at {solutionPath}");
                    continue;
                }

                try 
                {
                    // 1. Load Data
                    ITC2007ExamReader itcReader = new ITC2007ExamReader(inputPath);
                    ITC2007Evaluator.RegisterReader( SapLichThiCore.DataType.InputDataType.ITC,itcReader);
                    
                    // 2. Prepare Context (similar to AutomaticProcess setup but without running algorithms)
                    // We need a dummy AutomaticProcess to set up the context easily or manually build it.
                    // Doing it manually to ensure we just load data + apply schedule.
                    
                    var periods = itcReader.GetPeriods();
                    var rooms = itcReader.GetRooms();

                    // Create basic context structure
                    var context = new AlgorithmContext
                    {
                        I_inputDataType = SapLichThiCore.DataType.InputDataType.ITC,
                        I_allExamClasses = itcReader.GetAllExamClasses(),
                        I_students = itcReader.GetStudents(),
                        I_periods = itcReader.GetPeriods(), // Added required
                        I_allRooms = itcReader.GetRooms(), // Added required
                        I_spareRooms = new List<Room>(), // Added required dummy
                        I_prioritizedRooms = new List<Room>(), 
                        I_largeAndMediumRooms = rooms, // Added required dummy (assume all are relevant)
                        I_largeAndMediumRoomCount = rooms.Count, // Added required
                        I_biasTable = new int[] { 0, 1, 2, 3, 4 }, // Added required dummy
                        
                        I_examClass_validSlotsPenalties = itcReader.GetExamClassValidSlots(),
                        I_examClass_validRoomsPenalties = itcReader.GetExamClassValidRooms(),
                        I_room_validSlotsPenalties = itcReader.GetRoomValidSlots(),
                        I_slot_penalties = itcReader.GetSlotPriority(),
                        I_binaryConstraints = itcReader.GetBinaryConstraints(),
                        I_unaryConstraints = itcReader.GetUnaryConstraints()
                    };

                    // 3. Reconstruct Lake from Solution File
                    // Create empty ponds for all periods
                    var ponds = new List<Pond>();
                    
                    // Initialize Ponds
                    foreach(var period in periods)
                    {
                        var puddles = rooms.Select(r => new Puddle(period, r, new List<ExamClass>())).ToList();
                        var pond = new Pond(puddles, period, new List<ExamClass>());
                        ponds.Add(pond);
                    }
                    var lake = new Lake(ponds);
                    
                    // Load assignments from .sol file
                    var examMap = context.I_allExamClasses.ToDictionary(e => e.Id, e => e);
                    var lines = File.ReadAllLines(solutionPath);

                    // Note: The .sol file format in ExportScheduleToFile is "Id PeriodIndex RoomId". 
                    foreach (var line in lines)
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue;
                        
                        string examId = parts[0];
                        if (!int.TryParse(parts[1], out int periodIdx)) continue;
                        
                        string? roomId = parts.Length >= 3 ? parts[2] : null;

                        if (examMap.TryGetValue(examId, out var examClass))
                        {
                            var pond = lake.Ponds.FirstOrDefault(p => p.Period.Index == periodIdx);
                            if (pond != null)
                            {
                                // Try to add to a valid puddle (Room)
                                bool placed = false;
                                
                                if (roomId != null)
                                {
                                    var targetPuddle = pond.Puddles.FirstOrDefault(p => p.Room.Id == roomId);
                                    if (targetPuddle != null && targetPuddle.GetRemainingCapacity() >= Puddle.GetElementSize(examClass))
                                    {
                                        pond.TryAddElementToPond(examClass, targetPuddle);
                                        placed = true;
                                    }
                                }

                                if (!placed)
                                {
                                    // Simple First-Fit or Best-Fit into rooms of that period
                                    // We need to check validity (capacity etc) to duplicate the original assignment logic effectively?
                                    // No, just find *any* room it fits in for now to construct the state.
                                    foreach(var puddle in pond.Puddles)
                                    {
                                        // Check capacity simply
                                        if (puddle.GetRemainingCapacity() >= Puddle.GetElementSize(examClass))
                                        {
                                            pond.TryAddElementToPond(examClass, puddle);
                                            placed = true;
                                            break;
                                        }
                                    }
                                }
                                
                                if (!placed)
                                {
                                    // Could not place in any room (Capacity full?)
                                    // Force place in first room
                                    var fallbackPuddle = pond.Puddles.FirstOrDefault();
                                    if (fallbackPuddle != null)
                                    {
                                        pond.TryAddElementToPond(examClass, fallbackPuddle);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Warning: No rooms in period {periodIdx} for exam {examId}");
                                    }
                                }
                            }
                        }
                    }

                    // 4. Evaluate
                    context.I_lake = lake;
                    
                    // Need a neighborhood context for evaluator
                    var neighborhoodContext = NeighborhoodContext.FromAlgorithmContextAndLake(context, lake);
                    
                    var evaluator = new ITC2007Evaluator(lake);
                    evaluator.SetContext(context, neighborhoodContext);
                    
                    EvalDouble cost = evaluator.CalculateCost();
                    var vals = evaluator.GetEval();
                    var cols = evaluator.GetColumn();
                    
                    Console.WriteLine($"\n--- Evaluation for {dataSet} ---");
                    for(int k=0; k<cols.Length; k++)
                    {
                         Console.WriteLine($"{cols[k]}: {vals[k]}");
                    }
                    Console.WriteLine($"Total Cost: {cost}");
                    Console.WriteLine("------------------------------------------");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error evaluating {dataSet}: {ex.Message}");
                }
            }
        }

        static void ExportScheduleToFile(Lake schedule, string outputPath)
        {
            using (var writer = new StreamWriter(outputPath))
            {
                foreach (var pond in schedule.Ponds)
                {
                    var periodIndex = pond.Period.Index;
                    foreach (var puddle in pond.Puddles)
                    {
                        var room = puddle.Room;
                        foreach (var examClass in puddle.Elements)
                        {
                            writer.WriteLine($"{examClass.Id} {periodIndex} {room.Id}");
                        }
                    }
                }
            }
        }
    }
}
