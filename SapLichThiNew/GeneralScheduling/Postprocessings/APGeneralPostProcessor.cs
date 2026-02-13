using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.GeneralScheduling.Postprocessings
{
    public class APGeneralPostProcessor : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Tổng hợp kết quả.";
        }
        protected override async Task BeforeSubprocessesAsync()
        {
            await Task.Run(() =>
            {
            });
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
