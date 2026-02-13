using SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds.Coloring;
using SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiAlgorithm.AlgorithmsObjects;

namespace SapLichThiNew.StructuralBuildings
{
    public class APStructuralBuilder : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Xây dựng các cấu trúc dữ liệu ; tìm kiếm môn thi có kích thước lớn quá 1 kíp.";
        }
        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            // No input required;
            return null;
        }
        protected override async Task BeforeSubprocessesAsync()
        {
            await Task.Run(() =>
            {

                new ClassGraphFiller()
                .SetContext(Context).Run();

                new ClassGraphColorer()
                .SetContext(Context).Run();

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
    }
}
