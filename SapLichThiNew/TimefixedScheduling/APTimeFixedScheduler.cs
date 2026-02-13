using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.GeneralScheduling;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.TimefixedScheduling
{
    public class APTimeFixedScheduler : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Xếp các nhóm môn / dải môn.";
        }
        protected override async Task BeforeSubprocessesAsync()
        {
            
            GeneralSchedulingSettings.MaximumPercentage = Context.I_optimalRoomCoef;


        }

        protected override void BuildSubProcesses()
        {
            return;
        }

        protected override ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context)
        {
            return null;
        }

        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            return null;
        }


    }

}
