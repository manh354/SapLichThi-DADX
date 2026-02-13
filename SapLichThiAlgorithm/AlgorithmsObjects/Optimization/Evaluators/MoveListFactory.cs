using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove.TorontoNeighborhoodMove;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators
{
    public class MoveListFactory
    {
        public static Dictionary<INeighborhoodMove, double> CreateNeighborhoodMoveWeight(AlgorithmContext context)
        {
            switch (context.I_inputDataType)
            {
                case InputDataType.ITC:
                    return new Dictionary<INeighborhoodMove, double>() {
                        {new RoomMove(), 1.0 },
                        {new SingleExamClassMove(), 1.0 },
                        {new KempeChainMove(), 10.0 }, // Added KempeChainMove for ITC as well for testing
                        {new RoomPeriodSwapMove(), 1 },
                    };
                case InputDataType.Toronto:
                    return new Dictionary<INeighborhoodMove, double>() {
                         {new SingleExamClassMove(), 1.0 },
                         {new ShiftSwapMove(), 100 },
                         {new KempeChainMove(), 10.0 },
                    };
                default:
                    return new Dictionary<INeighborhoodMove, double>()
                    {

                    };
            }
        }

    }
}
