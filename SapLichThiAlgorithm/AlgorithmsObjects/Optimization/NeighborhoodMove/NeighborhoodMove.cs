using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove
{
    public interface INeighborhoodMove
    {
        public bool CreateNeighbor();
        public void RevertNeighbor();
        public void UpdateOldResource();
        public ClassMoveInfos? GetMoveResults();
        internal INeighborhoodMove IncludeContext( AlgorithmContext algorithmContext, NeighborhoodContext neighborhoodContext);
        internal INeighborhoodMove IncludeIndependentLake(Lake lake);
    }
}
