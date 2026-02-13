using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.GeneralScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.GeneralScheduling.SmallCourseScheduling
{
    public class APSmallCourseScheduler : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Xếp các môn học vừa trong 1 kíp.";
        }
        protected override void BuildSubProcesses()
        {
            // No subprocess available.
        }


        protected override Task BeforeSubprocessesAsync()
        {
            return Task.Run(() =>
            {
                new GeneralScheduler()
                .SetContext(Context).Run();
                
            });
        }

        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            return null;
        }

        protected override ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context)
        {
            return null;
        }
    }
}
