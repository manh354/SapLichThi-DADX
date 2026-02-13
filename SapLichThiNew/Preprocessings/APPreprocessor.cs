using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.AdditionalStructure;
using SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.Preprocessings
{
    public class APPreprocessor : AlgoProcess
    {

        protected override void BuildSubProcesses()
        {
            // No sub-process is required for Preprocessing
            return;
        }

        public override string GetProcessName()
        {
            return "Tiền xử lý và chuẩn hóa dữ liệu.";
        }

        protected override async Task BeforeSubprocessesAsync()
        {
            await Task.Run(() =>
            {
                new ClassesGrouper()
                   .SetContext(Context).Run();

                new CourseLinkageByCommonStudent()
                    .SetContext(Context).Run();

                new RoomSeperator()
                    .SetContext(Context).Run();

                new ClassRoomObjectBuilder()
                    .SetContext(Context).Run();

                new StudentExamClassesMapCreator()
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
