using AlgorithmExtensions;
using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiAlgorithm.Extensions;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Annealing
{
    public class AnnealingMain : BaseAlgorithmObject
    {
        public double I_timeBreakSeconds { get; set; } = 600;
        public int I_patient { get; set; } = 5;
        public int I_disappointment { get; set; } = 50;
        public double P_ellapsedTime { get; set; } = 0; // in seconds
        public EvalDouble P_bestCost { get; set; } = new(double.MaxValue, double.MaxValue);
        public double I_temperature { get; set; } = 1f;
        public double I_temperature_decrement { get; set; } = 0.99;
        public double I_terminate_temperature { get; set; } = 0.1;
        public int I_markovChain_length { get; set; } = 10;

        public Lake I_lake { get; set; }
        public Lake P_bestSol { get; set; }

        protected override void InitializeAllOutput()
        {
            // No output initialized
        }

        protected override void ProcedureRun()
        {
            RunAnnealing();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {

            I_timeBreakSeconds = context.I_timeBreakSeconds;
            I_patient = context.I_patient;
            I_disappointment = I_patient / 5;
            I_temperature = Context.I_A_temperatureShift;
            I_temperature_decrement = Context.I_A_temperature_decrementShift;
            I_terminate_temperature = Context.I_A_terminate_temperatureShift;
            I_markovChain_length = Context.I_A_markovChain_lengthShift;

            I_lake = Context.I_lake;
            P_bestSol = I_lake.DeepCloneLake();
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            Context.I_lake = P_bestSol;
            Context.I_eval_point = P_bestCost;
        }

        private void RunAnnealing()
        {
            if (!Context.I_A_useSimulatedAnnealingCourse || !Context.I_A_useSimulatedAnnealingShift)
            {
                return;
            }

            neighborhoodMoves_point = MoveListFactory.CreateNeighborhoodMoveWeight(Context);

            var sw = Stopwatch.StartNew();
            double temperature = I_temperature;
            var countRecurPoint = 0;
            var countDissapointment = 0;

            var curLake = I_lake.DeepCloneLake();
            P_bestSol = I_lake;

            NeighborhoodContext neighborhoodContext = NeighborhoodContext.FromAlgorithmContextAndLake(Context, curLake);
            IEvaluator evaluator = EvaluatorFactory.CreateEvaluator(Context, neighborhoodContext, curLake);

            EvalDouble currentCost = evaluator.CalculateCost();
            EvalDouble bestCost = currentCost;
            P_bestCost = bestCost;
            Logger.LogMessage($"Starting Simulated Annealing - Initial Cost: {currentCost}", LogType.Info);
            EvalDouble dissapointMentCost = currentCost;
            int moveCreated = 0;
            while (true)
            {
                EvalDouble oldCost = currentCost;

                INeighborhoodMove neighborhoodMove = CreateNeighborhoodMove(Context, neighborhoodContext).IncludeIndependentLake(curLake);
                for (int iteration = 0; iteration < I_markovChain_length; iteration++, moveCreated++)
                {
                    //evaluator.Check(); // Check the internal state after each swap
                    var createSuccess = neighborhoodMove.CreateNeighbor();
                    var moveResults = neighborhoodMove.GetMoveResults();
                    // evaluator.Check(); // Check the internal state after each swap

                    if (!createSuccess)
                        continue;

                    EvalDouble newCost = evaluator.CalculateCost();
                    
                    //EvalDouble newCost = evaluator.CalculateDiffCost(currentCost, moveResults);

                    EvalDouble deltaCost = newCost - currentCost;

                    bool accept = false;
                    if (deltaCost < 0)
                    {
                        accept = true;
                        // Logger.LogMessage($"Better solution found - Cost: {newCost} (Delta: {deltaCost})", LogType.Info);
                    }
                    else
                    {
                        double probability = deltaCost.hardCost > 0 ? Math.Exp(-1000 * deltaCost.hardCost / temperature) : Math.Exp(-deltaCost.softCost / temperature);
                        accept = RandomExtension.ChooseProbability(probability);

                    }

                    if (accept)
                    {
                        currentCost = newCost;
                        if (newCost < bestCost)
                        {
                            bestCost = newCost;
                            P_bestCost = bestCost;
                            // Logger.LogMessage($"New best solution found - Cost: {bestCost}", LogType.Info);
                            SaveBestSolution(curLake);
                            UpdateNeighborhoodPoint(neighborhoodMove, (-deltaCost.hardCost * 1000 - deltaCost.softCost * 1), 0.975);
                        }
                        neighborhoodMove.UpdateOldResource();

                    }
                    else
                    {
                        neighborhoodMove.RevertNeighbor();
                        // Logger.LogMessage("Revert", LogType.Warning);
                        // currentCost = evaluator.CalculateCost();
                        UpdateNeighborhoodPoint(0.99);
                    }
                    double[] vals = [currentCost.hardCost, currentCost.softCost, bestCost.hardCost, bestCost.softCost, .. evaluator.GetEval()];
                    string[] names = ["CurrentHardCost", "CurrentSoftCost", "BestHardCost", "BestSoftCost", .. evaluator.GetColumn()];
                    Logger.LogData(vals, names);
                }

                temperature *= I_temperature_decrement;
                if (temperature < 0.001)
                {
                    temperature = 0.001;
                }
                Logger.LogMessage($"Temperature: {temperature:F4}, Current Cost: {currentCost}, Best Cost: {bestCost}", LogType.Debug);
                if (currentCost == oldCost)
                {
                    countRecurPoint += 1;
                }
                else
                {
                    countRecurPoint = 0;
                }

                P_ellapsedTime = sw.ElapsedMilliseconds * 0.001;

                if (P_ellapsedTime >= I_timeBreakSeconds)
                {
                    break;
                }
                else if (countRecurPoint >= I_patient && P_ellapsedTime < I_timeBreakSeconds)
                {
                    if ((bestCost - oldCost).softCost < 0.01 * bestCost.softCost)
                    {
                        countDissapointment += 1;
                    }
                    if (countDissapointment >= I_disappointment)
                    {
                        temperature = -currentCost.softCost * 0.01 / Math.Log(0.1);
                        countDissapointment = 0;
                        foreach (var key in neighborhoodMoves_point.Keys)
                        {
                            neighborhoodMoves_point[key] = 1;
                        }
                        I_patient *= 2;
                    }
                    else
                    {
                        temperature = I_temperature;
                    }
                    countRecurPoint = 0;
                    curLake = P_bestSol.DeepCloneLake();
                    neighborhoodContext = NeighborhoodContext.FromAlgorithmContextAndLake(Context, curLake);
                    evaluator = EvaluatorFactory.CreateEvaluator(Context, neighborhoodContext, curLake);
                }
            }

            Logger.LogMessage($"Annealing completed - Final Cost: {bestCost}, Move Created {moveCreated}", LogType.Info);
            Logger.SaveDataFile("DATA.txt");
        }

        protected void SaveBestSolution(Lake lake)
        {
            P_bestSol = lake.DeepCloneLake();
        }

        Dictionary<INeighborhoodMove, double> neighborhoodMoves_point;

        private INeighborhoodMove CreateNeighborhoodMove(AlgorithmContext algorithmContext, NeighborhoodContext annealingContext)
        {
            return neighborhoodMoves_point.SelectByWeight().IncludeContext(algorithmContext, annealingContext);
        }

        private void UpdateNeighborhoodPoint(INeighborhoodMove neighborhoodMove, double pointMoved, double multiplyCoef)
        {
            var keys = neighborhoodMoves_point.Keys.ToList();
            foreach (var key in keys)
            {
                var curPoint = neighborhoodMoves_point[key] *= multiplyCoef;
                if (curPoint < 1)
                {
                    neighborhoodMoves_point[key] = 1;
                }
                if (double.IsNaN(curPoint))
                {
                    neighborhoodMoves_point[key] = 1;
                }
                if (curPoint < 0)
                {
                    neighborhoodMoves_point[key] = 10000;
                }
            }
            neighborhoodMoves_point[neighborhoodMove] += pointMoved;

        }

        private void UpdateNeighborhoodPoint(double multiplyCoef)
        {
            var keys = neighborhoodMoves_point.Keys.ToList();
            foreach (var key in keys)
            {
                var curPoint = neighborhoodMoves_point[key] *= multiplyCoef;
                if (curPoint < 1)
                {
                    neighborhoodMoves_point[key] = 1;
                }
            }

        }
    }
}
