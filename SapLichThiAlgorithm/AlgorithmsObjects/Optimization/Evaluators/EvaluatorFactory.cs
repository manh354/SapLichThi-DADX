using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    public class EvaluatorFactory
    {

        public static IEvaluator CreateEvaluator(AlgorithmContext context, NeighborhoodContext neighborhoodContext, Lake lake)
        {
            switch (context.I_inputDataType)
            {
                case InputDataType.Toronto:
                    return new TorontoEvaluator(lake).SetContext(context, neighborhoodContext);
                case InputDataType.ITC:
                    return new ITC2007Evaluator(lake).SetContext(context, neighborhoodContext);
                default:
                    return null;
            }
        }
    }
}
