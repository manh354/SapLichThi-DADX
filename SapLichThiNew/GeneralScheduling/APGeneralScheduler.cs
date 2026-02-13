using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.GeneralScheduling;
using SapLichThiNew.GeneralScheduling.Postprocessings;
using SapLichThiNew.GeneralScheduling.SmallCourseScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.GeneralScheduling
{
    public class APGeneralScheduler : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Xếp lịch các môn còn lại.";
        }
        protected override void BuildSubProcesses()
        {
            // AddSubProcess(new APBigCourseScheduler().SetContext(Context));
            AddSubProcess(new APSmallCourseScheduler().SetContext(Context));
            AddSubProcess(new APGeneralPostProcessor().SetContext(Context));
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
