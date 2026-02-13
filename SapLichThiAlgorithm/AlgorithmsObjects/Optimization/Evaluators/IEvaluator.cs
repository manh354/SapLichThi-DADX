using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiCore.DataObjects;
using System.Numerics;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    public interface IEvaluator
    {
        EvalDouble CalculateCost(bool print = false);
        EvalDouble CalculateDiffCost(EvalDouble currentCost, ClassMoveInfos classMoveInfos);
        void Check();
        IEvaluator SetContext(AlgorithmContext context);
        IEvaluator SetContext(AlgorithmContext context, NeighborhoodContext neighborhoodContext);
        double[] GetEval();
        string[] GetColumn();
    }
}